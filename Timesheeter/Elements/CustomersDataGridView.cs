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

        // Renamed to another customer's name (deleted or not): that customer is kept (brought back if
        // deleted) and takes over this one's project codes, and this one is deleted (keeping its old name).
        protected override Func<bool>? MergeOnRename(Customers.Customer item)
        {
            if (_data is not Customers customers || _factory?.GetData<ProjectCodes>() is not ProjectCodes projectCodes) return null;
            if (customers.GetOtherByName(item.Name, item) == null) return null;
            return () => customers.MergeRenamed(item, projectCodes);
        }


    }
}
