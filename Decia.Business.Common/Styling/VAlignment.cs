using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml.Style;

namespace Decia.Business.Common.Styling
{
    public enum VAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public static class VAlignmentUtils
    {
        public static ExcelVerticalAlignment GetExcelValue(this VAlignment vAlignment)
        {
            if (vAlignment == VAlignment.Top)
            { return ExcelVerticalAlignment.Top; }
            else if (vAlignment == VAlignment.Middle)
            { return ExcelVerticalAlignment.Center; }
            else if (vAlignment == VAlignment.Bottom)
            { return ExcelVerticalAlignment.Bottom; }
            else
            { throw new InvalidOperationException("Unrecognized VAlignment encountered."); }
        }
    }
}