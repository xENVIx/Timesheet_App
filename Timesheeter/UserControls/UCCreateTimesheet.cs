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

            // Its own binding context: controls bound to the same list in one window otherwise share a
            // "current item", so another page's project selection would move this one (and undo -1).
            _cbProjects.BindingContext = new BindingContext();
            _cbProjects.PostInit(_factory.GetData<ProjectCodes>(), _factory.GetData<Customers>());
            _cbProjects.SelectedIndex = -1;

            _dgvTimeEntries.PostInit(_timeEntries, _factory);
            // Show only this page's filtered list (empty until a project is picked).
            _dgvTimeEntries.DataSource = _weekEntries;

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

        private void _btnGenerate_Click(object sender, EventArgs e)
        {


            if (_weekEntries.Count <= 0) return;



        }
    }
}
