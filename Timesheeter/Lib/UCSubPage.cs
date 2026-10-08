using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Timesheeter.Core.Interfaces;
using Timesheeter.Core.Lib;

namespace Timesheeter.Lib
{
    public partial class UCSubPage : UserControl
    {

        protected IDataFactory? _factory;

        protected int MinSplitterX { get; set; } = 250;
        public UCSubPage()
        {
            InitializeComponent();

            Theme.SetRole(_pnlSideBar, ThemeRole.Sidebar);

            _pnlSideBar.Resize += _pnlSideBar_Resize;

            this.Visible = true;

        }

        protected virtual void SidebarResized()
        {

        }

        private void _pnlSideBar_Resize(object? sender, EventArgs e)
        {
            SidebarResized();

        }

        protected virtual void PostInit()
        {
            throw new NotImplementedException($"PostInit must be implemented");
        }

        public void PostInit(IDataFactory factory)
        {
            _factory = factory;
            PostInit();


        }


        private void _btnBack_Click(object sender, EventArgs e)
        {

            this.Visible = false;

        }



        private void splitter1_SplitterMoving(object sender, SplitterEventArgs e)
        {
            if (e.SplitX < MinSplitterX) e.SplitX = MinSplitterX;
        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Theme.Apply(this);
        }
    }
}
