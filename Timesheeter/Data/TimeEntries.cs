using LibSqlLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Timesheeter.Data.ProjectCodes;

namespace Timesheeter.Data
{
    public class TimeEntries : DataClass<TimeEntries.TimeEntry>
    {

        public TimeEntry? this[long key]
        {
            get
            {


                return null;
            }
        }

        public class TimeEntry
        {

            [PrimaryKey, GridHidden] public long ID { get; set; }
            [Required, DisplayName("Entry Date")] public DateOnly Date { get; set; }
            [Required, DisplayName("Entry Start Time")] public TimeOnly TimeStart { get; set; }
            [Required, DisplayName("Entry End Time")] public TimeOnly TimeEnd { get; set; }
            [Required, DisplayName("Customer"), GridLookup(typeof(ProjectCodes), "Code")] public long ProjectCodeID { get; set; }


        }


        public TimeEntries(SqliteStore dataStore) : base(dataStore)
        {
            _list = new BindingList<TimeEntry>();

            _dataStore.EnsureTable<TimeEntry>();

            foreach (var timeEntry in _dataStore.All<TimeEntry>())
            {
                _list.Add(timeEntry);
            }
        }


    }
}
