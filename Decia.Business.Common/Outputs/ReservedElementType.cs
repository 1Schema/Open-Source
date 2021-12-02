using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum ReservedElementType
    {
        Report = 0,
        AreaOutsideReport = 1,
        TitleTemplate = 2,
        HeaderTemplate = 3,
        DataTemplate = 4
    }

    public static class ReservedElementTypeUtils
    {
        public const ReservedElementType Report_ReportElementType = ReservedElementType.Report;
        public const int Report_ReportElementNumber = (int)Report_ReportElementType;
        public static readonly IEnumerable<int> Report_AllElementNumbers = Enum.GetValues(typeof(ReservedElementType)).Cast<int>();

        public static bool IsReservedElement(this int elementNumber)
        {
            return Report_AllElementNumbers.Contains(elementNumber);
        }

        public static ReservedElementType? GetReservedElementTypeForName(this string elementName)
        {
            if (string.IsNullOrWhiteSpace(elementName))
            { return null; }

            var loweredElementName = elementName.Replace(" ", "").ToLower();

            if (loweredElementName.Contains("outside"))
            { return ReservedElementType.AreaOutsideReport; }
            else if (loweredElementName.Contains("data"))
            { return ReservedElementType.DataTemplate; }
            else if (loweredElementName.Contains("header"))
            { return ReservedElementType.HeaderTemplate; }
            else if (loweredElementName.Contains("title"))
            { return ReservedElementType.TitleTemplate; }
            else if (loweredElementName == "report")
            { return ReservedElementType.Report; }
            else
            { return null; }
        }

        public static void AssertIsReservedElement(this int elementNumber)
        {
            if (!IsReservedElement(elementNumber))
            { throw new InvalidOperationException("The specified Element Number does not represent a reserved Element Number."); }
        }

        public static void AssertIsNotReservedElement(this int elementNumber)
        {
            if (IsReservedElement(elementNumber))
            { throw new InvalidOperationException("The specified Element Number does not represent a reserved Element Number."); }
        }
    }
}