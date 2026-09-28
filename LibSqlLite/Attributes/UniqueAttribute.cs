namespace LibSqlLite;

/// <summary>
/// Enforces that no two rows share the same value for this property, via a unique index
/// named <c>UX_{Table}_{Column}</c>. Has no effect on the primary key, which is already unique.
/// </summary>
/// <remarks>
/// The index is created with <c>CREATE UNIQUE INDEX IF NOT EXISTS</c>, so changing
/// <see cref="IgnoreCase"/> after the index exists does not alter it. Creating the index on an
/// existing table fails if that table already holds duplicate values.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class UniqueAttribute : Attribute
{
    /// <summary>
    /// When true, values that differ only by case (e.g. "Acme" and "ACME") count as duplicates.
    /// Uses SQLite's <c>NOCASE</c> collation, which folds ASCII letters only.
    /// </summary>
    public bool IgnoreCase { get; set; }
}
