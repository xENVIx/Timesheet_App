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

        //private delegate void ScreenToggle<T>();
        private Dictionary<string, Action> _actionList = new();
        // also the original width...
        private const int MIN_NAV_PANEL_WIDTH = 101;
        private int _navButtonOriginalWidth = -1;
        private int _btnBuffer = -1;

        public UCSelectionMenu()
        {




            InitializeComponent();

            _btnCodes.SelectedPage = _ucNewProjectCode;
            //_btnCodes.ButtonClickedTarget = "Codes";
            _btnCreateTimesheet.SelectedPage = _ucCreateTimesheet;
            //_btnCreateTimesheet.ButtonClickedTarget = "CreateTimesheet";
            _btnCustomers.SelectedPage = _ucCustomers;
            //_btnCustomers.ButtonClickedTarget = "Customers";
            
            _btnSettings.SelectedPage = _ucSettings;
            _btnTimeEntries.SelectedPage = _ucTimeEntries;
            _btnTimeSheet.SelectedPage = _ucTimesheet;

            _btnCustomers.BaseClicked += _navBtn_Clicked;
            


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
            _ucTimesheet.Selected = true;

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


            _actionList = new Dictionary<string, Action>()
            {
                { _ucSettings.Name, ToggleScreen<UCSettings> },
                { _ucCreateTimesheet.Name, ToggleScreen<UCCreateTimesheet> },
                { _ucCustomers.Name, ToggleScreen<UCCustomers> },
                { _ucNewProjectCode.Name, ToggleScreen<UCNewProjectCode> },
                { _ucTimeEntries.Name, ToggleScreen<UCTimeEntries> },
                { _ucTimesheet.Name, ToggleScreen<UCTimesheet> }
            };
        }

        private void _navBtn_Clicked(object? sender, Elements.BaseButtonClickedEventArgs e)
        {


            if (_actionList.TryGetValue(e.Name, out Action? action))
            {
                action?.Invoke();
                Theme.SetActiveNavigationButton(e.Button);
                
            }

            


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
            UCSubPage? current = null;
            UCSubPage? next = null;

            foreach (var control in _pnlControls.Controls)
            {
                if (control is not UCSubPage page) continue;

                if (page.GetType() == typeof(T)) next = page;
                else if (page.Selected) current = page;
            }

            if (next == null || next.Selected) return;

            if (current != null) current.Selected = false;
            next.Selected = true;

            // Fades the current page out and the next one in (and hides/shows them).
            PageTransition.Switch(_pnlControls, current, next);
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
