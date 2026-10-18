using LibLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Classes
{
    public class ServerManager
    {

        private IDataFactory _data;
        private ILoggerApp? _logger;

        public ServerManager(IDataFactory data, ILoggerApp? logger)
        {

            _data = data;
            _logger = logger;


            var customers = _data.GetData<Customers>() ?? throw new Exception($"Reference To Customers Returned Null");
            var projectCodes = _data.GetData<ProjectCodes>() ?? throw new Exception("Reference To ProjectCodes Returned Null");
            var timeEntries = _data.GetData<TimeEntries>() ?? throw new Exception("Reference To TimeEntries Returned Null");


            customers.All.ListChanged += All_ListChanged;
            
        }

        private void All_ListChanged(object? sender, System.ComponentModel.ListChangedEventArgs e)
        {

            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemChanged)
            {
                // 
                _logger?.Verbose($"Item Changed");
            }

        }
    }
}
