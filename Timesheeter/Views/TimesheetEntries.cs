using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheeter.Data;
using Timesheeter.Lib;

namespace Timesheeter.Views
{
    public class TimesheetEntries
    {


        public class TimesheetEntry
        {

            public long EntryID { get; set; }
            


        }

        private IDataFactory _data;

        private Data.ProjectCodes? _codes;
        private Data.TimeEntries? _entries;
        private Data.Customers? _customers;

        public TimesheetEntries(IDataFactory data)
        {
            _data = data;

            _codes = _data.GetData<Data.ProjectCodes>();
            _entries = _data.GetData<Data.TimeEntries>();
            _customers = _data.GetData<Data.Customers>();



        }









    }
}
