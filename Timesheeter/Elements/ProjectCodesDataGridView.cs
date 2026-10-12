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

        // Given another project code's code (deleted or not): that project code is kept (brought back if
        // deleted) and takes over this one's time entries, and this one is deleted (keeping its old code).
        protected override Func<bool>? MergeOnRename(ProjectCodes.ProjectCode item)
        {
            if (_data is not ProjectCodes projectCodes || _factory?.GetData<TimeEntries>() is not TimeEntries timeEntries) return null;
            if (projectCodes.GetOtherByCode(item.Code, item) == null) return null;
            return () => projectCodes.MergeRenamed(item, timeEntries);
        }

    }
}
