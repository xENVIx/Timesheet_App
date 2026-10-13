using LibLogging;
using LibSqlLite.Schema;
using LibSqlLite.TypeHandlers;
using Microsoft.Data.Sqlite;

namespace LibSqlLite;

/// <summary>What <see cref="SqliteConverter.Convert"/> did.</summary>
/// <param name="Succeeded">True when the new database was written and checks out.</param>
/// <param name="Problems">Why it failed (no new database is left behind then).</param>
/// <param name="RowCounts">Rows copied per table.</param>
public sealed record ConversionResult(bool Succeeded, IReadOnlyList<string> Problems, IReadOnlyDictionary<string, long> RowCounts);

/// <summary>
/// Converts a database to the schema its classes now declare, for changes SQLite can't make to an
/// existing table, such as adding a foreign key (<see cref="ReferencesAttribute"/>): creates a new
/// database from the classes, copies every row of the old one into it (keys and AUTOINCREMENT
/// counters kept), and checks every reference resolves. The old database is only read.
/// </summary>
/// <remarks>
/// Run it deliberately (e.g. from a conversion tool) while nothing else has either file open.
/// Tables in the old database that aren't in <c>types</c> are copied as they are; columns the old
/// tables don't have get their defaults.
/// </remarks>
public static class SqliteConverter
{
    /// <param name="sourcePath">The existing database.</param>
    /// <param name="destinationPath">The new database to create; must not exist yet.</param>
    /// <param name="logger">Optional; told what happened.</param>
    /// <param name="types">Every class stored in the database.</param>
    public static ConversionResult Convert(string sourcePath, string destinationPath, ILoggerApp? logger, params Type[] types)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        var source = Path.GetFullPath(sourcePath);
        var destination = Path.GetFullPath(destinationPath);

        if (!File.Exists(source)) return Failed($"{source} doesn't exist.");
        if (File.Exists(destination)) return Failed($"{destination} already exists.");
        if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase)) return Failed("The new database must be a different file.");

        TypeHandlerRegistration.Ensure();
        logger?.Info($"Converting {source} into {destination}");

        try
        {
            // Fold any -wal content into the old file, so the copy includes the latest changes.
            using (var old = Open(source)) Execute(old, "PRAGMA wal_checkpoint(TRUNCATE);");

            // The new file, with every table created from the current classes.
            using (var store = new SqliteStore(destination, logger))
            {
                foreach (var type in types)
                {
                    typeof(SqliteStore).GetMethod(nameof(SqliteStore.EnsureTable))!.MakeGenericMethod(type).Invoke(store, null);
                }
            }

            var (problems, counts) = CopyAndCheck(destination, source);
            if (problems.Count > 0)
            {
                DeleteDatabaseFiles(destination);
                foreach (var problem in problems) logger?.Error($"Conversion failed: {problem}");
                return new ConversionResult(false, problems, counts);
            }

            logger?.Info($"Converted {source} into {destination}");
            return new ConversionResult(true, [], counts);
        }
        catch (Exception ex)
        {
            DeleteDatabaseFiles(destination);
            var message = ex is System.Reflection.TargetInvocationException { InnerException: { } inner } ? inner.Message : ex.Message;
            logger?.Error($"Conversion failed: {message}", ex);
            return Failed(message);
        }
    }

    /// <summary>
    /// "Table.Column" for each <see cref="ReferencesAttribute"/> column whose table in the database
    /// exists without that foreign key, i.e. what a conversion would add. Empty if none.
    /// </summary>
    public static IReadOnlyList<string> MissingForeignKeys(string databasePath, params Type[] types)
    {
        var missing = new List<string>();
        if (!File.Exists(databasePath)) return missing;

        using var connection = Open(Path.GetFullPath(databasePath));
        foreach (var entity in types.Select(EntityInfo.Build))
        {
            if (!TableExists(connection, "main", entity.TableName)) continue;

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

    private static ConversionResult Failed(string problem) =>
        new(false, [problem], new Dictionary<string, long>());

    /// <summary>
    /// Copies every row of the old database into the new one (foreign keys off while copying, so
    /// table order doesn't matter), then lists whatever breaks a foreign key or a unique rule.
    /// </summary>
    private static (List<string> Problems, Dictionary<string, long> Counts) CopyAndCheck(string destination, string source)
    {
        var problems = new List<string>();
        var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        using var connection = Open(destination);
        Execute(connection, "PRAGMA foreign_keys=OFF;");

        using (var attach = connection.CreateCommand())
        {
            attach.CommandText = "ATTACH DATABASE $path AS original;";
            attach.Parameters.AddWithValue("$path", source);
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
                    using var copy = connection.CreateCommand();
                    copy.Transaction = tx;
                    copy.CommandText = $"INSERT INTO main.[{table}] ({list}) SELECT {list} FROM original.[{table}];";
                    counts[table] = copy.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    problems.Add($"{table}: {ex.Message}"); // e.g. duplicates a unique rule refuses
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

            if (problems.Count > 0) return (problems, counts);
            tx.Commit();
        }

        // Every reference must point at an existing row, or the foreign keys would block edits later.
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
        return (problems, counts);
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
        return System.Convert.ToInt64(command.ExecuteScalar()) > 0;
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
        foreach (var file in new[] { path, path + "-wal", path + "-shm" })
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
}
