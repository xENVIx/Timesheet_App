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
        private LibSqlLite.SqliteStore? _store;

        /// <summary>
        /// Where the database and its backups live: the TIMESHEETER_SERVER_DATA environment variable if
        /// set (e.g. /var/lib/timesheeter on a Linux box), otherwise Documents\Timersheeter.Server.
        /// </summary>
        internal static readonly String DataFolder =
            Environment.GetEnvironmentVariable("TIMESHEETER_SERVER_DATA") is { Length: > 0 } configured
                ? configured
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Timersheeter.Server");

        private Logger? _logger;
        private CancellationTokenSource _cts;
        private ApiKeys? _apiKeys;

        // True when the command line was a "keys" command: it has run, and there's no server to start.
        private bool _commandOnly;

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

            _apiKeys = new ApiKeys(_store);

            // "Timesheeter.Server keys ..." manages API keys and exits.
            if (KeyCommands.TryRun(args, _apiKeys))
            {
                _commandOnly = true;
                _initialized = true;
                return;
            }

            // Nothing can get in without a key, so the very first start makes one. It's printed once
            // to the console only (never the log file, which outlives the console).
            if (_apiKeys.Active().Count == 0)
            {
                KeyCommands.PrintNewKey("default", _apiKeys.Create("default"));
                _logger.Info("Created the first API key, 'default' (printed to the console).");
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
            builder.Services.AddSwaggerGen(options =>
            {
                // An "Authorize" button in Swagger UI, to paste a key in for trying requests.
                var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = ApiKeys.HeaderName,
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Description = "The API key, e.g. tsk_...",
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "ApiKey",
                    },
                };
                options.AddSecurityDefinition("ApiKey", scheme);
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement { [scheme] = [] });
            });

            builder.Services.AddSingleton(_store);
            builder.Services.AddSingleton(_apiKeys);

            _app = builder.Build();


            
            // Configure the HTTP request pipeline.
            if (_app.Environment.IsDevelopment())
            {
                _app.UseSwagger();
                _app.UseSwaggerUI();
            }

            // Every /api request needs a valid key in the X-Api-Key header; without one it gets 401
            // and nothing else runs. The key's name goes along with the request (for MOP).
            var apiKeys = _apiKeys;
            _app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    var key = apiKeys.Find(context.Request.Headers[ApiKeys.HeaderName]);
                    if (key == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { error = $"A valid API key is required in the {ApiKeys.HeaderName} header." });
                        return;
                    }
                    context.Items[KeyNameItem] = key.Name;
                }
                await next(context);
            });

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

            // GET /api/customers and /api/customers/{id}: JSON, or XML when the request's Accept header
            // asks for it (Accept: application/xml).
            string name = endpoint.TrimEnd('/')[(endpoint.TrimEnd('/').LastIndexOf('/') + 1)..];
            mapGroup.MapGet("/", (HttpRequest request, SqliteStore store) =>
                Respond(request, store.All<T>(), name));
            mapGroup.MapGet("{id:guid}", (Guid id, HttpRequest request, SqliteStore store) =>
                store.Get<T>(id) is { } data ? Respond(request, data) : Results.NotFound());

            // POST: an ID from the client is kept (devices create their own GUIDs); none gets a new one.
            mapGroup.MapPost("/", (T data, HttpContext context, LibSqlLite.SqliteStore store) =>
            {
                StampIfMissing(data, existing: null, context);
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


            mapGroup.MapPut("/{id:guid}", (Guid id, T data, HttpContext context, SqliteStore store) =>
            {

                data.ID = id;
                var existing = store.Get<T>(id);
                if (existing == null) return Results.NotFound();
                StampIfMissing(data, existing, context);
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

        /// <summary>HttpContext.Items entry holding the name of the API key the request used.</summary>
        private const string KeyNameItem = "ApiKeyName";

        /// <summary>
        /// TOC / TOM / MOP for a record written through the API. A device that sends them (it made the
        /// change, so it knows when and where) is kept as is; a request without them is stamped here,
        /// with MOP naming the API key used, e.g. "Timesheeter.Server 1.0.0 @ nas (key: laptop)".
        /// An update keeps the stored TOC unless the request has one.
        /// </summary>
        private static void StampIfMissing(object data, object? existing, HttpContext context)
        {
            if (data is not IAudited audited) return;

            if (audited.TOC == default && existing is IAudited stored) audited.TOC = stored.TOC;
            if (audited.TOM == default || string.IsNullOrWhiteSpace(audited.MOP))
            {
                Audit.Stamp(data, created: existing == null);
                if (context.Items[KeyNameItem] is string keyName)
                {
                    string mop = $"{audited.MOP} (key: {keyName})";
                    audited.MOP = mop.Length <= Audit.MaxProcessLength ? mop : mop[..Audit.MaxProcessLength];
                }
            }
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

            if (_commandOnly) return;


            if (_app == null)
                throw new ArgumentNullException(nameof(_app));

            _app.Run();
        }


    }
}
