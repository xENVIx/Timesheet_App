namespace LibSqlLite;

/// <summary>
/// Makes this property a foreign key to the primary key of <see cref="Type"/>'s table, e.g.
/// <c>[References(typeof(Customer))] public long CustomerID { get; set; }</c>. SQLite then refuses
/// rows whose value has no matching row there, and refuses deleting a row something still refers to.
/// A null value (for a nullable property) refers to nothing and is allowed.
/// </summary>
/// <remarks>
/// SQLite can only add a foreign key when it creates a table, so a table that already exists keeps
/// working without it. To add it, convert the database deliberately with
/// <see cref="SqliteConverter.Convert"/>, which creates a new database and copies the data into it.
/// The referenced table is created first when this table is.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ReferencesAttribute : Attribute
{
    public ReferencesAttribute(Type type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>The class whose table this property refers to.</summary>
    public Type Type { get; }
}
