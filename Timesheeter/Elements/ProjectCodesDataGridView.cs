using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Data;

namespace Timesheeter.Elements
{
    public class ProjectCodesDataGridView : DataGridViewData<ProjectCodes.ProjectCode>
    {


        public ProjectCodesDataGridView() : base()
        {


            base.CellEndEdit += ProjectCodesDataGridView_CellEndEdit;


        }

        private void ProjectCodesDataGridView_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {

            int colInd = e.ColumnIndex;
            int rowInd = e.RowIndex;

            if (rowInd < 0) return;

            if (colInd < 0) return;

            // bweiss TODO need to find the row that was modified?  The data that is modified is not getting saved...?


        }
    }
}
