using LibLogging;
using LibSqlLite;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

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

        public Factory()
        {
            _cts = new CancellationTokenSource();
        }

        public void Initialize(string[] args)
        {
            /* initialize database...
            */
            _logger = new Logger("Timesheeter.Server", LogLevel.Trace, _cts.Token, true);


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

                
                

            }
            catch (Exception ex)
            {
                _logger.Error($"Exception Ensuring Tables Exist");
                _logger.Exception(ex);
                throw;
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
                
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception Initializing Endpoint: {ex.Message}");
                throw;
            }

            _initialized = true;
        }

        /*
        private void InitializeCustomersEndpoint()
        {
            if (_app == null) throw new ArgumentNullException(nameof(_app));

            var customers = _app.MapGroup("/api/customers");
            customers.MapGet("/", (SqliteStore store) => store.All<Customers.Customer>());
            customers.MapGet("{id:long}", (long id, SqliteStore store) =>
                store.Get<Customers.Customer>(id) is { } customer ? Results.Ok(customer) : Results.NotFound());

            customers.MapPost("/", (Customers.Customer customer, LibSqlLite.SqliteStore store) =>
            {
                if (!IsValid(customer, out var errors)) return Results.ValidationProblem(errors);

                try
                {
                    //return store.Update(customer) ? Results.Ok(customer) : Results.NotFound();
                    store.Insert(customer);
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    return Results.Conflict($"A customer named \'{customer.Name}\' already exists.");
                }
                return Results.Created($"/api/customers/{customer.ID}", customer);
            });


            customers.MapPut("/{id:long}", (long id, Customers.Customer customer, SqliteStore store) =>
            {
                customer.ID = id;
                if (!IsValid(customer, out var errors)) return Results.ValidationProblem(errors);
                try { return store.Update(customer) ? Results.Ok(customer) : Results.NotFound(); }
                catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    return Results.Conflict($"A customer named \'{customer.Name}\' already exists.");
                }
            });
        }
        */

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

            // Reading is only through the format endpoints: GET /api/customers.json and
            // /api/customers.xml give every record in that format, or with ?id=<guid> just that one
            // (404 if there's no such record).
            string name = endpoint.TrimEnd('/')[(endpoint.TrimEnd('/').LastIndexOf('/') + 1)..];
            _app.MapGet($"{endpoint.TrimEnd('/')}.json", (Guid? id, SqliteStore store) =>
                id is { } key
                    ? store.Get<T>(key) is { } item ? Results.Json(item) : Results.NotFound()
                    : Results.Json(store.All<T>()))
                .WithTags(name);
            _app.MapGet($"{endpoint.TrimEnd('/')}.xml", (Guid? id, SqliteStore store) =>
                id is { } key
                    ? store.Get<T>(key) is { } item ? Results.Content(XmlFormat.Write(item), XmlFormat.ContentType) : Results.NotFound()
                    : Results.Content(XmlFormat.Write(store.All<T>(), name), XmlFormat.ContentType))
                .WithTags(name);

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
                return Results.Created($"{endpoint.TrimEnd('/')}.json?id={data.ID}", data);

                
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


    }
}
