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



        public UCSubPage()
        {
            InitializeComponent();

            this.Visible = true;

        }


        private void _btnBack_Click(object sender, EventArgs e)
        {

            this.Visible = false;

        }
    }
}
