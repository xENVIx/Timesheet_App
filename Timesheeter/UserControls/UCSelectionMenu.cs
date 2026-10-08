using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Lib;
using Timesheeter.Core.Interfaces;
using Timesheeter.Core.Lib;

namespace Timesheeter.UserControls
{
    public partial class UCSelectionMenu : UserControl
    {
        public UCSelectionMenu()
        {
            InitializeComponent();

            _ucNewProjectCode.Dock = DockStyle.Fill;
            _ucNewProjectCode.Visible = false;
            _ucNewProjectCode.Enabled = false;

            _ucCustomers.Dock = DockStyle.Fill;
            _ucCustomers.Visible = false;
            _ucCustomers.Enabled = false;

            _ucTimeEntries.Dock = DockStyle.Fill;
            _ucTimeEntries.Visible = false;
            _ucTimeEntries.Enabled = false;

            _ucTimesheet.Dock = DockStyle.Fill;
            _ucTimesheet.Visible = true;
            _ucTimesheet.Enabled = true;

            _ucSettings.Dock = DockStyle.Fill;
            _ucSettings.Visible = false;
            _ucSettings.Enabled = false;

            _ucCreateTimesheet.Dock = DockStyle.Fill;
            _ucCreateTimesheet.Visible = false;
            _ucCreateTimesheet.Enabled = false;

            Theme.SetRole(panel1, ThemeRole.Navigation);
            Theme.SetActiveNavigationButton(_btnTimeSheet);
        }

        public void PostInit(IDataFactory factory)
        {
            _ucNewProjectCode.PostInit(factory);
            _ucCustomers.PostInit(factory);
            _ucTimeEntries.PostInit(factory);
            _ucTimesheet.PostInit(factory);
            _ucCreateTimesheet.PostInit(factory);
            _ucSettings.PostInit(factory);
        }

        private void ToggleScreen<T>()
        {
            foreach (var control in _pnlControls.Controls)
            {

                if (control is UCSubPage page)
                {

                    if (typeof(T) != page.GetType())
                    {
                        page.Visible = false;
                        page.Enabled = false;
                    }
                    else
                    {
                        page.Visible = true;
                        page.Enabled = true;
                    }


                }


            }



        }

        private void _btnCodes_Click(object sender, EventArgs e)
        {
            this.ToggleScreen<UCNewProjectCode>();
            Theme.SetActiveNavigationButton(_btnCodes);





            //_ucNewProjectCode.Enabled = true;
            //_ucNewProjectCode.Visible = true;
        }

        private void _btnCustomers_Click(object sender, EventArgs e)
        {

            this.ToggleScreen<UCCustomers>();
            Theme.SetActiveNavigationButton(_btnCustomers);
            //_ucCustomers.Enabled = true;
            //_ucCustomers.Visible = true;
        }

        private void _btnTimeEntries_Click(object sender, EventArgs e)
        {
            this.ToggleScreen<UCTimeEntries>();
            Theme.SetActiveNavigationButton(_btnTimeEntries);
            //_ucTimeEntries.Enabled = true;
            //_ucTimeEntries.Visible = true;
        }

        private void _btnSettings_Click(object sender, EventArgs e)
        {
            this.ToggleScreen<UCSettings>();
            Theme.SetActiveNavigationButton(_btnSettings);
        }

        private void _btnTimeSheet_Click(object sender, EventArgs e)
        {
            this.ToggleScreen<UCTimesheet>();
            Theme.SetActiveNavigationButton(_btnTimeSheet);
            //_ucTimesheet.Enabled = true;
            //_ucTimesheet.Visible = true;
        }

        private void _btnCreateTimesheet_Click(object sender, EventArgs e)
        {
            this.ToggleScreen<UCCreateTimesheet>();
            Theme.SetActiveNavigationButton(_btnCreateTimesheet);
        }
    }
}
