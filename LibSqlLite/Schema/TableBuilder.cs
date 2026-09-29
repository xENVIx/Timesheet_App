using Microsoft.Data.Sqlite;

namespace LibSqlLite.Schema;

/// <summary>
/// Creates and additively evolves the SQLite table backing an <see cref="EntityInfo"/>.
/// </summary>
internal static class TableBuilder
{
    public static void EnsureTable(SqliteConnection connection, SqliteTransaction transaction, EntityInfo entity)
    {
        using (var create = connection.CreateCommand())
        {
            create.Transaction = transaction;
            create.CommandText = entity.CreateTableSql;
            create.ExecuteNonQuery();
        }

        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.Transaction = transaction;
            pragma.CommandText = $"PRAGMA table_info([{entity.TableName}]);";
            using var reader = pragma.ExecuteReader();
            var nameOrdinal = reader.GetOrdinal("name");
            while (reader.Read())
            {
                existingColumns.Add(reader.GetString(nameOrdinal));
            }
        }

        foreach (var column in entity.Columns)
        {
            if (column.IsKey || existingColumns.Contains(column.Name))
            {
                continue;
            }

            using var alter = connection.CreateCommand();
            alter.Transaction = transaction;
            alter.CommandText = column.IsNullable
                ? $"ALTER TABLE [{entity.TableName}] ADD COLUMN [{column.Name}] {column.SqliteType};"
                : $"ALTER TABLE [{entity.TableName}] ADD COLUMN [{column.Name}] {column.SqliteType} " +
                  $"NOT NULL DEFAULT {EntityInfo.DefaultLiteral(column)};";
            alter.ExecuteNonQuery();
        }

        // After the column step, so a [Unique] property added to an existing class has its column.
        foreach (var sql in entity.CreateUniqueIndexSql)
        {
            using var index = connection.CreateCommand();
            index.Transaction = transaction;
            index.CommandText = sql;
            index.ExecuteNonQuery();
        }
    }

    public static async Task EnsureTableAsync(
        SqliteConnection connection, SqliteTransaction transaction, EntityInfo entity, CancellationToken cancellationToken)
    {
        using (var create = connection.CreateCommand())
        {
            create.Transaction = transaction;
            create.CommandText = entity.CreateTableSql;
            await create.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var pragma = connection.CreateCommand())
        {
            pragma.Transaction = transaction;
            pragma.CommandText = $"PRAGMA table_info([{entity.TableName}]);";
            using var reader = await pragma.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var nameOrdinal = reader.GetOrdinal("name");
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                existingColumns.Add(reader.GetString(nameOrdinal));
            }
        }

        foreach (var column in entity.Columns)
        {
            if (column.IsKey || existingColumns.Contains(column.Name))
            {
                continue;
            }

            using var alter = connection.CreateCommand();
            alter.Transaction = transaction;
            alter.CommandText = column.IsNullable
                ? $"ALTER TABLE [{entity.TableName}] ADD COLUMN [{column.Name}] {column.SqliteType};"
                : $"ALTER TABLE [{entity.TableName}] ADD COLUMN [{column.Name}] {column.SqliteType} " +
                  $"NOT NULL DEFAULT {EntityInfo.DefaultLiteral(column)};";
            await alter.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var sql in entity.CreateUniqueIndexSql)
        {
            using var index = connection.CreateCommand();
            index.Transaction = transaction;
            index.CommandText = sql;
            await index.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
