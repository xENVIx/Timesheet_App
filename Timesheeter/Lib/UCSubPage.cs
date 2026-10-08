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

        public UCSubPage()
        {
            InitializeComponent();

            Theme.SetRole(_pnlSideBar, ThemeRole.Sidebar);

            this.Visible = true;

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
    }
}
