using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.UserControls
{
    public partial class UCTimeEntries : Lib.UCSubPage
    {

        public UCTimeEntries() : base()
        {
            InitializeComponent();
            base.MinSplitterX = 298;
        }

        protected override void PostInit()
        {
            if (_datFactory == null)
            {
                throw new ArgumentNullException(nameof(_datFactory));
            }

            _cbProjCodes.PostInit(_datFactory.GetData<ProjectCodes>(), _datFactory.GetData<Customers>());
            _dgvTimeEntries.PostInit(_datFactory.GetData<TimeEntries>(), _datFactory);

            _deleteBar.PostInit(_dgvTimeEntries, _datFactory.GetData<TimeEntries>()!, "time entry", "time entries");

            // Default to a one hour entry starting now.

            _tpEnd.Time = _tpStart.Time.AddHours(1);

        }

        private void _btnAdd_Click(object sender, EventArgs e)
        {
            if (_datFactory == null) throw new ArgumentNullException(nameof(_datFactory));

            var timeEntries = _datFactory.GetData<TimeEntries>();
            if (timeEntries == null) throw new Exception("Failed to retrieve time entries factory");

            var entry = new TimeEntries.TimeEntry()
            {
                Date = _dtpDate.Date,
                TimeStart = _tpStart.Time,
                TimeEnd = _tpEnd.Time,
                ProjectCodeID = _cbProjCodes.SelectedValue is Guid id ? id : Guid.Empty,
                Comment = _tbComment.Text,
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
