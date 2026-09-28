using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>
/// Registers the library's Dapper type handlers exactly once per process.
/// The CLR guarantees a type's static constructor runs at most once and is thread-safe,
/// so registration happens on first call to <see cref="Ensure"/>.
/// </summary>
internal static class TypeHandlerRegistration
{
    static TypeHandlerRegistration()
    {
        // Dapper's built-in typeMap already knows how to bind these types directly and is
        // consulted before the ITypeHandler registry, so our handlers would otherwise be
        // silently ignored. RemoveTypeMap forces Dapper to fall through to AddTypeHandler.
        RemoveBuiltInMapping<Guid>();
        RemoveBuiltInMapping<DateTime>();
        RemoveBuiltInMapping<DateTimeOffset>();
        RemoveBuiltInMapping<decimal>();

        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        SqlMapper.AddTypeHandler(new DecimalTypeHandler());
    }

    private static void RemoveBuiltInMapping<T>() where T : struct
    {
        SqlMapper.RemoveTypeMap(typeof(T));
        SqlMapper.RemoveTypeMap(typeof(T?));
    }

    public static void Ensure()
    {
        // Touching this type triggers the static constructor above, if it hasn't run yet.
    }
}
