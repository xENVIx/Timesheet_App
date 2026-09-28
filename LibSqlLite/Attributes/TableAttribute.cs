namespace LibSqlLite;

/// <summary>
/// Overrides the table name used for a class. By default the table name is the class name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TableAttribute : Attribute
{
    public TableAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
