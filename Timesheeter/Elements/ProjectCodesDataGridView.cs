using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Elements
{
    public class ProjectCodesDataGridView : DataGridViewData<ProjectCodes.ProjectCode>
    {


        public ProjectCodesDataGridView() : base()
        {


        }

        // Given a deleted project code's code: that project code comes back and takes over this one's
        // time entries, and this one is deleted (keeping its old code).
        protected override Func<bool>? MergeOntoDeleted(ProjectCodes.ProjectCode item)
        {
            if (_data is not ProjectCodes projectCodes || _factory?.GetData<TimeEntries>() is not TimeEntries timeEntries) return null;
            if (projectCodes.GetDeletedByCode(item.Code, item) == null) return null;
            return () => projectCodes.MergeRenamedIntoDeleted(item, timeEntries);
        }

    }
}
