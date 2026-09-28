namespace LibSqlLite;

/// <summary>
/// Excludes a public property from schema generation and all CRUD operations.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class IgnoreAttribute : Attribute
{
}
