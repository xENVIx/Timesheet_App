using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Timesheeter.Data;
using System.Reflection;
using ClosedXML.Excel;

namespace Timesheeter.Classes
{
    public static class ReportGenerator
    {

        public static bool Generate(
            IWin32Window? owner,
            String firstName,
            String lastName,
            String projectCode, 
            IReadOnlyList<TimeEntries.TimeEntry> _reportEntries
        )
        {

            if (projectCode.Length <= 0)
            {
                MessageBox.Show(owner, "Invalid Project Code");
                return false;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            string resourceName = assembly
            .GetManifestResourceNames()
            .First(x => x.Contains("Timesheet.xlsx"));

            using Stream stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new Exception($"Could not find resource '{resourceName}'");

            using XLWorkbook workbook = new XLWorkbook(stream);

            var ws = workbook.Worksheet("CW");

            var ranges = ws.DefinedName("Engineer");
            if (ranges == null)
                throw new Exception($"Named Range \'Engineer\' Does Not Exist");

            var range = ranges.Ranges.First();

            var engCell = range.Cells().First();
            engCell.Value = $"{firstName} {lastName}";

            using SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
            dlg.DefaultExt = "xlsx";
            dlg.AddExtension = true;
            dlg.FileName = $"{projectCode}_Timesheet_{firstName}_{lastName}.xlsx";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                workbook.SaveAs(dlg.FileName);
            }

            stream.Close();
            stream.Dispose();
            workbook.Dispose();

           

            return true;
        }


    }
}
