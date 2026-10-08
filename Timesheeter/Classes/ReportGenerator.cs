using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;
using Timesheeter.Extensions;

namespace Timesheeter.Classes
{
    public static class ReportGenerator
    {

        public static bool Generate(
            IWin32Window? owner,
            String firstName,
            String lastName,
            String projectCode,
            String projectPurpose,
            String description,
            String customerName,
            String location,
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
            try
            {
                var ws = workbook.Worksheet("CW");

                ws.GetNamedRangeCell("Engineer").Value = $"{firstName} {lastName}";
                ws.GetNamedRangeCell("Customer").Value = customerName;
                ws.GetNamedRangeCell("ProjLineName").Value = projectPurpose;
                ws.GetNamedRangeCell("Purpose").Value = description;
                ws.GetNamedRangeCell("Location").Value = location;
                ws.GetNamedRangeCell("ProjCode").Value = projectCode;



                // get any random date from the list...?
                DateOnly randomDate = _reportEntries.First().Date;

                DateOnly monday = randomDate.AddDays(-(((int)randomDate.DayOfWeek + 6) % 7));




                String dateFormat = "{0:00}/{1:00}/{2:0000}";
                String fileDateFormat = "{0:0000}{1:00}{2:00}";
                String date = String.Format(dateFormat, monday.Month, monday.Day, monday.Year);

                ws.GetNamedRangeCell("Start_Date").Value = monday.ToDateTime(TimeOnly.MinValue);


                var totalsByDate = _reportEntries.GroupBy(x => x.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalHours = g.Sum(x => Math.Round(x.Hours, 2)),
                        Comments = String.Join(", ",
                        g.Select(x => x.Comment)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct())

                    })
                    .OrderBy(x => x.Date)
                    .ToList();



                foreach (var record in totalsByDate)
                {

                    const String hoursRangeFormat = "Hours{0}";
                    const String commentsRangeFormat = "DailyActivityDescription{0}";

                    ws.GetNamedRangeCell(String.Format(hoursRangeFormat, record.Date.DayOfWeek.ToString()))
                        .Value = record.TotalHours;

                    ws.GetNamedRangeCell(String.Format(commentsRangeFormat, record.Date.DayOfWeek.ToString()))
                        .Value = record.Comments;


                }


                using SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                dlg.DefaultExt = "xlsx";
                dlg.AddExtension = true;
                dlg.FileName =
                    $"{String.Format(fileDateFormat, monday.Year, monday.Month, monday.Day)}_" +
                    $"{projectCode}_" +
                    $"{customerName}_" +
                    $"Timesheet_" +
                    $"{firstName}_{lastName}" +
                    $".xlsx";




                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    bool loop = false;
                    do
                    {
                        loop = false;
                        try
                        {
                            workbook.SaveAs(dlg.FileName);
                        }
                        catch (IOException exx)
                        {
                            if (exx.Message.Contains("because it is being used by another process."))
                            {
                                var res = MessageBox.Show($"{exx.Message}\n\nPlease Close The File And Try Again.",
                                    "File Opened In Another App",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Exclamation);

                                if (res == DialogResult.OK)
                                {
                                    loop = true;
                                }

                            }
                            else throw;
                        }
                    } while (loop);
                }
            }
            catch
            {

            }
            finally
            {

                stream.Close();
                stream.Dispose();
                workbook.Dispose();
            }


           

            return true;
        }


    }
}
