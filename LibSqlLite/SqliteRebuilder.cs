using LibLogging;
using LibSqlLite.Schema;
using LibSqlLite.TypeHandlers;
using Microsoft.Data.Sqlite;

namespace LibSqlLite;

/// <summary>What <see cref="SqliteRebuilder.RebuildIfNeeded"/> did.</summary>
/// <param name="Rebuilt">True when the database was replaced by a rebuilt copy.</param>
/// <param name="BackupPath">Where the original database was kept, when rebuilt.</param>
/// <param name="Problems">Why a needed rebuild didn't happen (the original is then left as it was).</param>
public sealed record RebuildResult(bool Rebuilt, string? BackupPath, IReadOnlyList<string> Problems)
{
    public static readonly RebuildResult NotNeeded = new(false, null, []);
}

/// <summary>
/// Rebuilds a database whose tables lack foreign keys their classes now declare with
/// <see cref="ReferencesAttribute"/>. SQLite can't add a foreign key to an existing table, so this
/// creates a new database file with the current schema, copies every row across (keeping their
/// keys), checks every reference resolves, and only then swaps the new file in, keeping the
/// original as a backup. If anything doesn't check out, the original is left untouched.
/// </summary>
/// <remarks>
/// Call it before opening a <see cref="SqliteStore"/> on the file, e.g. at startup:
/// <code>
/// var result = SqliteRebuilder.RebuildIfNeeded(path, logger, typeof(Customer), typeof(ProjectCode));
/// using var store = new SqliteStore(path, logger);
/// </code>
/// Tables in the file that aren't in <c>types</c> are copied as they are.
/// </remarks>
public static class SqliteRebuilder
{
    /// <param name="databasePath">The database file. Nothing happens if it doesn't exist yet.</param>
    /// <param name="logger">Optional; told what happened.</param>
    /// <param name="types">Every class stored in the database.</param>
    public static RebuildResult RebuildIfNeeded(string databasePath, ILoggerApp? logger, params Type[] types)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        var path = Path.GetFullPath(databasePath);
        if (!File.Exists(path)) return RebuildResult.NotNeeded;

        TypeHandlerRegistration.Ensure();
        var entities = types.Select(EntityInfo.Build).ToList();

        List<string> missing;
        using (var original = Open(path))
        {
            // Fold any -wal content into the file first: it's what gets copied, and later moved.
            Execute(original, "PRAGMA wal_checkpoint(TRUNCATE);");
            missing = MissingForeignKeys(original, entities);
        }

        if (missing.Count == 0) return RebuildResult.NotNeeded;

        logger?.Info($"Rebuilding {path} to add foreign keys: {string.Join(", ", missing)}");

        var rebuiltPath = path + ".rebuild";
        DeleteDatabaseFiles(rebuiltPath);

        try
        {
            // The new file, with every table created from the current classes (foreign keys included).
            using (var store = new SqliteStore(rebuiltPath, logger))
            {
                foreach (var entity in entities) EnsureTable(store, entity.Type);
            }

            var problems = CopyAndCheck(rebuiltPath, path);
            if (problems.Count > 0)
            {
                DeleteDatabaseFiles(rebuiltPath);
                foreach (var problem in problems) logger?.Error($"Database not rebuilt: {problem}");
                return new RebuildResult(false, null, problems);
            }
        }
        catch (Exception ex)
        {
            DeleteDatabaseFiles(rebuiltPath);
            logger?.Error($"Database not rebuilt: {ex.Message}", ex);
            return new RebuildResult(false, null, [ex.Message]);
        }

        // Swap: the original becomes the backup, the rebuilt copy takes its place.
        var backupPath = $"{path}.before-rebuild-{DateTime.Now:yyyyMMddHHmmss}.bak";
        File.Move(path, backupPath);
        DeleteFile(path + "-wal");
        DeleteFile(path + "-shm");
        File.Move(rebuiltPath, path);
        DeleteDatabaseFiles(rebuiltPath);

        logger?.Info($"Database rebuilt; the original is kept at {backupPath}");
        return new RebuildResult(true, backupPath, []);
    }

    /// <summary>"Table.Column" for each [References] column whose table exists without that foreign key.</summary>
    private static List<string> MissingForeignKeys(SqliteConnection connection, IEnumerable<EntityInfo> entities)
    {
        var missing = new List<string>();
        foreach (var entity in entities)
        {
            if (!TableExists(connection, "main", entity.TableName)) continue; // created fresh, with its keys

            var declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA foreign_key_list([{entity.TableName}]);";
                using var reader = command.ExecuteReader();
                int from = reader.GetOrdinal("from"), table = reader.GetOrdinal("table");
                while (reader.Read()) declared.Add($"{reader.GetString(from)}->{reader.GetString(table)}");
            }

            foreach (var column in entity.ForeignKeyColumns)
            {
                if (!declared.Contains($"{column.Name}->{column.References!.Table}"))
                    missing.Add($"{entity.TableName}.{column.Name}");
            }
        }
        return missing;
    }

    /// <summary>
    /// Copies every row of the original into the rebuilt file (foreign keys off while copying, so
    /// table order doesn't matter), then lists whatever breaks a foreign key or a unique rule.
    /// </summary>
    private static List<string> CopyAndCheck(string rebuiltPath, string originalPath)
    {
        var problems = new List<string>();

        using var connection = Open(rebuiltPath);
        Execute(connection, "PRAGMA foreign_keys=OFF;");

        using (var attach = connection.CreateCommand())
        {
            attach.CommandText = "ATTACH DATABASE $path AS original;";
            attach.Parameters.AddWithValue("$path", originalPath);
            attach.ExecuteNonQuery();
        }

        using (var tx = connection.BeginTransaction())
        {
            foreach (var table in Tables(connection, "original"))
            {
                if (!TableExists(connection, "main", table, tx))
                {
                    // Not one of the classes: recreate it as it was.
                    Execute(connection, CreateSqlOf(connection, table, tx), tx);
                }

                var columns = Columns(connection, "original", table, tx)
                    .Intersect(Columns(connection, "main", table, tx), StringComparer.OrdinalIgnoreCase)
                    .Select(c => $"[{c}]")
                    .ToList();
                if (columns.Count == 0) continue;

                var list = string.Join(", ", columns);
                try
                {
                    Execute(connection, $"INSERT INTO main.[{table}] ({list}) SELECT {list} FROM original.[{table}];", tx);
                }
                catch (SqliteException ex)
                {
                    problems.Add($"{table}: {ex.Message}"); // e.g. duplicates a new unique rule refuses
                }
            }

            // Keep the AUTOINCREMENT counters, so keys of rows deleted in the past are never reused.
            if (TableExists(connection, "original", "sqlite_sequence", tx) && TableExists(connection, "main", "sqlite_sequence", tx))
            {
                Execute(connection,
                    "DELETE FROM main.sqlite_sequence WHERE name IN (SELECT name FROM original.sqlite_sequence);" +
                    "INSERT INTO main.sqlite_sequence (name, seq) SELECT name, seq FROM original.sqlite_sequence " +
                    "WHERE name IN (SELECT name FROM main.sqlite_master WHERE type = 'table');", tx);
            }

            if (problems.Count > 0) return problems;
            tx.Commit();
        }

        // Every reference must point at an existing row, or the new foreign keys would block edits.
        using (var check = connection.CreateCommand())
        {
            check.CommandText = "PRAGMA main.foreign_key_check;";
            using var reader = check.ExecuteReader();
            while (reader.Read())
            {
                string table = reader.GetString(0);
                string row = reader.IsDBNull(1) ? "?" : reader.GetValue(1).ToString()!;
                string parent = reader.GetString(2);
                problems.Add($"{table} row {row} refers to a {parent} that doesn't exist.");
            }
        }

        Execute(connection, "DETACH DATABASE original;");
        return problems;
    }

    private static void EnsureTable(SqliteStore store, Type type)
    {
        typeof(SqliteStore).GetMethod(nameof(SqliteStore.EnsureTable))!.MakeGenericMethod(type).Invoke(store, null);
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Pooling=False");
        connection.Open();
        return connection;
    }

    private static void Execute(SqliteConnection connection, string sql, SqliteTransaction? tx = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static bool TableExists(SqliteConnection connection, string schema, string table, SqliteTransaction? tx = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = $"SELECT COUNT(*) FROM {schema}.sqlite_master WHERE type = 'table' AND name = $name COLLATE NOCASE;";
        command.Parameters.AddWithValue("$name", table);
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static List<string> Tables(SqliteConnection connection, string schema)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM {schema}.sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";
        using var reader = command.ExecuteReader();
        var tables = new List<string>();
        while (reader.Read()) tables.Add(reader.GetString(0));
        return tables;
    }

    private static string CreateSqlOf(SqliteConnection connection, string table, SqliteTransaction tx)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "SELECT sql FROM original.sqlite_master WHERE type = 'table' AND name = $name;";
        command.Parameters.AddWithValue("$name", table);
        return (string)command.ExecuteScalar()!;
    }

    private static List<string> Columns(SqliteConnection connection, string schema, string table, SqliteTransaction tx)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = $"PRAGMA {schema}.table_info([{table}]);";
        using var reader = command.ExecuteReader();
        int name = reader.GetOrdinal("name");
        var columns = new List<string>();
        while (reader.Read()) columns.Add(reader.GetString(name));
        return columns;
    }

    private static void DeleteDatabaseFiles(string path)
    {
        DeleteFile(path);
        DeleteFile(path + "-wal");
        DeleteFile(path + "-shm");
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}
