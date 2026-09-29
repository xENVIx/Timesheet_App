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
    public partial class UCTimeEntries : Lib.UCSubPage
    {
        public UCTimeEntries() : base()
        {
            InitializeComponent();
        }

        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            //_cbCustomer.PostInit(_factory.GetData<Customers>());
            //_projCodesDgv.PostInit(_factory.GetData<ProjectCodes>(), _factory);

        }

        
    }
}
