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
            if (_datFactory == null)
            {
                throw new ArgumentNullException(nameof(_datFactory));
            }

            //_cbCustomer.PostInit(_factory.GetData<Customers>());
            _custDgv.PostInit(_datFactory.GetData<Customers>(), _datFactory);

            var projectCodes = _datFactory.GetData<ProjectCodes>();
            _deleteBar.PostInit(_custDgv, _datFactory.GetData<Customers>()!, "customer", "customers", customers =>
            {
                // Project codes keep showing a deleted customer's name; just say how many there are.
                var ids = customers.Select(c => c.ID).ToHashSet();
                int used = projectCodes?.Active.Count(code => ids.Contains(code.CustomerID)) ?? 0;
                return used == 0 ? null
                    : $"{used} project code{(used == 1 ? " uses" : "s use")} {(customers.Count == 1 ? "this customer" : "these customers")}. " +
                      "They will keep showing the customer's name, but it can't be picked for new project codes.";
            });
        }

        private void _btnSave_Click(object sender, EventArgs e)
        {
            if (_tbCustName.Text.Length <= 0)
            {
                MessageBox.Show($"Customer Name Cannot Be Blank");
                return;
            }    

            if (_datFactory == null) throw new ArgumentNullException(nameof(_datFactory));

            var customers = _datFactory.GetData<Customers>();
            if (customers != null)
            {

                String custNameTrim = _tbCustName.Text.Trim();

                var existing = customers.GetCustomerByName(custNameTrim);
                if (existing != null && existing.Deleted)
                {
                    // Re-adding a deleted customer's name brings that customer back (same ID, so
                    // everything that referred to it lines up again).
                    if (!customers.Restore(existing)) MessageBox.Show($"Error Saving New Customer");
                    return;
                }

                if (existing != null)
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
