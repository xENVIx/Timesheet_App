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

        private bool _deleteMode = false;
        private DataGridViewCheckBoxColumn? _delCol;

        public UCTimeEntries() : base()
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

            _cbProjCodes.PostInit(_factory.GetData<ProjectCodes>(), _factory.GetData<Customers>());
            _dgvTimeEntries.PostInit(_factory.GetData<TimeEntries>(), _factory);

            _delCol = new DataGridViewCheckBoxColumn();
            _delCol.Name = "_colDelete";
            _delCol.HeaderText = "Select Item(s) for Deletion";
            _delCol.Visible = false;
            
            _dgvTimeEntries.Columns.Add(_delCol);

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

        private void _btnDelete_Click(object sender, EventArgs e)
        {

            if (_deleteMode)
            {
                // find records selected...
                _delCol!.Visible = false;
                _btnCancel.Visible = false;
                _deleteMode = false;
            }
            else
            {
                _deleteMode = true;
                _delCol!.Visible = true;
                _btnCancel.Visible = true;
            }

        }

        private void _btnCancel_Click(object sender, EventArgs e)
        {
            _deleteMode = false;
            _delCol!.Visible = false;
            _btnCancel.Visible = false;
        }
    }
}
