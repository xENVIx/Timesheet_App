using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timesheeter.Core.Data;

using Timesheeter.Forms;
using Timesheeter.Core.Lib;
using Timesheeter.Lib;

namespace Timesheeter.UserControls
{



    public partial class UCCreateTimesheet : Lib.UCSubPage
    {

        private TimeEntries? _timeEntries;
        private ProjectCodes? _projectCodes;
        private readonly BindingListView<TimeEntries.TimeEntry> _weekEntries = new();

        // The combo box's list: only the project codes with entries in the selected week.
        private readonly BindingListView<ProjectCodes.ProjectCode> _weekProjects = new();

        // True while the combo box's list is rebuilt, so its selection changes don't each refilter.
        private bool _refreshingProjects;

        public UCCreateTimesheet()
        {
            InitializeComponent();

            base.MinSplitterX = 298;
        }


        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }


            _timeEntries = _factory.GetData<TimeEntries>();
            _projectCodes = _factory.GetData<ProjectCodes>();

            // Its own binding context: controls bound to the same list in one window otherwise share a
            // "current item", so another page's project selection would move this one (and undo -1).
            _cbProjects.BindingContext = new BindingContext();
            _cbProjects.PostInit(_projectCodes, _factory.GetData<Customers>());
            // PostInit binds every project code; show only the selected week's instead.
            _cbProjects.DataSource = _weekProjects;
            _weekProjects.Sort = "Code";

            _dgvTimeEntries.PostInit(_timeEntries, _factory);
            // Show only this page's filtered list (empty until a project is picked).
            _dgvTimeEntries.DataSource = _weekEntries;

            _weekEntries.Sort = "Date, TimeStart";

            _dp.ValueChanged += (s, e) => RefreshWeekProjects();
            _cbProjects.SelectedIndexChanged += (s, e) =>
            {
                if (!_refreshingProjects) ShowSelectedWeek();
            };

            RefreshWeekProjects();
        }

        /// <summary>Monday to Sunday of the week containing the picked date.</summary>
        private (DateOnly Monday, DateOnly Sunday) SelectedWeek()
        {
            DateOnly picked = _dp.Date;
            DateOnly monday = picked.AddDays(-(((int)picked.DayOfWeek + 6) % 7));
            return (monday, monday.AddDays(6));
        }

        /// <summary>
        /// Fills the combo box with the project codes worked in the selected week. Keeps the current
        /// project selected if it was worked that week too; otherwise nothing is selected.
        /// </summary>
        private void RefreshWeekProjects()
        {
            if (_timeEntries == null || _projectCodes == null) return;

            var (monday, sunday) = SelectedWeek();
            long? previous = _cbProjects.SelectedValue is long id ? id : null;

            var workedCodeIds = _timeEntries.All.Unfiltered
                .Where(entry => entry.Date >= monday && entry.Date <= sunday)
                .Select(entry => entry.ProjectCodeID)
                .ToHashSet();

            _refreshingProjects = true;
            try
            {
                _weekProjects.Clear();
                foreach (var code in _projectCodes.All.Unfiltered)
                {
                    if (workedCodeIds.Contains(code.ID)) _weekProjects.Add(code);
                }

                // Adding items makes the combo box select the first one; select the previous project or none.
                int index = previous is long keep ? IndexOfProject(keep) : -1;
                _cbProjects.SelectedIndex = index;
                if (index < 0) _cbProjects.Text = "";
            }
            finally
            {
                _refreshingProjects = false;
            }

            ShowSelectedWeek();
        }

        private int IndexOfProject(long projectCodeId)
        {
            for (int i = 0; i < _weekProjects.Count; i++)
            {
                if (_weekProjects[i].ID == projectCodeId) return i;
            }
            return -1;
        }

        // select week
        // select project code

        private void ShowSelectedWeek()
        {
            _weekEntries.Clear();

            if (_timeEntries == null || _cbProjects.SelectedValue is not long projectCodeId) return;

            var (monday, sunday) = SelectedWeek();

            foreach (var entry in _timeEntries.All.Unfiltered)
            {
                if (entry.ProjectCodeID == projectCodeId && entry.Date >= monday && entry.Date <= sunday)
                {
                    _weekEntries.Add(entry);
                }
            }
        }

        private void _btnGenerate_Click(object sender, EventArgs e)
        {


            if (_weekEntries.Count <= 0) return;



            String code = "";
            String customerName = "";
            String projPurpose = "";
            String description = "";
            String location = "";



            int selInd = _cbProjects.SelectedIndex;

            var item = _cbProjects.Items[selInd];

            if (item is ProjectCodes.ProjectCode projCode)
            {
                code = projCode.Code;
                customerName = _factory?.GetData<Customers>()?[projCode.CustomerID]?.Name ?? "";


                projPurpose = projCode.ProjectName;
                description = projCode.Description;
                location = projCode.Location;
            }
            else
            {
                // could not find project code...
                MessageBox.Show($"Error Validating Project Code");
                return;
            }

            var names = _factory?.GetData<AppSettings>()?.Name ?? null;

            if (names == null)
            {

                var ret = FrmTimesheetUserInfo.ShowAndReturnUserInfo(this);
                names = ret;


            }

            if (names == null)
            {
                MessageBox.Show(this, $"User Information Could Not Be Collected");
                return;
            }

            var retValue = names.Value;

            if (!Classes.ReportGenerator.Generate(
                this,
                retValue.FirstName,
                retValue.LastName,
                code,
                projPurpose,
                description,
                customerName,
                location,
                _weekEntries))
            {
                MessageBox.Show(this, $"Could Not Generate Report");
            }

        }

        private void _pnlSideBar_Resize(object sender, EventArgs e)
        {

        }
    }
}
