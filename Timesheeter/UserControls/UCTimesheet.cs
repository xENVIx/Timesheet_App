using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;
using Timesheeter.Lib;

namespace Timesheeter.UserControls
{
    public partial class UCTimesheet : UCSubPage
    {



        public UCTimesheet()
        {
            InitializeComponent();

            DateTime today = DateTime.Today;

            int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

            DateTime thisMonday = today.AddDays(-daysSinceMonday);
            _datePicker.Value = thisMonday;

            base.MinSplitterX = 126;
        }


        protected override void PostInit()
        {
            if (_datFactory == null) throw new ArgumentNullException(nameof(_datFactory));

            // Subscribed before the grid's first build, so the chart starts with this week's hours.
            _dgvTimesheet.WeekTotalsChanged += (s, totals) => _chartTimesheet.ShowTotals(totals);
            _dgvTimesheet.PostInit(_datFactory);
            _dgvTimesheet.SetStartDate(DateOnly.FromDateTime(_datePicker.Value));

            _initializedFull = true;
        }


        private void _datePickerChanged(object sender, EventArgs e)
        {

            int daysSinceMonday = ((int)_datePicker.Value.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

            DateTime thisMonday = _datePicker.Value.AddDays(-daysSinceMonday);
            //_datePicker.Value = thisMonday;

            _dgvTimesheet.SetStartDate(DateOnly.FromDateTime(thisMonday));

        }

        private void _rbHours_CheckedChanged(object sender, EventArgs e)
        {

            if (!_initializedFull)
            {
                return;
            }

            _dgvTimesheet.ToggleHoursMode();
        }

        private volatile bool _initializedFull = false;
    }
}
