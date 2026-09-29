using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Data;

namespace Timesheeter.UserControls
{
    public partial class UCTimeEntries : Lib.UCSubPage
    {
        public UCTimeEntries() : base()
        {
            InitializeComponent();
        }

        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            _cbProjCodes.PostInit(_factory.GetData<ProjectCodes>());
            _dgvTimeEntries.PostInit(_factory.GetData<TimeEntries>(), _factory);

            // Default to a one hour entry starting now.

            _tpEnd.Time = _tpStart.Time.AddHours(1);

        }

        private void _btnAdd_Click(object sender, EventArgs e)
        {
            if (_factory == null) throw new ArgumentNullException(nameof(_factory));

            var timeEntries = _factory.GetData<TimeEntries>();
            if (timeEntries == null) throw new Exception("Failed to retrieve time entries factory");

            var entry = new TimeEntries.TimeEntry()
            {
                Date = _dtpDate.Date,
                TimeStart = _tpStart.Time,
                TimeEnd = _tpEnd.Time,
                ProjectCodeID = _cbProjCodes.SelectedValue is long id ? id : 0,
            };

            var errors = timeEntries.GetValidationErrors(entry);
            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors));
                return;
            }

            if (!timeEntries.Save(entry))
            {
                MessageBox.Show("Failed to save the time entry.");
                return;
            }

            // Ready for the next entry: it starts where this one ended.
            _tpStart.Time = entry.TimeEnd;
            _tpEnd.Time = entry.TimeEnd.AddHours(1);
        }
    }
}
