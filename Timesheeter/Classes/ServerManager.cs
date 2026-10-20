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


            //customers.All.ListChanged += All_ListChanged;
            customers.ItemChanged += ItemChanged;
            projectCodes.ItemChanged += ItemChanged;
            timeEntries.ItemChanged += ItemChanged;
        }

        private void ItemChanged<T>(object? sender, ItemEventArgs<T> e) where T : class
        {

            if (e.Item == null)
            {
                _logger?.Warn($"Item Changed But Data Not Handled");
                return;
            }

            if (e.Item is IData data)
            {
                _logger?.Verbose($"{typeof(T)} Changed: {data.ID} - {e.ChangeType.ToString()}");
            



                if (e.Item is Customers.Customer customer)
                {
                    _logger?.Verbose($"Customer Name: {customer.Name}");
                }
                else if (e.Item is ProjectCodes.ProjectCode projectCode)
                {
                    _logger?.Verbose($"Project Code: {projectCode.Code}");
                }
                else if (e.Item is TimeEntries.TimeEntry timeEntry)
                {
                    _logger?.Verbose($"Time Entry Changed...");
                }
                else
                {
                    _logger?.Error($"Unknown Type Change! {typeof(T)}");
                    return;
                }
            }
            else
            {
                _logger?.Error($"Uknown Data Type: {typeof(T)}");
                return;
            }


        }

        /*
        private void All_ListChanged(object? sender, System.ComponentModel.ListChangedEventArgs e)
        {

            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemChanged)
            {
                // 
                _logger?.Verbose($"Item Changed");
            }

        }
        */
    }
}
