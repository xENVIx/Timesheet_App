using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Timesheeter.Data;
using Timesheeter.Lib;
using Timesheeter.UserControls;

namespace Timesheeter.Elements
{
    /// <summary>
    /// A week of hours: one row per project code, one column per day (Monday first), built from
    /// the time entries. Read-only; it rebuilds itself when time entries change.
    /// </summary>
    public class TimesheetDataGridView : DataGridView
    {

        IFactory? _factory;
        private DateOnly _dateSelected;

        private TimeEntries? _timeEntriesFactory;
        private ProjectCodes? _projectCodesFactory;
        private List<TimeEntries.TimeEntry> _timeEntries = new List<TimeEntries.TimeEntry>();
        private int _dateRow = -1;

        private volatile bool _postInitOccured = false;
        private bool _populated = false;
        private Font? _dateRowFont;

        public TimesheetDataGridView() : base()
        {
            // The grid is a report built from the time entries, not bound to them, so edits
            // here would never be saved.
            ReadOnly = true;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            RowHeadersVisible = false;
        }


        public void PostInit(IFactory factory)
        {
            _factory = factory;

            _timeEntriesFactory = _factory.GetData<TimeEntries>();
            if (_timeEntriesFactory == null) throw new ArgumentNullException(nameof(_timeEntriesFactory));

            _projectCodesFactory = _factory.GetData<ProjectCodes>();
            if (_projectCodesFactory == null) throw new ArgumentNullException(nameof(_projectCodesFactory));

            // Keep the week up to date when entries are added or edited on the time entries page.
            _timeEntriesFactory.All.ListChanged += (s, e) => Rebuild();
            _projectCodesFactory.All.ListChanged += (s, e) => Rebuild();

            _postInitOccured = true;
        }

        public void SetStartDate(DateOnly dateOnly)
        {
            // The date picker can set the date before PostInit, so also build the first time
            // after PostInit even when the date hasn't changed.
            if (dateOnly == _dateSelected && _populated) return;

            _dateSelected = dateOnly;
            Rebuild();
        }

        private void Rebuild()
        {
            if (!_postInitOccured) return;

            PopulateTimeEntries();
            PopulateList();
            _populated = true;
        }

        private void PopulateTimeEntries()
        {
            DateOnly lastDay = _dateSelected.AddDays(6);

            // Unfiltered: a filter on the time entries page must not remove hours from the timesheet.
            _timeEntries = _timeEntriesFactory!.All.Unfiltered
                .Where(entry => entry.Date >= _dateSelected && entry.Date <= lastDay)
                .ToList();
        }


        private void PopulateList()
        {
            Rows.Clear();

            CreateBaseHeading();

            // One row per project code, with that code's hours summed per day.
            var byProjectCode = _timeEntries
                .GroupBy(t => t.ProjectCodeID)
                .Select(g => new
                {
                    ProjectCode = _projectCodesFactory![g.Key]?.Code ?? $"(unknown {g.Key})",
                    HoursByDate = g
                        .GroupBy(t => t.Date)
                        .ToDictionary(d => d.Key, d => d.Sum(x => (x.TimeEnd - x.TimeStart).TotalHours)),
                })
                .OrderBy(p => p.ProjectCode, StringComparer.OrdinalIgnoreCase);

            foreach (var project in byProjectCode)
            {
                int newEntryRow = Rows.Add();
                Rows[newEntryRow].Cells[0].Value = project.ProjectCode;

                for (int i = 1; i < 8; i++)
                {
                    if (Columns[i].Tag is DateOnly day && project.HoursByDate.TryGetValue(day, out double hours))
                    {
                        Rows[newEntryRow].Cells[i].Value = hours.ToString("0.##");
                    }
                }
            }
        }

        private void CreateBaseHeading()
        {

            // ProjCode | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday
            // **       | Date   | Date    | Date      | Date     | Date   | Date     | Date

            // Built once; later weeks only change the dates.
            if (Columns.Count == 0)
            {
                Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "_colProjCode",
                    HeaderText = "Project Code"
                });

                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                foreach (var day in days)
                {
                    Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = $"_col{day}",
                        HeaderText = day
                    });
                }

                // Sorting would move the date row away from the top.
                foreach (DataGridViewColumn column in Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }

            _dateRow = Rows.Add();
            Rows[_dateRow].Frozen = true; // stays at the top when scrolling
            _dateRowFont ??= new Font(Font, FontStyle.Bold);
            Rows[_dateRow].DefaultCellStyle.Font = _dateRowFont;

            for (int i = 1; i < 8; i++)
            {
                DateOnly date = _dateSelected.AddDays(i - 1);

                // The column's Tag is what hours are matched against; the date row is just for display.
                Columns[i].Tag = date;
                Rows[_dateRow].Cells[i].Value = date.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _dateRowFont?.Dispose();
            base.Dispose(disposing);
        }


    }
}
