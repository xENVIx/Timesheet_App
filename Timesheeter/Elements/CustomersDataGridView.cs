using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Elements
{
    internal class CustomersDataGridView : DataGridViewData<Customers.Customer>
    {


        public CustomersDataGridView()
        {

        }

        // Renamed to a deleted customer's name: that customer comes back and takes over this one's
        // project codes, and this one is deleted (keeping its old name).
        protected override Func<bool>? MergeOntoDeleted(Customers.Customer item)
        {
            if (_data is not Customers customers || _factory?.GetData<ProjectCodes>() is not ProjectCodes projectCodes) return null;
            if (customers.GetDeletedByName(item.Name, item) == null) return null;
            return () => customers.MergeRenamedIntoDeleted(item, projectCodes);
        }


    }
}
