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
    public partial class UCNewProjectCode : Lib.UCSubPage
    {
        public UCNewProjectCode()
        {
            InitializeComponent();
            base.MinSplitterX = 298;
        }

        protected override void PostInit()
        {
            if (_datFactory == null)
            {
                throw new ArgumentNullException(nameof(_datFactory));
            }

            _cbCustomer.PostInit(_datFactory.GetData<Customers>());
            _projCodesDgv.PostInit(_datFactory.GetData<ProjectCodes>(), _datFactory);

            var timeEntries = _datFactory.GetData<TimeEntries>();
            _deleteBar.PostInit(_projCodesDgv, _datFactory.GetData<ProjectCodes>()!, "project code", "project codes", codes =>
            {
                // Time entries keep showing a deleted code (and the timesheets keep their hours).
                var ids = codes.Select(c => c.ID).ToHashSet();
                int used = timeEntries?.All.Unfiltered.Count(entry => ids.Contains(entry.ProjectCodeID)) ?? 0;
                return used == 0 ? null
                    : $"{used} time entr{(used == 1 ? "y uses" : "ies use")} {(codes.Count == 1 ? "this project code" : "these project codes")}. " +
                      "They will keep showing it, but it can't be picked for new time entries.";
            });

        }

        private void _btnAdd_Click(object sender, EventArgs e)
        {


            // check the values...
            String custName = _cbCustomer.Text.Trim();
            String location = _tbLocation.Text.Trim();
            String projCode = _tbProjectCode.Text.Trim();
            String projDescription = _tbDescription.Text.Trim();

            if (custName.Length <= 0 || projCode.Length <= 0)
            {
                MessageBox.Show($"Project Code and / or Customer Name must have a value");
                return;
            }


            var custs = _datFactory!.GetData<Customers>();
            if (custs == null) throw new ArgumentNullException(nameof(custs));


            Customers.Customer? customer;
            if (!custs.CustomerExistsByName(custName))
            {
                customer = new Customers.Customer()
                {
                    Name = custName,
                };

                if (!custs.Save(customer))
                {
                    MessageBox.Show($"Failed to add new customer {custName}");
                    return;
                }
               
            }
            else
            {
                customer = custs.GetCustomerByName(custName);

                // Typing a deleted customer's name brings that customer back.
                if (customer != null && customer.Deleted && !custs.Restore(customer))
                {
                    MessageBox.Show($"Failed to add new customer {custName}");
                    return;
                }
            }

            if (customer == null)
                throw new Exception($"Failed to retrieve customer information {custName}");

            ProjectCodes.ProjectCode newCode = new ProjectCodes.ProjectCode()
            {
                Code = projCode,
                Location = location,
                CustomerID = customer.ID,
                Description = projDescription,
            };

            var projCodesFactory = _datFactory.GetData<ProjectCodes>();
            if (projCodesFactory == null) throw new Exception($"Failed to retrieve project codes factory");

            // Re-adding a deleted code brings that project code back with the details just entered
            // (same ID, so its old time entries line up with it again).
            var deletedCode = projCodesFactory.GetByCode(projCode);
            if (deletedCode != null && deletedCode.Deleted)
            {
                deletedCode.Location = newCode.Location;
                deletedCode.CustomerID = newCode.CustomerID;
                deletedCode.Description = newCode.Description;

                if (!projCodesFactory.Restore(deletedCode))
                {
                    MessageBox.Show($"Failed to create project code: {projCode}");
                }
                return;
            }

            if (!projCodesFactory.Save(newCode))
            {
                MessageBox.Show($"Failed to create project code: {projCode}, perhaps a duplicate?");
            }

        }
    }
}
