using LibLogging;
using LibSqlLite;
using LibInterfaces;
using Timesheeter.Core.Data;
using static Timesheeter.Server.Classes.APIKeys;

namespace Timesheeter.Server.Classes
{
    public class AppFactory
    {

        private ILoggerApp? _logger;
        private IDataStore _store;

        private IDataFactory _datFact;


        private readonly WebApplication _app;
        private readonly CancellationToken _ct;




        public AppFactory(IDataStore store, IDataFactory datFact, ILoggerApp logger, CancellationToken ct, String[] args)
        {
            _ct = ct;
            _store = store;
            _logger = logger;
            _datFact = datFact;



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

        }

        public void Run()
        {
            _app.Run();
        }



        public void Initialize()
        {

            try
            {
                var mapGroupCustomers = MapCRUD<Customers.Customer>("/api/customers");
                var mapGroupProjectCodes = MapCRUD<ProjectCodes.ProjectCode>("/api/project_codes");
                var mapGroupTimeEntries = MapCRUD<TimeEntries.TimeEntry>("/api/time_entries");


                var mapGroupTest = _app.MapGroup("/api/test");

                // GET /api/customers and /api/customers/{id}: JSON, or XML when the request's Accept header
                // asks for it (Accept: application/xml).
                string name = "/api/test".TrimEnd('/')[("/api/test".TrimEnd('/').LastIndexOf('/') + 1)..];
                mapGroupTest.MapGet("/", (HttpRequest request) =>
                {
                    _logger?.Verbose($"Test Received");


                    foreach (var header in request.Headers)
                    {
                        _logger?.Verbose($"Header: {header.Key} || {header.Value}");
                    }

                    // get the API key...
                    if (!request.Headers.TryGetValue("X-API-Key", out var key) || !request.Headers.TryGetValue("X-Signature", out var signature))
                    {
                        
                        return Results.Unauthorized();
                    }


                    var apiKeys = _datFact.GetData<APIKeys>();
                    if (apiKeys == null)
                        return Results.NotFound();

                    if (apiKeys.Active.ToList().Exists(x => x.Key == key))
                    {
                        // compute the HMAC and COMPARE to sent SIGNATURE...
                        String secret = apiKeys.Active.Where(x => x.Key == key).First().Secret;

                        var sig = Core.Lib.Security.ComputeSignature(key!, secret);

                        if (sig == null)
                        {

                            return Results.Unauthorized();
                        }


                        if (String.Compare(sig, signature) == 0)
                        {
                            
                            return Results.Json("Success!"); 
                        }
                        else
                        {
                            
                            return Results.Unauthorized();
                        }

                    }
                    else
                    {
                        
                        return Results.Unauthorized();
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
            {
                Respond(request, store.All<T>(), name);

            });
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






    }
}
