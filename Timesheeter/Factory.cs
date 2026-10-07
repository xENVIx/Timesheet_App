using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Timesheeter.Data;

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


            String dbFile = Path.Combine(timeSheeterPath, "Timesheeter.db");
            _store = new LibSqlLite.SqliteStore(dbFile);

            /*
            _customers = new Data.Customers(_store);
            _projectCodes = new ProjectCodes(_store);
            _timeEntries = new TimeEntries(_store);
            _settings = new AppSettings(_store);
            */

            // Before any window exists, so the first paint already uses the saved look.
            //Lib.Theme.Set(_settings.ThemeMode, _settings.Accent);

            _dataFactory = new DataFactory(_store);

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

        
        

        internal void Run()
        {




            Application.Run(new UserInterface(_dataFactory));
        }


    }
}
