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
using System.Drawing.Text;

namespace Timesheeter.UserControls
{
    public partial class UCSelectionMenu : UserControl
    {

        // also the original width...
        private const int MIN_NAV_PANEL_WIDTH = 101;
        private int _navButtonOriginalWidth = -1;
        private int _btnBuffer = -1;

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

            // just pick one button...
            _navButtonOriginalWidth = _btnCodes.Width;
            _btnBuffer = MIN_NAV_PANEL_WIDTH - _navButtonOriginalWidth;

            Theme.SetRole(_panelNav, ThemeRole.Navigation);
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

        private void _pnlSplitter_MarginChanged(object sender, EventArgs e)
        {

        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Theme.Apply(this);

            if (_panelNav.Width < MIN_NAV_PANEL_WIDTH) _panelNav.Width = MIN_NAV_PANEL_WIDTH;
        }

        private void splitter1_SplitterMoving(object sender, SplitterEventArgs e)
        {
            if (e.SplitX < MIN_NAV_PANEL_WIDTH) e.SplitX = MIN_NAV_PANEL_WIDTH;
        }


        private void ResizePanelButtons()
        {


            int newWidth = _panelNav.Width - _btnBuffer;
            foreach (var control in _panelNav.Controls)
            {

                // panelWidth - curWidth = buffer
                // bufferSize = buffer / 2



                if (control is Button btn)
                {
                    btn.Size = new Size(newWidth, btn.Size.Height);
                }

            }


        }

        private void _panelNav_Resize(object sender, EventArgs e)
        {


            ResizePanelButtons();


        }
    }
}
