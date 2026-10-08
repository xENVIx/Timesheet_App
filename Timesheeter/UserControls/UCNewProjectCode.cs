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
        }

        protected override void PostInit()
        {
            if (_factory == null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            _cbCustomer.PostInit(_factory.GetData<Customers>());
            _projCodesDgv.PostInit(_factory.GetData<ProjectCodes>(), _factory);

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


            var custs = _factory!.GetData<Customers>();
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

            var projCodesFactory = _factory.GetData<ProjectCodes>();
            if (projCodesFactory == null) throw new Exception($"Failed to retrieve project codes factory");

            if (!projCodesFactory.Save(newCode))
            {
                MessageBox.Show($"Failed to create project code: {projCode}, perhaps a duplicate?");
            }

        }
    }
}
