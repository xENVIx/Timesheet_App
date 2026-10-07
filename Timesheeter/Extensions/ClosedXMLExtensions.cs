using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheeter.Extensions
{
    internal static class ClosedXMLExtensions
    {

        internal static IXLCell GetNamedRangeCell(this IXLWorksheet? sheet, String key)
        {
            if (sheet == null) throw new ArgumentNullException(nameof(sheet));

            var ranges = sheet.DefinedName(key);
            if (ranges == null)
                throw new Exception($"Named Range \'{key}\' Does Not Exist");

            var range = ranges.Ranges.First();

            var cell = range.Cells().First();

            if (cell == null)
                throw new ArgumentNullException(nameof(cell));

            return cell;
        }

    }
}
