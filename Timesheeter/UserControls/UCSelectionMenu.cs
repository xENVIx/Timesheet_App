using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Lib;

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
        }

        public void PostInit(IFactory factory)
        {
            _ucNewProjectCode.PostInit(factory);
            _ucCustomers.PostInit(factory);
            _ucTimeEntries.PostInit(factory);
        }

        private void _btnCodes_Click(object sender, EventArgs e)
        {
            _ucNewProjectCode.Enabled = true;
            _ucNewProjectCode.Visible = true;
        }

        private void _btnCustomers_Click(object sender, EventArgs e)
        {
            _ucCustomers.Enabled = true;
            _ucCustomers.Visible = true;
        }

        private void _btnTimeEntries_Click(object sender, EventArgs e)
        {
            _ucTimeEntries.Enabled = true;
            _ucTimeEntries.Visible = true;
        }
    }
}
