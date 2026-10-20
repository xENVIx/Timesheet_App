using LibLogging;
using LibSqlLite;
using Microsoft.AspNetCore.DataProtection;
using SQLitePCL;
using System.Security.Cryptography;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Server.Classes;
using static Timesheeter.Server.Classes.APIKeys;
namespace Timesheeter.Server
{
    

    public class Factory
    {

        
        private volatile bool _initialized = false;
        private LibSqlLite.SqliteStore? _store;/// <summary>Where the database and its backups live (also shown on the About screen).</summary>
        internal static readonly String DataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Timersheeter.Server");

        private Logger? _logger;
        private CancellationTokenSource _cts;


        private AppFactory? _appFactory;

        private APIKeys? _apiKeys;
        public Factory()
        {
            _cts = new CancellationTokenSource();

        }

        public bool Initialize(string[] args)
        {
            /* initialize database...
            */
            _logger = new Logger("Timesheeter.Server", LogLevel.Trace, _cts.Token, true);
            _logger.Run();

            String timeSheeterPath = DataFolder;

            if (!Directory.Exists(timeSheeterPath))
            {
                Directory.CreateDirectory(timeSheeterPath);
            }
            const String fileName = "Timesheeter";
            const String fileExt = ".db";

            String dbFile = Path.Combine(timeSheeterPath, String.Concat(fileName, fileExt));


            // Checked before opening: opening creates the file, and a brand new database needs no backup.
            bool existingDatabase = File.Exists(dbFile);

            _store = new LibSqlLite.SqliteStore(dbFile, _logger);

            try
            {
                _store.EnsureTable<Customers.Customer>();
                _store.EnsureTable<ProjectCodes.ProjectCode>();
                _store.EnsureTable<TimeEntries.TimeEntry>();

                _apiKeys = new APIKeys(_store, _logger);
                

            }
            catch (Exception ex)
            {
                _logger.Error($"Exception Ensuring Tables Exist");
                _logger.Exception(ex);
                throw;
            }

            // go through the arguments here...
            if (args.Length > 0)
            {
                if (!ArgumentProcessor(args))
                {
                    return false;
                }

            }
            else
            {
                Console.WriteLine($"Run the server with the argument --commands to see the server command options.");
            }



            // make a backup if the file already existed...
            // Backed up through the open store rather than copying the .db file: changes from the
            // last session can still be in the -wal file, which a file copy would leave out.
            if (existingDatabase)
            {
                
                DateTime dateNow = DateTime.Now;
                String backupPath = Path.Combine(timeSheeterPath, "backups");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                String newBackupFormat = "_{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}";

                String backupFile =
                    String.Concat(
                        Path.Combine(
                            backupPath,
                            String.Concat(
                                fileName,
                                fileExt,
                                String.Format(
                                    newBackupFormat,
                                    dateNow.Year,
                                    dateNow.Month,
                                    dateNow.Day,
                                    dateNow.Hour,
                                    dateNow.Minute,
                                    dateNow.Second
                                )
                                
                            )
                        ),
                        ".bak"
                    );


                _store.BackupTo(backupFile);

                // Keep only the newest backups. The timestamp in the name (yyyyMMddHHmmss) sorts
                // the same as the time, so newest first is descending by name.
                const int backupsToKeep = 10;
                var oldBackups = Directory.GetFiles(backupPath, String.Concat(fileName, fileExt, "_*.bak"))
                    .OrderByDescending(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                    .Skip(backupsToKeep);

                foreach (var oldBackup in oldBackups)
                {
                    // A backup that can't be deleted (e.g. open elsewhere) is retried next launch.
                    try { File.Delete(oldBackup); }
                    catch (Exception ex) { Console.WriteLine($"Could not delete old backup '{oldBackup}': {ex.Message}"); }
                }

            }



            _appFactory = new AppFactory(_store, _logger, _cts.Token, args);
            _appFactory.Initialize();

            


            _initialized = true;

            return _initialized;
        }

        private bool ArgumentProcessor(String[] args)
        {
            //System.Threading.Thread.Sleep(10000);
            switch (args[0].Trim())
            {
                case "--generateApi": GenerateApiKey(args); return false;
                case "--viewKeys": ViewApiKeys(args); return false;
                case "--deleteKey": DeleteApiKey(args); return false;
                case "--commands": ShowCommands(); return false;
            }

            return true;
        }

        private void ShowCommands()
        {
            Console.WriteLine($"Server Commands (each one runs and exits without starting the server)\n" +
                $"--generateApi \"<secret>\"    Generates a new API Key for the secret.  The API Key is shown once only.\n" +
                $"--viewKeys                  Displays all registered secrets.\n" +
                $"--deleteKey \"<secret>\"      Deletes the API Key for the secret.\n" +
                $"--commands                  Shows this list.\n" +
                $"\nRun without a command to start the server.");
        }

        private void DeleteApiKey(String[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine($"Invalid Argument Format.\n--deleteKey \"<secret>\"");
                return;
            }

            if (_apiKeys == null)
            {
                Console.WriteLine($"Couldn't Delete API Key: {nameof(_apiKeys)} is null");
                return;
            }

            String secret = args[1].Trim();
            var key = _apiKeys.Active.FirstOrDefault(x => String.Compare(x.Secret, secret, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (key == null)
            {
                Console.WriteLine($"No API Key With Secret \"{secret}\".  Use --viewKeys to see the registered secrets.");
                return;
            }

            if (!_apiKeys.Delete(key))
            {
                Console.WriteLine($"Failed To Delete API Key");
                return;
            }

            Console.WriteLine($"Deleted API Key With Secret \"{key.Secret}\".");
        }

        private void ViewApiKeys(String[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine($"Proper Argument Usage\n" +
                    $"--viewKeys\n" +
                    $"Displays all registered secrets.  API Keys cannot be viewed again, if you need a new one you must generate a new set of data.");
                return;
            }

            if (_apiKeys == null)
            {
                Console.WriteLine($"Couldn't Read API Keys: {nameof(_apiKeys)} is null");
                return;
            }

            foreach (var key in _apiKeys.Active)
            {
                Console.WriteLine($"Secret: {key.Secret} | TOC: {key.TOC} | TOM: {key.TOM} | MOP: {key.MOP}");
            }
        }

        private void GenerateApiKey(String[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine($"Invalid Argument Format.\n--generateApi \"<secret>\"");
                return;
            }
            // args 1 should be the secret
            String secret = args[1].Trim();
            String apiKey = ApiKeyUtility.GenerateApiKey();
            if (secret.Length <= 0 || apiKey.Length <= 0)
            {
                Console.WriteLine($"Secret or API Key Generation Failed");
            }
            else
            {

                if (_apiKeys == null)
                {
                    Console.WriteLine($"Couldn't Write New API Key: {nameof(_apiKeys)} is null");
                    return;
                }

                var oldKey = _apiKeys.All.FirstOrDefault(x => String.Compare(x.Secret, secret, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (oldKey != null)
                {
                    if (!oldKey.Deleted)
                    {
                        Console.WriteLine($"Secret Already Exists.  Enter a new one and try again.");
                        return;
                    }

                    // key exists just in deleted state... undelete it with the new key.
                    // Restore updates the existing row; Save would try to insert it again.
                    byte[] oldHash = oldKey.Key;
                    oldKey.Key = ApiKeyUtility.HashApiKey(apiKey);

                    if (!_apiKeys.Restore(oldKey))
                    {
                        oldKey.Key = oldHash;
                        Console.WriteLine($"Failed To Save New API Key");
                        return;
                    }
                }
                else
                {
                    var apiKeyEnter = new APIKeys.APIKey
                    {
                        Secret = secret,
                        Key = ApiKeyUtility.HashApiKey(apiKey),
                    };

                    if (!_apiKeys.Save(apiKeyEnter))
                    {
                        Console.WriteLine($"Failed To Save New API Key");
                        return;
                    }
                }

                Console.WriteLine($"Secret: {secret}\nAPI Key: {apiKey}\nCopy For Your Records... The API Key Can Not Be Viewed Again!");
            }

            return;
        }




        


        public void Run()
        {
            if (!_initialized)
                throw new Exception($"Factory is not initialized");


            if (_appFactory == null)
                throw new ArgumentNullException(nameof(_appFactory));



            //_app.Run();
            _appFactory.Run();
        }
        

    }
}
