namespace LibSqlLite;

/// <summary>
/// Marks the property that is the table's primary key when it isn't named <c>Id</c>.
/// Takes precedence over an <c>Id</c>-named property when both are present.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class PrimaryKeyAttribute : Attribute
{
}
