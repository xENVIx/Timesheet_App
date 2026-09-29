using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Data;

namespace Timesheeter
{
    internal class Factory : Lib.IFactory
    {



        private LibSqlLite.SqliteStore _store;
        private Data.Customers _customers;
        private Data.ProjectCodes _projectCodes;
        private TimeEntries _timeEntries;
        private readonly System.Windows.Forms.Timer _timer;


        internal Factory()
        {


            String dbFile = Path.Combine(AppContext.BaseDirectory, "Timesheeter.db");
            _store = new LibSqlLite.SqliteStore(dbFile);
            _customers = new Data.Customers(_store);
            _projectCodes = new ProjectCodes(_store);
            _timeEntries = new TimeEntries(_store);

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 10000;
            _timer.Tick += _timer_Tick;
            
            _timer.Start();


        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            
        }


        public T? GetData<T>()
        {


            if (typeof(T) == typeof(Customers)) return (T)(object)_customers;
            else if (typeof(T) == typeof(ProjectCodes)) return (T)(object)_projectCodes;
            else if (typeof(T) == typeof(TimeEntries)) return (T)(object)_timeEntries;
            return default(T);

        }


        public Object? GetData(Type dataType)
        {

            if (dataType == typeof(Customers)) return _customers;
            else if (dataType == typeof(ProjectCodes)) return _projectCodes;
            else if (dataType == typeof(TimeEntries)) return _timeEntries;
            else return null;

        }
        

        internal void Run()
        {




            Application.Run(new UserInterface(this));
        }


    }
}
