using System.Reflection;
using System.Text;

namespace LibSqlLite.Schema;

internal enum PrimaryKeyKind
{
    AutoIncrementInteger,
    Guid,
    String,
}

internal sealed class ColumnInfo
{
    public required PropertyInfo Property { get; init; }
    public required string Name { get; init; }
    public required string SqliteType { get; init; }
    public required bool IsNullable { get; init; }
    public required bool IsKey { get; init; }
}

/// <summary>
/// Cached reflection metadata and pre-built SQL for a type stored via <see cref="SqliteStore"/>.
/// One instance is built per type and reused for the lifetime of the store.
/// </summary>
internal sealed class EntityInfo
{
    public required Type Type { get; init; }
    public required string TableName { get; init; }
    public required ColumnInfo KeyColumn { get; init; }
    public required PrimaryKeyKind KeyKind { get; init; }
    public required IReadOnlyList<ColumnInfo> Columns { get; init; }
    public required IReadOnlyList<ColumnInfo> InsertColumns { get; init; }

    public required string CreateTableSql { get; init; }
    public required string InsertSql { get; init; }
    public required string UpsertSql { get; init; }
    public required string UpdateSql { get; init; }
    public required string SelectByIdSql { get; init; }
    public required string SelectAllSql { get; init; }
    public required string DeleteByIdSql { get; init; }
    public required string DeleteAllSql { get; init; }
    public required IReadOnlyList<string> CreateUniqueIndexSql { get; init; }

    public bool IsKeyDefault(object? keyValue)
    {
        if (keyValue is null)
        {
            return true;
        }

        return KeyKind switch
        {
            PrimaryKeyKind.AutoIncrementInteger => Convert.ToInt64(keyValue) == 0,
            PrimaryKeyKind.Guid => (Guid)keyValue == Guid.Empty,
            PrimaryKeyKind.String => string.IsNullOrEmpty((string)keyValue),
            _ => throw new InvalidOperationException($"Unhandled primary key kind '{KeyKind}'."),
        };
    }

    public static EntityInfo Build(Type type)
    {
        var tableName = type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name;

        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
                p.CanRead && p.CanWrite &&
                p.GetMethod is { IsPublic: true } &&
                p.SetMethod is { IsPublic: true } &&
                p.GetIndexParameters().Length == 0 &&
                p.GetCustomAttribute<IgnoreAttribute>() is null &&
                TypeMap.IsSupported(p.PropertyType))
            .ToList();

        var keyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() is not null)
            ?? properties.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));

        if (keyProperty is null)
        {
            throw new InvalidOperationException(
                $"Type '{type.FullName}' has no primary key. Add a property named 'Id' or mark one with [PrimaryKey].");
        }

        var keyUnderlyingType = TypeMap.Unwrap(keyProperty.PropertyType);
        var keyKind = keyUnderlyingType switch
        {
            var t when t == typeof(int) || t == typeof(long) => PrimaryKeyKind.AutoIncrementInteger,
            var t when t == typeof(Guid) => PrimaryKeyKind.Guid,
            var t when t == typeof(string) => PrimaryKeyKind.String,
            _ => throw new InvalidOperationException(
                $"Type '{type.FullName}' has an unsupported primary key type '{keyProperty.PropertyType}'. " +
                "Primary keys must be int, long, Guid, or string."),
        };

        var columns = properties
            .Select(p =>
            {
                TypeMap.TryGetSqliteType(p.PropertyType, out var sqliteType);
                var isKey = p == keyProperty;
                return new ColumnInfo
                {
                    Property = p,
                    Name = p.Name,
                    SqliteType = keyKind == PrimaryKeyKind.AutoIncrementInteger && isKey ? "INTEGER" : sqliteType,
                    IsNullable = !isKey && TypeMap.IsNullableColumn(p.PropertyType),
                    IsKey = isKey,
                };
            })
            .ToList();

        var keyColumn = columns.Single(c => c.IsKey);

        var insertColumns = keyKind == PrimaryKeyKind.AutoIncrementInteger
            ? columns.Where(c => !c.IsKey).ToList()
            : columns;

        return new EntityInfo
        {
            Type = type,
            TableName = tableName,
            KeyColumn = keyColumn,
            KeyKind = keyKind,
            Columns = columns,
            InsertColumns = insertColumns,
            CreateTableSql = BuildCreateTableSql(tableName, columns, keyColumn, keyKind),
            InsertSql = BuildInsertSql(tableName, insertColumns),
            UpsertSql = BuildUpsertSql(tableName, columns, keyColumn),
            UpdateSql = BuildUpdateSql(tableName, columns, keyColumn),
            SelectByIdSql = $"SELECT * FROM [{tableName}] WHERE [{keyColumn.Name}] = @__key;",
            SelectAllSql = $"SELECT * FROM [{tableName}];",
            DeleteByIdSql = $"DELETE FROM [{tableName}] WHERE [{keyColumn.Name}] = @__key;",
            DeleteAllSql = $"DELETE FROM [{tableName}];",
            CreateUniqueIndexSql = BuildCreateUniqueIndexSql(tableName, columns),
        };
    }

    private static string BuildCreateTableSql(
        string tableName, IReadOnlyList<ColumnInfo> columns, ColumnInfo keyColumn, PrimaryKeyKind keyKind)
    {
        var sb = new StringBuilder();
        sb.Append($"CREATE TABLE IF NOT EXISTS [{tableName}] (\n");

        var definitions = columns.Select(c =>
        {
            if (c.IsKey)
            {
                return keyKind == PrimaryKeyKind.AutoIncrementInteger
                    ? $"  [{c.Name}] INTEGER PRIMARY KEY AUTOINCREMENT"
                    : $"  [{c.Name}] {c.SqliteType} PRIMARY KEY NOT NULL";
            }

            return c.IsNullable
                ? $"  [{c.Name}] {c.SqliteType}"
                : $"  [{c.Name}] {c.SqliteType} NOT NULL DEFAULT {DefaultLiteral(c.SqliteType)}";
        });

        sb.Append(string.Join(",\n", definitions));
        sb.Append("\n);");
        return sb.ToString();
    }

    private static IReadOnlyList<string> BuildCreateUniqueIndexSql(string tableName, IReadOnlyList<ColumnInfo> columns)
    {
        // The key is already unique; an extra index on it would be redundant.
        return columns
            .Where(c => !c.IsKey)
            .Select(c => (Column: c, Unique: c.Property.GetCustomAttribute<UniqueAttribute>()))
            .Where(x => x.Unique is not null)
            .Select(x =>
            {
                var collate = x.Unique!.IgnoreCase ? " COLLATE NOCASE" : "";
                return $"CREATE UNIQUE INDEX IF NOT EXISTS [UX_{tableName}_{x.Column.Name}] " +
                       $"ON [{tableName}] ([{x.Column.Name}]{collate});";
            })
            .ToList();
    }

    internal static string DefaultLiteral(string sqliteType) => sqliteType switch
    {
        "INTEGER" => "0",
        "REAL" => "0",
        "TEXT" => "''",
        "BLOB" => "x''",
        _ => throw new InvalidOperationException($"Unhandled SQLite type '{sqliteType}'."),
    };

    private static string BuildInsertSql(string tableName, IReadOnlyList<ColumnInfo> insertColumns)
    {
        var columnList = string.Join(", ", insertColumns.Select(c => $"[{c.Name}]"));
        var paramList = string.Join(", ", insertColumns.Select(c => $"@{c.Name}"));
        return $"INSERT INTO [{tableName}] ({columnList}) VALUES ({paramList});";
    }

    private static string BuildUpsertSql(string tableName, IReadOnlyList<ColumnInfo> columns, ColumnInfo keyColumn)
    {
        var columnList = string.Join(", ", columns.Select(c => $"[{c.Name}]"));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.Name}"));
        var updateAssignments = string.Join(
            ", ", columns.Where(c => !c.IsKey).Select(c => $"[{c.Name}] = excluded.[{c.Name}]"));

        // A key-only entity has no non-key columns to reassign on conflict - "DO UPDATE SET"
        // with an empty assignment list is invalid SQL, and there's nothing to update anyway.
        var conflictClause = updateAssignments.Length == 0
            ? "DO NOTHING"
            : $"DO UPDATE SET {updateAssignments}";

        return $"INSERT INTO [{tableName}] ({columnList}) VALUES ({paramList}) " +
               $"ON CONFLICT([{keyColumn.Name}]) {conflictClause};";
    }

    private static string BuildUpdateSql(string tableName, IReadOnlyList<ColumnInfo> columns, ColumnInfo keyColumn)
    {
        var setList = string.Join(", ", columns.Where(c => !c.IsKey).Select(c => $"[{c.Name}] = @{c.Name}"));
        return $"UPDATE [{tableName}] SET {setList} WHERE [{keyColumn.Name}] = @{keyColumn.Name};";
    }
}
