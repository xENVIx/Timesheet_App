namespace Timesheeter.Server
{
    public class Factory
    {


        private WebApplication? _app;
        private volatile bool _initialized = false;
        private LibSqlLite.SqliteStore? _store;/// <summary>Where the database and its backups live (also shown on the About screen).</summary>
        internal static readonly String DataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Timersheeter.Server");

        public Factory()
        {

        }

        public void Initialize(string[] args)
        {
            /* initialize database...
            */

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

            _store = new LibSqlLite.SqliteStore(dbFile);

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

            _app = builder.Build();

            // Configure the HTTP request pipeline.
            if (_app.Environment.IsDevelopment())
            {
                _app.UseSwagger();
                _app.UseSwaggerUI();
            }

            _app.UseHttpsRedirection();

            _app.UseAuthorization();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            _app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

            _initialized = true;
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
