using System.Reflection;

namespace LibSqlLite.Schema;

/// <summary>
/// Maps CLR property types to SQLite column type affinities. See the type map in CLAUDE.md.
/// </summary>
internal static class TypeMap
{
    /// <summary>
    /// Returns the underlying, non-nullable CLR type (unwraps <see cref="Nullable{T}"/>).
    /// Reference types are returned unchanged.
    /// </summary>
    public static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    /// <summary>
    /// Whether a column for this property type should allow NULL.
    /// Reference types and <see cref="Nullable{T}"/> value types are nullable columns;
    /// non-nullable value types are not.
    /// </summary>
    public static bool IsNullableColumn(Type type) =>
        Nullable.GetUnderlyingType(type) is not null || !type.IsValueType;

    /// <summary>
    /// Whether a property of this type can be persisted as a column.
    /// Unsupported types are skipped silently by callers, never thrown on.
    /// </summary>
    public static bool IsSupported(Type type) => TryGetSqliteType(type, out _);

    /// <summary>
    /// Resolves the SQLite column type affinity ("INTEGER", "REAL", "TEXT", "BLOB") for a
    /// property's CLR type, or returns false if the type has no supported mapping.
    /// </summary>
    public static bool TryGetSqliteType(Type type, out string sqliteType)
    {
        var underlying = Unwrap(type);

        if (underlying.IsEnum)
        {
            sqliteType = "INTEGER";
            return true;
        }

        if (underlying == typeof(int) || underlying == typeof(long) ||
            underlying == typeof(short) || underlying == typeof(byte) ||
            underlying == typeof(bool))
        {
            sqliteType = "INTEGER";
            return true;
        }

        if (underlying == typeof(double) || underlying == typeof(float))
        {
            sqliteType = "REAL";
            return true;
        }

        if (underlying == typeof(decimal) || underlying == typeof(string) ||
            underlying == typeof(Guid) || underlying == typeof(DateTime) ||
            underlying == typeof(DateTimeOffset))
        {
            sqliteType = "TEXT";
            return true;
        }

        if (underlying == typeof(byte[]))
        {
            sqliteType = "BLOB";
            return true;
        }

        sqliteType = string.Empty;
        return false;
    }
}
