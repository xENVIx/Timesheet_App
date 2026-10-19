using LibLogging;
using LibSqlLite;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;
using static Timesheeter.Server.APIKeys;
namespace Timesheeter.Server
{
    

    public class Factory
    {

        

        private WebApplication? _app;
        private volatile bool _initialized = false;
        private LibSqlLite.SqliteStore? _store;/// <summary>Where the database and its backups live (also shown on the About screen).</summary>
        internal static readonly String DataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Timersheeter.Server");

        private Logger? _logger;
        private CancellationTokenSource _cts;


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


            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton(_store);

            _app = builder.Build();


            
            // Configure the HTTP request pipeline.
            if (_app.Environment.IsDevelopment())
            {
                _app.UseSwagger();
                _app.UseSwaggerUI();
            }

            

            try
            {
                var mapGroupCustomers       = MapCRUD   <Customers.Customer>       ("/api/customers");
                var mapGroupProjectCodes    = MapCRUD   <ProjectCodes.ProjectCode> ("/api/project_codes");
                var mapGroupTimeEntries     = MapCRUD   <TimeEntries.TimeEntry>    ("/api/time_entries");


                var mapGroupTest = _app.MapGroup("/api/test");

                // GET /api/customers and /api/customers/{id}: JSON, or XML when the request's Accept header
                // asks for it (Accept: application/xml).
                string name = "/api/test".TrimEnd('/')[("/api/test".TrimEnd('/').LastIndexOf('/') + 1)..];
                mapGroupTest.MapGet("/", (HttpRequest request) => 
                {
                    _logger?.Verbose($"Test Received");


                    foreach ( var header in request.Headers)
                    {
                        _logger?.Verbose($"Header: {header.Key} || {header.Value}");
                    }

                    // get the API key...
                    if (!request.Headers.TryGetValue("X-API-Key", out var key) || !request.Headers.TryGetValue("X-Signature", out var signature))
                    {
                        Results.Unauthorized();
                        return;
                    }

                    if (_apiKeys.Active.ToList().Exists(x => x.Key == key))
                    {
                        // compute the HMAC and COMPARE to sent SIGNATURE...
                        String secret = _apiKeys.Active.Where(x => x.Key == key).First().Secret;

                        var sig = Core.Lib.Security.ComputeSignature(key!, secret);

                        if (sig == null)
                        {
                            Results.Unauthorized();
                            return;
                        }


                        if (String.Compare(sig, signature) == 0)
                        {
                            Results.Json("Success!");
                            return;
                        }
                        else
                        {
                            Results.Unauthorized();
                            return;
                        }

                    }
                    else
                    {
                        Results.Unauthorized();
                        return;
                    }



                        Results.Json("Success!");
                    //Results.Json("Success!");
                });

                


            }
            catch (Exception ex)
            {
                _logger.Error($"Exception Initializing Endpoint: {ex.Message}");
                throw;
            }

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


        /// <summary>
        /// Must inherit interface IData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RouteGroupBuilder MapCRUD<T>(String endpoint) where T : class, IData
        {
            if (_app == null) throw new ArgumentNullException(nameof(_app));

           

            var mapGroup = _app.MapGroup(endpoint);

            // GET /api/customers and /api/customers/{id}: JSON, or XML when the request's Accept header
            // asks for it (Accept: application/xml).
            string name = endpoint.TrimEnd('/')[(endpoint.TrimEnd('/').LastIndexOf('/') + 1)..];
            mapGroup.MapGet("/", (HttpRequest request, SqliteStore store) =>
                Respond(request, store.All<T>(), name));
            mapGroup.MapGet("{id:guid}", (Guid id, HttpRequest request, SqliteStore store) =>
                store.Get<T>(id) is { } data ? Respond(request, data) : Results.NotFound());

            // POST: an ID from the client is kept (devices create their own GUIDs); none gets a new one.
            mapGroup.MapPost("/", (T data, LibSqlLite.SqliteStore store) =>
            {
                StampIfMissing(data, existing: null);
                if (!IsValid(data, out var errors)) return Results.ValidationProblem(errors);

                

                try
                {
                    //return store.Update(customer) ? Results.Ok(customer) : Results.NotFound();
                    store.Insert(data);
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    return Results.Conflict(ConflictMessage<T>(ex));
                }
                return Results.Created($"{endpoint.TrimEnd('/')}/{data.ID}", data);

                
            });


            mapGroup.MapPut("/{id:guid}", (Guid id, T data, SqliteStore store) =>
            {

                data.ID = id;
                var existing = store.Get<T>(id);
                if (existing == null) return Results.NotFound();
                StampIfMissing(data, existing);
                if (!IsValid(data, out var errors)) return Results.ValidationProblem(errors);
                try { return store.Update(data) ? Results.Ok(data) : Results.NotFound(); }
                catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    return Results.Conflict(ConflictMessage<T>(ex));
                }
            });

            return mapGroup;
        }


        /// <summary>True when the request's Accept header asks for XML (application/xml or text/xml).</summary>
        private static bool WantsXml(HttpRequest request) =>
            request.GetTypedHeaders().Accept is { Count: > 0 } accept &&
            accept.Any(type => type.MediaType.Value is "application/xml" or "text/xml");

        /// <summary>A list of records as JSON, or as XML (under a <paramref name="root"/> element) if asked.</summary>
        private static IResult Respond<T>(HttpRequest request, IEnumerable<T> items, string root) where T : class =>
            WantsXml(request) ? Results.Content(XmlFormat.Write(items, root), XmlFormat.ContentType) : Results.Json(items);

        /// <summary>A single record as JSON, or as XML if asked.</summary>
        private static IResult Respond<T>(HttpRequest request, T item) where T : class =>
            WantsXml(request) ? Results.Content(XmlFormat.Write(item), XmlFormat.ContentType) : Results.Json(item);

        /// <summary>
        /// TOC / TOM / MOP for a record written through the API. A device that sends them (it made the
        /// change, so it knows when and where) is kept as is; a request without them is stamped here.
        /// An update keeps the stored TOC unless the request has one.
        /// </summary>
        private static void StampIfMissing(object data, object? existing)
        {
            if (data is not IAudited audited) return;

            if (audited.TOC == default && existing is IAudited stored) audited.TOC = stored.TOC;
            if (audited.TOM == default || string.IsNullOrWhiteSpace(audited.MOP)) Audit.Stamp(data, created: existing == null);
        }

        /// <summary>
        /// A readable message for a SQLite constraint error (code 19), e.g. for
        /// "UNIQUE constraint failed: Customer.Name": "A Customer with that Name already exists."
        /// </summary>
        private static string ConflictMessage<T>(Microsoft.Data.Sqlite.SqliteException ex)
        {
            string type = typeof(T).Name;

            // SQLite's foreign key error names no column: "FOREIGN KEY constraint failed".
            if (ex.Message.Contains("FOREIGN KEY constraint failed", StringComparison.OrdinalIgnoreCase))
                return $"The {type} refers to a record that doesn't exist.";

            // SQLite's text, e.g. "SQLite Error 19: 'UNIQUE constraint failed: Customer.Name'."
            var match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"(\w+) constraint failed: ([^']+)");
            if (!match.Success) return $"The {type} conflicts with existing data.";

            string kind = match.Groups[1].Value;
            var columns = match.Groups[2].Value
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(column => column.Contains('.') ? column[(column.LastIndexOf('.') + 1)..] : column);

            return kind.Equals("UNIQUE", StringComparison.OrdinalIgnoreCase)
                ? $"A {type} with that {string.Join(" and ", columns)} already exists."
                : $"The {type} breaks a {kind} rule on {string.Join(", ", columns)}.";
        }

        private static bool IsValid(Object item, out Dictionary<String, String[]> errors)
        {
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool valid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                item, new System.ComponentModel.DataAnnotations.ValidationContext(item), results, validateAllProperties: true);
            errors = results.GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
                .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Invalid value.").ToArray());

            return valid;
        }


        public void Run()
        {
            if (!_initialized)
                throw new Exception($"Factory is not initialized");


            if (_app == null)
                throw new ArgumentNullException(nameof(_app));


            
            _app.Run();
        }
        public static class ApiKeyUtility
        {
            public static string GenerateApiKey()
            {
                byte[] keyBytes = RandomNumberGenerator.GetBytes(32);

                return Convert.ToBase64String(keyBytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }

            public static byte[] HashApiKey(string apiKey)
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(apiKey);
                return SHA256.HashData(keyBytes);
            }

            public static bool ValidateApiKey(
                string suppliedApiKey,
                byte[] storedHash)
            {
                byte[] suppliedHash = HashApiKey(suppliedApiKey);

                return CryptographicOperations.FixedTimeEquals(
                    suppliedHash,
                    storedHash
                );
            }
        }

    }
}
