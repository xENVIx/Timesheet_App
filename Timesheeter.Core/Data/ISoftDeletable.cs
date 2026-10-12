namespace Timesheeter.Core.Data
{
    /// <summary>
    /// A record that is never removed from the database: deleting it only sets <see cref="Deleted"/>.
    /// See <see cref="DataClass{T}.Delete"/> for what that hides.
    /// </summary>
    public interface ISoftDeletable
    {
        bool Deleted { get; set; }
    }
}
