using LibSqlLite;
using Timesheeter.Core.Data;

namespace Timesheeter.DbConversion
{
    /// <summary>
    /// Converts a Timesheeter database to the current schema, e.g. to add the foreign keys SQLite
    /// can't add to existing tables: creates a new database from the current record classes and
    /// loads all the old database's data into it. Run deliberately, with Timesheeter closed.
    /// </summary>
    internal static class Program
    {
        private const string Usage = """
            Timesheeter.DbConversion [<old.db>] [<new.db>] [--replace] [--force]

              <old.db>    The database to convert. Default: Documents\Timersheeter\Timesheeter.db
              <new.db>    Where to write the converted database.
                          Default: next to the old one, <name>.converted-<date>.db
              --replace   After a successful conversion, put the new database in the old one's place
                          (the old one is kept as <name>.before-conversion-<date>.bak)
              --force     Convert even if the database already has everything
              --no-song   Skip "Timesheet Toxicity"

            The old database is only read. Close Timesheeter (and the server) first.
            """;

        private static int Main(string[] args)
        {
            // So the song's ♪ shows in the Windows console too.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args.Any(a => a is "-h" or "--help" or "/?"))
            {
                Console.WriteLine(Usage);
                return 0;
            }

            bool replace = args.Contains("--replace", StringComparer.OrdinalIgnoreCase);
            bool force = args.Contains("--force", StringComparer.OrdinalIgnoreCase);
            bool song = !args.Contains("--no-song", StringComparer.OrdinalIgnoreCase);
            var paths = args.Where(a => !a.StartsWith("--")).ToList();

            string oldPath = Path.GetFullPath(paths.ElementAtOrDefault(0) ?? DefaultDatabase());
            string stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string newPath = Path.GetFullPath(paths.ElementAtOrDefault(1)
                ?? Path.Combine(Path.GetDirectoryName(oldPath)!, $"{Path.GetFileNameWithoutExtension(oldPath)}.converted-{stamp}.db"));

            if (song) Console.WriteLine(TimesheetToxicity.Lyrics);
            Console.WriteLine($"Timesheeter.DbConversion {typeof(Program).Assembly.GetName().Version?.ToString(2)}");
            Console.WriteLine($"  old: {oldPath}");

            if (!File.Exists(oldPath))
            {
                Console.Error.WriteLine("The database doesn't exist.");
                Console.WriteLine();
                Console.WriteLine(Usage);
                return 1;
            }

            var missing = SqliteConverter.MissingForeignKeys(oldPath, StoredTypes.App);
            if (missing.Count == 0 && !force)
            {
                Console.WriteLine("Already up to date: it has every foreign key. Nothing to do (--force converts anyway).");
                return 0;
            }
            if (missing.Count > 0) Console.WriteLine($"  adds foreign keys: {string.Join(", ", missing)}");
            Console.WriteLine($"  new: {newPath}");
            Console.WriteLine();

            var result = SqliteConverter.Convert(oldPath, newPath, null, StoredTypes.App);

            if (!result.Succeeded)
            {
                Console.Error.WriteLine("Conversion failed; the old database is unchanged and no new one was kept:");
                foreach (var problem in result.Problems) Console.Error.WriteLine($"  - {problem}");
                return 2;
            }

            foreach (var (table, rows) in result.RowCounts.OrderBy(kv => kv.Key))
            {
                Console.WriteLine($"  {table,-16} {rows,8} rows");
            }
            Console.WriteLine();
            Console.WriteLine("Converted.");

            if (!replace)
            {
                Console.WriteLine("To use it, close Timesheeter and put the new file in place of the old one (or run again with --replace).");
                return 0;
            }

            try
            {
                string backup = Path.Combine(Path.GetDirectoryName(oldPath)!,
                    $"{Path.GetFileNameWithoutExtension(oldPath)}.before-conversion-{stamp}.bak");
                File.Move(oldPath, backup);
                foreach (var leftover in new[] { oldPath + "-wal", oldPath + "-shm" })
                {
                    if (File.Exists(leftover)) File.Delete(leftover);
                }
                File.Move(newPath, oldPath);

                Console.WriteLine($"Replaced. The old database is kept as {backup}");
                return 0;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Couldn't put the new database in place ({ex.Message}). Is Timesheeter still open?");
                Console.Error.WriteLine($"The converted database is at {newPath}");
                return 3;
            }
        }

        // Where the desktop app keeps its database (Timesheeter's Factory.DataFolder).
        private static string DefaultDatabase() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Timersheeter", "Timesheeter.db");
    }
}
