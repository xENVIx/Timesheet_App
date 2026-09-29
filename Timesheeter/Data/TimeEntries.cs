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

        public class TimeEntry : IValidatableObject
        {

            [PrimaryKey, GridHidden] public long ID { get; set; }
            [Required, DisplayName("Entry Date")] public DateOnly Date { get; set; }
            [Required, DisplayName("Entry Start Time")] public TimeOnly TimeStart { get; set; }
            [Required, DisplayName("Entry End Time")] public TimeOnly TimeEnd { get; set; }
            [Required, DisplayName("Project Code"), GridLookup(typeof(ProjectCodes), "Code")] public long ProjectCodeID { get; set; }

            // [Required] can't catch these: value types always have a value, so check them here.
            // DataClass.IsValid runs this on every Save and Update.
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                // Also enforces that an entry can't cross midnight: it must end later the same day.
                if (TimeEnd <= TimeStart)
                    yield return new ValidationResult("End time must be after start time.", [nameof(TimeEnd)]);

                if (ProjectCodeID <= 0)
                    yield return new ValidationResult("A project code must be selected.", [nameof(ProjectCodeID)]);
            }


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
