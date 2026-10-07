using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timesheeter.Data;
using Timesheeter.Lib;

namespace Timesheeter.UserControls
{

    

    public partial class UCCreateTimesheet : Lib.UCSubPage
    {

        private TimeEntries? _timeEntries;
        private readonly BindingListView<TimeEntries.TimeEntry> _weekEntries = new();

        public UCCreateTimesheet()
        {
            InitializeComponent();
        }


        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            
            _timeEntries = _factory.GetData<TimeEntries>();

            _cbProjects.PostInit(_factory.GetData<ProjectCodes>());
            _cbProjects.SelectedIndex = -1;

            _dgvTimeEntries.PostInit(_timeEntries, _factory);
            _dgvTimeEntries.DataSource = _timeEntries;

            _weekEntries.Sort = "Date, TimeStart";

            _dp.ValueChanged += (s, e) => ShowSelectedWeek();
            _cbProjects.SelectedIndexChanged += (s, e) => ShowSelectedWeek();
        }

        // select week
        // select project code

        private void ShowSelectedWeek()
        {
            _weekEntries.Clear();

            if (_timeEntries == null || _cbProjects.SelectedValue is not long projectCodeId) return;

            DateOnly picked = _dp.Date;
            DateOnly monday = picked.AddDays(-(((int)picked.DayOfWeek + 6) % 7));
            DateOnly sunday = monday.AddDays(6);

            foreach (var entry in _timeEntries.All.Unfiltered)
            {
                if (entry.ProjectCodeID == projectCodeId && entry.Date >= monday && entry.Date <= sunday)
                {
                    _weekEntries.Add(entry);
                }
            }
        }



    }
}
