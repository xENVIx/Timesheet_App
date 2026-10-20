using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Core.Interfaces;
using Timesheeter.Core.Lib;
using Timesheeter.Core.Data;
using LibLogging;

namespace Timesheeter.UserControls
{
    public partial class UCMain : UserControl
    {
        public UCMain()
        {
            InitializeComponent();

        }


        public void PostInit(IDataFactory factory, ILoggerApp? logger = null)
        {
            _ucSelectionMenu.PostInit(factory, logger);
        }
    }
}
