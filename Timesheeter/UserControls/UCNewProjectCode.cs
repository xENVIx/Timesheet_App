using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Data;

namespace Timesheeter.UserControls
{
    public partial class UCNewProjectCode : Lib.UCSubPage
    {
        public UCNewProjectCode()
        {
            InitializeComponent();
        }

        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            _cbCustomer.PostInit(_factory.GetData<Customers>());

        }
    }
}
