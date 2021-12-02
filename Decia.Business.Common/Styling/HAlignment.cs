using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml.Style;

namespace Decia.Business.Common.Styling
{
    public enum HAlignment
    {
        Left,
        Center,
        Right
    }

    public static class HAlignmentUtils
    {
        public static ExcelHorizontalAlignment GetExcelValue(this HAlignment hAlignment)
        {
            if (hAlignment == HAlignment.Left)
            { return ExcelHorizontalAlignment.Left; }
            else if (hAlignment == HAlignment.Center)
            { return ExcelHorizontalAlignment.Center; }
            else if (hAlignment == HAlignment.Right)
            { return ExcelHorizontalAlignment.Right; }
            else
            { throw new InvalidOperationException("Unrecognized HAlignment encountered."); }
        }
    }
}