using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timesheeter.Lib;

namespace Timesheeter.Forms
{
    public partial class FrmTimesheetUserInfo : Form
    {

        private String _fn;
        private String _ln;
        private String _etc;

        public FrmTimesheetUserInfo()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Abort;

        }



        public static (String firstName, String lastName)? ShowAndReturnUserInfo(IWin32Window? owner = null)
        {

            FrmTimesheetUserInfo frm = new FrmTimesheetUserInfo();
            Theme.Apply(frm);

            var dlg = frm.ShowDialog(owner);
            if (dlg == DialogResult.OK)
            {
                return (frm._fn, frm._ln);
            }

            return null;
        }

        private void _btnOk_Click(object sender, EventArgs e)
        {
            if (_tbFirstName.Text.Length > 0 && 
                _tbLastName.Text.Length > 0)
            {
                _fn = _tbFirstName.Text;
                _ln = _tbLastName.Text;


                this.DialogResult = DialogResult.OK;
                
                
            }



            base.Close();
        }
    }
}
