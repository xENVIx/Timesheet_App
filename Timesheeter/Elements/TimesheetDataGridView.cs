using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Data;
using Timesheeter.Lib;
using Timesheeter.UserControls;

namespace Timesheeter.Elements
{
    public class TimesheetDataGridView : DataGridView
    {

        private class TimesheetEntry
        {
            public String ProjectCode { get; set; }
            public DateOnly Date { get; set; }
            public Double Hours { get; set; }
        }

        IFactory? _factory;
        private DateOnly _dateSelected;

        private TimeEntries? _timeEntriesFactory;
        private List<TimeEntries.TimeEntry>? _timeEntries = new List<TimeEntries.TimeEntry>();
        private int _dateRow = -1;

        private volatile bool _postInitOccured = false;

        public TimesheetDataGridView() : base()
        {

        }


        public void PostInit(IFactory factory)
        {
            _factory = factory;

            _timeEntriesFactory = _factory.GetData<TimeEntries>();

            if (_timeEntriesFactory == null) throw new ArgumentNullException(nameof(_timeEntriesFactory));

            _postInitOccured = true;
        }

        public void SetStartDate(DateOnly dateOnly)
        {
            if (dateOnly != _dateSelected)
            {
                _dateSelected = dateOnly;

                if (_postInitOccured)
                {
                    PopulateTimeEntries();
                    PopulateList();
                }
            }
        }

        private void PopulateTimeEntries()
        {
            _timeEntries = _timeEntriesFactory!.All.Where(entry =>
            {
                DateOnly sevenDays =
                DateOnly.FromDateTime(
                    _dateSelected
                    .ToDateTime(TimeOnly.MinValue)
                    .AddDays(6)
                );

                if (entry.Date >= _dateSelected && entry.Date <= sevenDays) return true;
                return false;
            }).ToList();



            

        }


        private void PopulateList()
        {
            if (_timeEntries == null) return;

            base.Rows.Clear();

            CreateBaseHeading();

            var consolidated = _timeEntries!
            .GroupBy(t => new
            {
                t.ProjectCodeID,
                t.Date
            })
            .Select(g => new TimesheetEntry
            {
                ProjectCode = g.Key.ProjectCodeID.ToString(), // replace with actual code lookup
                Date = g.Key.Date,
                Hours = g.Sum(x =>
                (x.TimeEnd.ToTimeSpan() - x.TimeStart.ToTimeSpan()).TotalHours)
            })
            .ToList();

            if (consolidated == null) return;

            //DateOnly date = _dateSelected;
            foreach (var entry in consolidated)
            {
                int newEntryRow = this.Rows.Add();
                for (int i = 0; i < 8; i++)
                {
                    
                    if (i == 0)
                    {
                        this.Rows[newEntryRow].Cells[i].Value = entry.ProjectCode;
                    }
                    else
                    {
                        // get row date...
                        int addDay = i - 1;
                        //date = DateOnly.FromDateTime(_dateSelected.ToDateTime(TimeOnly.MinValue).AddDays(addDay));

                        
                        if (this.Rows[_dateRow].Cells[i].Tag is DateOnly dateRef && entry.Date == dateRef)
                        {
                            this.Rows[newEntryRow].Cells[i].Value = entry.Hours.ToString();
                            break;
                        }



                    }

                }

            }

            // need consolidated list of entries per project code...
        }

        private void CreateBaseHeading()
        {

            // ProjCode | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday
            // **       | Date   | Date    | Date      | Date     | Date   | Date     | Date

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colProjCode",
                HeaderText = "Project Code"
            });

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colMonday",
                HeaderText = "Monday"
            });
            

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colTuesday",
                HeaderText = "Tuesday"
            });

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colWednesday",
                HeaderText = "Wednesday"
            });

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colThursday",
                HeaderText = "Thursday"
            });

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colFriday",
                HeaderText = "Friday"
            });

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colSaturday",
                HeaderText = "Saturday"
            });

            this.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "_colSunday",
                HeaderText = "Sunday"
            });


            //Object?[] rowData = new object[8];

            _dateRow = this.Rows.Add();
            DateOnly date = _dateSelected;
            
            for (int i = 0; i < 8; i++)
            {

                //DataGridViewRowCollection dgvRows = new DataGridViewRowCollection(this)
                //{

                //};
                //DataGridViewRow newRow = new DataGridViewRow();
                //dgvRows.Add(newRow);

                

                if (i == 0)
                {
                    this.Rows[_dateRow].Cells[i].Value = null;
                }
                else
                {
                    this.Rows[_dateRow].Cells[i].Value = date.ToString();
                    this.Rows[_dateRow].Cells[i].Tag = date;
                    
                    date = DateOnly.FromDateTime(_dateSelected.ToDateTime(TimeOnly.MinValue).AddDays(1));
                }

                   
            }


        }


    }
}
