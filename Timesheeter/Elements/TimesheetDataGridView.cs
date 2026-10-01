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
    /// the time entries, with a Total column per project and a Total row per day.
    /// Read-only; it rebuilds itself when time entries change.
    /// </summary>
    public class TimesheetDataGridView : DataGridView
    {
        /// <summary>A project code's total hours for the week shown.</summary>
        public record ProjectHours(string ProjectCode, double Hours);

        /// <summary>
        /// Raised after every rebuild with each project code's total for the week, in row order
        /// (the same numbers as the Total column), e.g. for a chart.
        /// </summary>
        public event EventHandler<IReadOnlyList<ProjectHours>>? WeekTotalsChanged;


        IFactory? _factory;
        private DateOnly _dateSelected;

        private TimeEntries? _timeEntriesFactory;
        private ProjectCodes? _projectCodesFactory;
        private List<TimeEntries.TimeEntry> _timeEntries = new List<TimeEntries.TimeEntry>();
        private int _dateRow = -1;
        private int _totalRow = -1;

        private const int FirstDayColumn = 1;
        private const int TotalColumn = 8;

        private volatile bool _postInitOccured = false;
        private bool _populated = false;
        private Font? _boldFont;

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

            // Special row colours come from the theme, so rebuild when it changes.
            Theme.Changed += (s, e) => Rebuild();

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

            // One row per project code, with that code's entries per day (in start time order).
            var byProjectCode = _timeEntries
                .GroupBy(t => t.ProjectCodeID)
                .Select(g => new
                {
                    ProjectCode = _projectCodesFactory![g.Key]?.Code ?? $"(unknown {g.Key})",
                    EntriesByDate = g
                        .GroupBy(t => t.Date)
                        .ToDictionary(d => d.Key, d => d.OrderBy(x => x.TimeStart).ToList()),
                })
                .OrderBy(p => p.ProjectCode, StringComparer.OrdinalIgnoreCase);

            // Summed from the hours themselves, not the displayed (rounded) text.
            var dayTotals = new double[7];
            var projectTotals = new List<ProjectHours>();

            foreach (var project in byProjectCode)
            {
                int newEntryRow = Rows.Add();
                Rows[newEntryRow].Cells[0].Value = project.ProjectCode;

                double projectTotal = 0;
                for (int i = FirstDayColumn; i < TotalColumn; i++)
                {
                    if (Columns[i].Tag is DateOnly day && project.EntriesByDate.TryGetValue(day, out var entries))
                    {
                        double hours = entries.Sum(x => x.Hours);
                        Rows[newEntryRow].Cells[i].Value = FormatHours(hours);
                        Rows[newEntryRow].Cells[i].ToolTipText = EntriesToolTip(entries);
                        dayTotals[i - FirstDayColumn] += hours;
                        projectTotal += hours;
                    }
                }

                Rows[newEntryRow].Cells[TotalColumn].Value = FormatHours(projectTotal);
                projectTotals.Add(new ProjectHours(project.ProjectCode, projectTotal));
            }

            CreateTotalRow(dayTotals);

            WeekTotalsChanged?.Invoke(this, projectTotals);
        }

        private void CreateTotalRow(double[] dayTotals)
        {
            _totalRow = Rows.Add();
            Rows[_totalRow].DefaultCellStyle.Font = _boldFont;
            Rows[_totalRow].DefaultCellStyle.BackColor = Theme.Current.GridHighlight;
            Rows[_totalRow].Cells[0].Value = "Total";

            for (int i = FirstDayColumn; i < TotalColumn; i++)
            {
                double total = dayTotals[i - FirstDayColumn];
                if (total > 0) Rows[_totalRow].Cells[i].Value = FormatHours(total);
            }

            // Always shown, so an empty week reads as 0 rather than blank.
            Rows[_totalRow].Cells[TotalColumn].Value = FormatHours(dayTotals.Sum());
        }

        private static string FormatHours(double hours) => hours.ToString("0.##");

        /// <summary>One line per time entry, e.g. "hours: 1.5, comment: Site visit".</summary>
        private static string EntriesToolTip(IEnumerable<TimeEntries.TimeEntry> entries)
        {
            return string.Join(Environment.NewLine, entries.Select(entry =>
                string.IsNullOrWhiteSpace(entry.Comment)
                    ? $"hours: {FormatHours(entry.Hours)}"
                    : $"hours: {FormatHours(entry.Hours)}, comment: {entry.Comment.Trim()}"));
        }

        private void CreateBaseHeading()
        {

            // ProjCode | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday | Total
            // **       | Date   | Date    | Date      | Date     | Date   | Date     | Date   |
            // <code>   | hours  | ...                                                | row total
            // Total    | day totals ...                                              | week total

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

                _boldFont ??= new Font(Font, FontStyle.Bold);

                Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = "_colTotal",
                    HeaderText = "Total",
                    DefaultCellStyle = { Font = _boldFont },
                });

                // Sorting would move the date row away from the top.
                foreach (DataGridViewColumn column in Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }

            _dateRow = Rows.Add();
            Rows[_dateRow].Frozen = true; // stays at the top when scrolling
            Rows[_dateRow].DefaultCellStyle.Font = _boldFont;
            Rows[_dateRow].DefaultCellStyle.BackColor = Theme.Current.GridHighlight;

            // The empty cells under "Project Code" and "Total" are solid black in every theme.
            foreach (int column in new[] { 0, TotalColumn })
            {
                var style = Rows[_dateRow].Cells[column].Style;
                style.BackColor = Color.Black;
                style.SelectionBackColor = Color.Black;
            }

            for (int i = FirstDayColumn; i < TotalColumn; i++)
            {
                DateOnly date = _dateSelected.AddDays(i - FirstDayColumn);

                // The column's Tag is what hours are matched against; the date row is just for display.
                Columns[i].Tag = date;
                Rows[_dateRow].Cells[i].Value = date.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _boldFont?.Dispose();
            base.Dispose(disposing);
        }


    }
}
