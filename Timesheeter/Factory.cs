using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Timesheeter.Core.Data;

namespace Timesheeter
{
    internal class Factory
    {



        private LibSqlLite.SqliteStore _store;
        //private Data.Customers _customers;
        //private Data.ProjectCodes _projectCodes;
        //private TimeEntries _timeEntries;
        //private AppSettings _settings;

        private DataFactory _dataFactory;
        private readonly System.Windows.Forms.Timer _timer;


        internal Factory()
        {


            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String timeSheeterPath = Path.Combine(documentsPath, "Timersheeter");

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

            /*
            _customers = new Data.Customers(_store);
            _projectCodes = new ProjectCodes(_store);
            _timeEntries = new TimeEntries(_store);
            _settings = new AppSettings(_store);
            */

            _dataFactory = new DataFactory(_store);

            // Before any window exists, so the first paint already uses the saved look.
            // Done here rather than in Core's DataFactory: the theme is WinForms-only.
            var settings = _dataFactory.GetData<AppSettings>();
            if (settings != null) Lib.Theme.Set(settings.ThemeMode, settings.Accent);

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 10000;
            _timer.Tick += _timer_Tick;
            
            _timer.Start();

            

            String getAssembly = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.Contains("Timesheet.xlsx")).First() ?? "";
            if (getAssembly.Length <= 0) throw new Exception($"Error Pulling Template Timesheet from Assembly - Project Report Creation Impossible");       }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            
        }


        /*
        public T? GetData<T>()
        {

            switch (typeof(T))
            { 
                case Type t when t == typeof(Customers):
                    return (T)(object)_customers;
                case Type t when t == typeof(ProjectCodes):
                    return (T)(object)_projectCodes;
                case Type t when t == typeof(TimeEntries):
                    return (T)(object)_timeEntries;
                case Type t when t == typeof(AppSettings):
                    return (T)(object)_settings;
                // just some easter eggs for fun, not really needed
                case Type t when t == typeof(float):
                    return (T)(object)8008135f;
                case Type t when t == typeof(string):
                    return (T)(object)"8======D";
                // end easter eggs
                default: return default(T);

                 
            }
            
        } */


        /*public Object? GetData(Type dataType)
        {

            if (dataType == typeof(Customers)) return _customers;
            else if (dataType == typeof(ProjectCodes)) return _projectCodes;
            else if (dataType == typeof(TimeEntries)) return _timeEntries;
            else if (dataType == typeof(AppSettings)) return _settings;
            else return null;

        } */

        
        

        internal void Run(Forms.FrmSplash? splash = null)
        {
            var ui = new UserInterface(_dataFactory);
            splash?.CloseWhenShown(ui);

            Application.Run(ui);

            _store.Dispose();
            Environment.Exit(0);
        }


    }
}
