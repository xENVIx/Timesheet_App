using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Timesheeter.Lib
{
    public partial class UCSubPage : UserControl
    {

        protected IFactory? _factory;

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

        public void PostInit(IFactory factory)
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
