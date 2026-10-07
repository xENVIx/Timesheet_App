using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Data;
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


        }


        protected override void PostInit()
        {
            if (_factory == null) throw new ArgumentNullException(nameof(_factory));

            // Subscribed before the grid's first build, so the chart starts with this week's hours.
            _dgvTimesheet.PostInit(_factory);
            _dgvTimesheet.SetStartDate(DateOnly.FromDateTime(_datePicker.Value));

        }

        
        private void _datePickerChanged(object sender, EventArgs e)
        {

            int daysSinceMonday = ((int)_datePicker.Value.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

            DateTime thisMonday = _datePicker.Value.AddDays(-daysSinceMonday);
            //_datePicker.Value = thisMonday;

            _dgvTimesheet.SetStartDate(DateOnly.FromDateTime(thisMonday));

            
        }



    }
}
