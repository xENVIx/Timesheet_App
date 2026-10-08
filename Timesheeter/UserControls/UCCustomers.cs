using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.UserControls
{
    public partial class UCCustomers : Lib.UCSubPage
    {
        public UCCustomers() : base()
        {
            InitializeComponent();

            base.MinSplitterX = 161;
        }

        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            //_cbCustomer.PostInit(_factory.GetData<Customers>());
            _custDgv.PostInit(_factory.GetData<Customers>(), _factory);
        }

        private void _btnSave_Click(object sender, EventArgs e)
        {
            if (_tbCustName.Text.Length <= 0)
            {
                MessageBox.Show($"Customer Name Cannot Be Blank");
                return;
            }    

            if (_factory == null) throw new ArgumentNullException(nameof(_factory));

            var customers = _factory.GetData<Customers>();
            if (customers != null)
            {

                String custNameTrim = _tbCustName.Text.Trim();

                if (customers.CustomerExistsByName(_tbCustName.Text))
                {
                    MessageBox.Show($"Customer Name {custNameTrim} Already Exists");
                    _tbCustName.Text = "";
                    return;
                }

                if (!customers.Save(new Customers.Customer()
                {
                    Name = custNameTrim,
                }))
                {
                    MessageBox.Show($"Error Saving New Customer");
                    return;
                }

            }
            else
                throw new ArgumentNullException(nameof(customers));
        }
    }
}
