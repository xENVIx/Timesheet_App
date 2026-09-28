using System.Collections.Concurrent;
using Dapper;
using LibSqlLite.Schema;
using LibSqlLite.TypeHandlers;
using Microsoft.Data.Sqlite;

namespace LibSqlLite;

/// <summary>
/// Persists plain C# classes to a local SQLite database. Tables and columns are derived from
/// class shape via reflection; new properties become new columns automatically. See the
/// project's CLAUDE.md for the full class-to-table mapping conventions.
/// </summary>
public sealed class SqliteStore : IDisposable
{
    private readonly SqliteConnection _connection;

    // A single SemaphoreSlim guards the connection for both sync and async callers (it supports
    // both Wait() and WaitAsync() on the same instance). Using two separate primitives here -
    // e.g. a Monitor for sync and this semaphore for async - would let a sync call and an async
    // call run against the connection at the same time, since neither lock would know about the
    // other.
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ConcurrentDictionary<Type, EntityInfo> _entities = new();
    private readonly ConcurrentDictionary<Type, byte> _ensuredTables = new();

    private readonly AsyncLocal<SqliteTransaction?> _ambientTransactionLocal = new();

    // Types whose EnsureTable ran during the current ambient transaction but haven't been
    // promoted to _ensuredTables yet. If that transaction rolls back, the CREATE TABLE it ran
    // is undone too, so the type must NOT be marked ensured - otherwise every later call would
    // skip table creation and fail with "no such table". Entries here are merged into
    // _ensuredTables only when the owning top-level InTransaction/InTransactionAsync commits.
    private readonly AsyncLocal<HashSet<Type>?> _pendingEnsuredTablesLocal = new();

    private bool _disposed;

    private SqliteTransaction? _ambientTransaction
    {
        get => _ambientTransactionLocal.Value;
        set => _ambientTransactionLocal.Value = value;
    }

    private HashSet<Type>? _pendingEnsuredTables
    {
        get => _pendingEnsuredTablesLocal.Value;
        set => _pendingEnsuredTablesLocal.Value = value;
    }

    /// <summary>Opens (creating if necessary) a SQLite database at <paramref name="databasePath"/>.</summary>
    public SqliteStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        TypeHandlerRegistration.Ensure();

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Pooling is off: a store owns exactly one connection for its whole lifetime, so pooling
        // buys nothing, but it would keep the native file handle open after Dispose() (in the
        // pool, for reuse), which surprises callers who want to delete/move the file right after.
        _connection = new SqliteConnection($"Data Source={fullPath};Pooling=False");
        _connection.Open();

        using var pragma = _connection.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();
    }

    /// <summary>Opens a database at %AppData%/{appName}/{fileName}.</summary>
    public static SqliteStore ForApp(string appName, string fileName = "data.db")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName, fileName);
        return new SqliteStore(path);
    }

    /// <summary>Creates the table for <typeparamref name="T"/> if missing and adds any new columns.</summary>
    public void EnsureTable<T>() where T : class => EnsureTableCore(typeof(T));

    /// <summary>Creates the table for <typeparamref name="T"/> if missing and adds any new columns.</summary>
    public async Task EnsureTableAsync<T>(CancellationToken cancellationToken = default) where T : class =>
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);

    /// <summary>Inserts <paramref name="item"/>, writing a generated key back onto it if applicable.</summary>
    public void Insert<T>(T item) where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        RunInLock(tx => InsertCore(item, entity, tx));
    }

    /// <summary>Inserts <paramref name="item"/>, writing a generated key back onto it if applicable.</summary>
    public async Task InsertAsync<T>(T item, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        await RunInLockAsync(
            tx => InsertCoreAsync(item, entity, tx, cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Inserts all <paramref name="items"/> in a single transaction.</summary>
    public void InsertMany<T>(IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        var list = items as IReadOnlyList<T> ?? items.ToList();

        RunInLock(tx =>
        {
            if (tx is not null)
            {
                foreach (var item in list)
                {
                    InsertCore(item, entity, tx);
                }

                return;
            }

            using var localTx = _connection.BeginTransaction();
            foreach (var item in list)
            {
                InsertCore(item, entity, localTx);
            }

            localTx.Commit();
        });
    }

    /// <summary>Inserts all <paramref name="items"/> in a single transaction.</summary>
    public async Task InsertManyAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        var list = items as IReadOnlyList<T> ?? items.ToList();

        await RunInLockAsync(async tx =>
        {
            if (tx is not null)
            {
                foreach (var item in list)
                {
                    await InsertCoreAsync(item, entity, tx, cancellationToken).ConfigureAwait(false);
                }

                return;
            }

            using var localTx = (SqliteTransaction)await _connection
                .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            foreach (var item in list)
            {
                await InsertCoreAsync(item, entity, localTx, cancellationToken).ConfigureAwait(false);
            }

            await localTx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Updates the row matching <paramref name="item"/>'s key. Returns false if no row matched.</summary>
    public bool Update<T>(T item) where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        return RunInLock(tx => _connection.Execute(entity.UpdateSql, item, tx) > 0);
    }

    /// <summary>Updates the row matching <paramref name="item"/>'s key. Returns false if no row matched.</summary>
    public async Task<bool> UpdateAsync<T>(T item, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        return await RunInLockAsync(
            async tx =>
            {
                var command = new CommandDefinition(
                    entity.UpdateSql, item, tx, cancellationToken: cancellationToken);
                return await _connection.ExecuteAsync(command).ConfigureAwait(false) > 0;
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Inserts <paramref name="item"/>, or updates it by key if a matching row already exists.</summary>
    public void Upsert<T>(T item) where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        RunInLock(tx => UpsertCore(item, entity, tx));
    }

    /// <summary>Inserts <paramref name="item"/>, or updates it by key if a matching row already exists.</summary>
    public async Task UpsertAsync<T>(T item, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(item);
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        await RunInLockAsync(
            tx => UpsertCoreAsync(item, entity, tx, cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes the row with the given key. Returns false if no row matched.</summary>
    public bool Delete<T>(object id) where T : class
    {
        ArgumentNullException.ThrowIfNull(id);
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        return RunInLock(tx => _connection.Execute(entity.DeleteByIdSql, KeyParameter(id), tx) > 0);
    }

    /// <summary>Deletes the row with the given key. Returns false if no row matched.</summary>
    public async Task<bool> DeleteAsync<T>(object id, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(id);
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        return await RunInLockAsync(
            async tx =>
            {
                var command = new CommandDefinition(
                    entity.DeleteByIdSql, KeyParameter(id), tx, cancellationToken: cancellationToken);
                return await _connection.ExecuteAsync(command).ConfigureAwait(false) > 0;
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes every row of <typeparamref name="T"/>'s table and returns the row count removed.</summary>
    public int DeleteAll<T>() where T : class
    {
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        return RunInLock(tx => _connection.Execute(entity.DeleteAllSql, transaction: tx));
    }

    /// <summary>Deletes every row of <typeparamref name="T"/>'s table and returns the row count removed.</summary>
    public async Task<int> DeleteAllAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        return await RunInLockAsync(
            async tx =>
            {
                var command = new CommandDefinition(entity.DeleteAllSql, transaction: tx, cancellationToken: cancellationToken);
                return await _connection.ExecuteAsync(command).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Fetches a single row by key, or null if no row matches.</summary>
    public T? Get<T>(object id) where T : class
    {
        ArgumentNullException.ThrowIfNull(id);
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        return RunInLock(tx => _connection.QueryFirstOrDefault<T>(entity.SelectByIdSql, KeyParameter(id), tx));
    }

    /// <summary>Fetches a single row by key, or null if no row matches.</summary>
    public async Task<T?> GetAsync<T>(object id, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(id);
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        return await RunInLockAsync(
            async tx =>
            {
                var command = new CommandDefinition(
                    entity.SelectByIdSql, KeyParameter(id), tx, cancellationToken: cancellationToken);
                return await _connection.QueryFirstOrDefaultAsync<T>(command).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Fetches every row of <typeparamref name="T"/>'s table.</summary>
    public IReadOnlyList<T> All<T>() where T : class
    {
        EnsureTableCore(typeof(T));
        var entity = GetEntity(typeof(T));
        return RunInLock(tx => _connection.Query<T>(entity.SelectAllSql, transaction: tx).AsList());
    }

    /// <summary>Fetches every row of <typeparamref name="T"/>'s table.</summary>
    public async Task<IReadOnlyList<T>> AllAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        await EnsureTableCoreAsync(typeof(T), cancellationToken).ConfigureAwait(false);
        var entity = GetEntity(typeof(T));
        return await RunInLockAsync(
            async tx =>
            {
                var command = new CommandDefinition(entity.SelectAllSql, transaction: tx, cancellationToken: cancellationToken);
                var rows = await _connection.QueryAsync<T>(command).ConfigureAwait(false);
                return (IReadOnlyList<T>)rows.AsList();
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Escape hatch: runs raw SQL and maps the results with Dapper. Does not call EnsureTable.</summary>
    public IReadOnlyList<T> Query<T>(string sql, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return RunInLock(tx => _connection.Query<T>(sql, parameters, tx).AsList());
    }

    /// <summary>Escape hatch: runs raw SQL and maps the results with Dapper. Does not call EnsureTable.</summary>
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return await RunInLockAsync(
            async tx =>
            {
                var command = new CommandDefinition(sql, parameters, tx, cancellationToken: cancellationToken);
                var rows = await _connection.QueryAsync<T>(command).ConfigureAwait(false);
                return (IReadOnlyList<T>)rows.AsList();
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Escape hatch: runs raw, non-query SQL. Does not call EnsureTable.</summary>
    public int Execute(string sql, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return RunInLock(tx => _connection.Execute(sql, parameters, tx));
    }

    /// <summary>Escape hatch: runs raw, non-query SQL. Does not call EnsureTable.</summary>
    public async Task<int> ExecuteAsync(
        string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return await RunInLockAsync(
            tx =>
            {
                var command = new CommandDefinition(sql, parameters, tx, cancellationToken: cancellationToken);
                return _connection.ExecuteAsync(command);
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs <paramref name="work"/> atomically. Calls made on the passed-in store (which is this
    /// instance) enlist in the same transaction. Rolls back entirely if <paramref name="work"/> throws.
    /// </summary>
    public void InTransaction(Action<SqliteStore> work)
    {
        ArgumentNullException.ThrowIfNull(work);

        if (_ambientTransaction is not null)
        {
            work(this);
            return;
        }

        _lock.Wait();
        try
        {
            using var tx = _connection.BeginTransaction();
            _ambientTransaction = tx;
            var pendingEnsured = new HashSet<Type>();
            _pendingEnsuredTables = pendingEnsured;
            try
            {
                work(this);
                tx.Commit();
                foreach (var type in pendingEnsured)
                {
                    _ensuredTables[type] = 0;
                }
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            finally
            {
                _ambientTransaction = null;
                _pendingEnsuredTables = null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Runs <paramref name="work"/> atomically. Calls made on the passed-in store (which is this
    /// instance) enlist in the same transaction. Rolls back entirely if <paramref name="work"/> throws.
    /// </summary>
    public async Task InTransactionAsync(
        Func<SqliteStore, Task> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        if (_ambientTransaction is not null)
        {
            await work(this).ConfigureAwait(false);
            return;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tx = (SqliteTransaction)await _connection
                .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = tx.ConfigureAwait(false);
            _ambientTransaction = tx;
            var pendingEnsured = new HashSet<Type>();
            _pendingEnsuredTables = pendingEnsured;
            try
            {
                await work(this).ConfigureAwait(false);
                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                foreach (var type in pendingEnsured)
                {
                    _ensuredTables[type] = 0;
                }
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                _ambientTransaction = null;
                _pendingEnsuredTables = null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Builds a parameter for a boxed key value. A plain anonymous object would bind the
    /// parameter using its compile-time type (<see cref="object"/>), losing the runtime type
    /// Dapper needs to pick the right registered TypeHandler (e.g. for Guid keys). DynamicParameters
    /// instead resolves the DbType/handler from the value's actual runtime type at execute time.
    /// </summary>
    private static DynamicParameters KeyParameter(object id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("__key", id);
        return parameters;
    }

    private EntityInfo GetEntity(Type type) => _entities.GetOrAdd(type, EntityInfo.Build);

    private void EnsureTableCore(Type type)
    {
        if (_ensuredTables.ContainsKey(type))
        {
            return;
        }

        var entity = GetEntity(type);
        RunInLock(tx =>
        {
            var pending = _pendingEnsuredTables;
            if (_ensuredTables.ContainsKey(type) || (pending?.Contains(type) ?? false))
            {
                return;
            }

            if (tx is not null)
            {
                TableBuilder.EnsureTable(_connection, tx, entity);

                // Inside an ambient transaction: stage the "ensured" flag instead of committing
                // it, since this CREATE TABLE is undone if the enclosing InTransaction rolls back.
                (pending ?? throw new InvalidOperationException(
                    "Expected a pending-ensured-tables set while an ambient transaction is active."))
                    .Add(type);
            }
            else
            {
                using var localTx = _connection.BeginTransaction();
                TableBuilder.EnsureTable(_connection, localTx, entity);
                localTx.Commit();
                _ensuredTables[type] = 0;
            }
        });
    }

    private async Task EnsureTableCoreAsync(Type type, CancellationToken cancellationToken)
    {
        if (_ensuredTables.ContainsKey(type))
        {
            return;
        }

        var entity = GetEntity(type);
        await RunInLockAsync(async tx =>
        {
            var pending = _pendingEnsuredTables;
            if (_ensuredTables.ContainsKey(type) || (pending?.Contains(type) ?? false))
            {
                return;
            }

            if (tx is not null)
            {
                await TableBuilder.EnsureTableAsync(_connection, tx, entity, cancellationToken).ConfigureAwait(false);

                // Inside an ambient transaction: stage the "ensured" flag instead of committing
                // it, since this CREATE TABLE is undone if the enclosing InTransactionAsync rolls back.
                (pending ?? throw new InvalidOperationException(
                    "Expected a pending-ensured-tables set while an ambient transaction is active."))
                    .Add(type);
            }
            else
            {
                var localTx = (SqliteTransaction)await _connection
                    .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                await using var _ = localTx.ConfigureAwait(false);
                await TableBuilder.EnsureTableAsync(_connection, localTx, entity, cancellationToken).ConfigureAwait(false);
                await localTx.CommitAsync(cancellationToken).ConfigureAwait(false);
                _ensuredTables[type] = 0;
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private void InsertCore<T>(T item, EntityInfo entity, SqliteTransaction? tx) where T : class
    {
        PrepareKeyForInsert(item, entity);
        _connection.Execute(entity.InsertSql, item, tx);

        if (entity.KeyKind == PrimaryKeyKind.AutoIncrementInteger)
        {
            var newId = _connection.ExecuteScalar<long>("SELECT last_insert_rowid();", transaction: tx);
            SetKeyValue(item, entity, newId);
        }
    }

    private async Task InsertCoreAsync<T>(
        T item, EntityInfo entity, SqliteTransaction? tx, CancellationToken cancellationToken) where T : class
    {
        PrepareKeyForInsert(item, entity);
        var insertCommand = new CommandDefinition(entity.InsertSql, item, tx, cancellationToken: cancellationToken);
        await _connection.ExecuteAsync(insertCommand).ConfigureAwait(false);

        if (entity.KeyKind == PrimaryKeyKind.AutoIncrementInteger)
        {
            var idCommand = new CommandDefinition(
                "SELECT last_insert_rowid();", transaction: tx, cancellationToken: cancellationToken);
            var newId = await _connection.ExecuteScalarAsync<long>(idCommand).ConfigureAwait(false);
            SetKeyValue(item, entity, newId);
        }
    }

    private void UpsertCore<T>(T item, EntityInfo entity, SqliteTransaction? tx) where T : class
    {
        if (entity.KeyKind == PrimaryKeyKind.AutoIncrementInteger &&
            entity.IsKeyDefault(entity.KeyColumn.Property.GetValue(item)))
        {
            InsertCore(item, entity, tx);
            return;
        }

        PrepareKeyForInsert(item, entity);
        _connection.Execute(entity.UpsertSql, item, tx);
    }

    private async Task UpsertCoreAsync<T>(
        T item, EntityInfo entity, SqliteTransaction? tx, CancellationToken cancellationToken) where T : class
    {
        if (entity.KeyKind == PrimaryKeyKind.AutoIncrementInteger &&
            entity.IsKeyDefault(entity.KeyColumn.Property.GetValue(item)))
        {
            await InsertCoreAsync(item, entity, tx, cancellationToken).ConfigureAwait(false);
            return;
        }

        PrepareKeyForInsert(item, entity);
        var command = new CommandDefinition(entity.UpsertSql, item, tx, cancellationToken: cancellationToken);
        await _connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    private static void PrepareKeyForInsert<T>(T item, EntityInfo entity) where T : class
    {
        if (entity.KeyKind != PrimaryKeyKind.Guid)
        {
            return;
        }

        var current = (Guid)(entity.KeyColumn.Property.GetValue(item) ?? Guid.Empty);
        if (current == Guid.Empty)
        {
            entity.KeyColumn.Property.SetValue(item, Guid.NewGuid());
        }
    }

    private static void SetKeyValue<T>(T item, EntityInfo entity, long newId) where T : class
    {
        object typedId = entity.KeyColumn.Property.PropertyType == typeof(int)
            ? (object)(int)newId
            : (object)newId;
        entity.KeyColumn.Property.SetValue(item, typedId);
    }

    private void RunInLock(Action<SqliteTransaction?> action)
    {
        if (_ambientTransaction is not null)
        {
            action(_ambientTransaction);
            return;
        }

        _lock.Wait();
        try
        {
            action(_ambientTransaction);
        }
        finally
        {
            _lock.Release();
        }
    }

    private TResult RunInLock<TResult>(Func<SqliteTransaction?, TResult> action)
    {
        if (_ambientTransaction is not null)
        {
            return action(_ambientTransaction);
        }

        _lock.Wait();
        try
        {
            return action(_ambientTransaction);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task RunInLockAsync(Func<SqliteTransaction?, Task> action, CancellationToken cancellationToken)
    {
        if (_ambientTransaction is not null)
        {
            await action(_ambientTransaction).ConfigureAwait(false);
            return;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await action(_ambientTransaction).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<TResult> RunInLockAsync<TResult>(
        Func<SqliteTransaction?, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (_ambientTransaction is not null)
        {
            return await action(_ambientTransaction).ConfigureAwait(false);
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await action(_ambientTransaction).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connection.Dispose();
        _lock.Dispose();
    }
}
