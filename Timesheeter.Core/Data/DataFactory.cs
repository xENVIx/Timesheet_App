using LibLogging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheeter.Core.Data
{
    public class DataFactory : Interfaces.IDataFactory
    {

        
        
        
        private Customers? _customers;
        private ProjectCodes? _projectCodes;
        private TimeEntries? _timeEntries;
        private AppSettings? _appSettings;


        private ILoggerApp? _logger;
        private LibSqlLite.SqliteStore _store;
        

        public DataFactory(LibSqlLite.SqliteStore store, ILoggerApp? logger = null)
        {
            _store = store;
            _logger = logger;

            _customers = new Customers(_store, logger);
            _projectCodes = new ProjectCodes(_store, logger);
            _timeEntries = new TimeEntries(_store, logger);
            _appSettings = new AppSettings(_store, logger);

        }


        public T? GetData<T>()
        {

            switch (typeof(T))
            {
                case Type t when t == typeof(Customers):
                    
                    return _customers != null ? (T)(object)_customers : default(T);
                case Type t when t == typeof(ProjectCodes):
                    return _projectCodes != null ? (T)(object)_projectCodes : default(T);
                case Type t when t == typeof(TimeEntries):
                    return _timeEntries != null ? (T)(object)_timeEntries : default(T);
                case Type t when t == typeof(AppSettings):
                    return _appSettings != null ? (T)(object)_appSettings : default(T);
                default: return default(T);


            }

        }

        public Object? GetData(Type dataType)
        {

            if (dataType == typeof(Customers)) return _customers;
            else if (dataType == typeof(ProjectCodes)) return _projectCodes;
            else if (dataType == typeof(TimeEntries)) return _timeEntries;
            else if (dataType == typeof(AppSettings)) return _appSettings;
            else return null;

        }




        public String[] AvailableDataTypes()
        {

            return new List<string>()
            {
                nameof(Customers),
                nameof(ProjectCodes),
                nameof(TimeEntries),
                nameof(AppSettings)
            }.ToArray();


        }




    }
}
