namespace Timesheeter.Core.Data
{
    /// <summary>
    /// A record that remembers when it was created and last changed, and by what. Set by
    /// <see cref="Audit.Stamp"/> (DataClass and the server do it on every write); never edited by hand.
    /// </summary>
    public interface IAudited
    {
        /// <summary>Time of creation, UTC.</summary>
        DateTime TOC { get; set; }

        /// <summary>Time of the last modification, UTC.</summary>
        DateTime TOM { get; set; }

        /// <summary>Modification process: the program and machine that made the last change.</summary>
        string MOP { get; set; }
    }
}
