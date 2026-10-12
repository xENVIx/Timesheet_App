using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Elements
{
    public class CustomerComboBox : DataComboBox<Customers.Customer>
    {


        public CustomerComboBox() : base()
        {

        }

        /// <summary>Binds to the customers that aren't deleted: a deleted one can't be picked.</summary>
        internal void PostInit(Customers? customers)
        {
            base.PostInit(customers);
            DataSource = customers!.Active;
            Text = "";
        }



    }
}
