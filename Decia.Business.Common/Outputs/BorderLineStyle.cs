using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml.Style;

namespace Decia.Business.Common.Outputs
{
    public enum BorderLineStyle
    {
        None,
        Thin,
        Medium,
        Thick,
        Double
    }

    public static class BorderLineStyleUtils
    {
        public static ExcelBorderStyle GetExcelValue(this BorderLineStyle borderLineStyle)
        {
            if (borderLineStyle == BorderLineStyle.None)
            { return ExcelBorderStyle.None; }
            else if (borderLineStyle == BorderLineStyle.Thin)
            { return ExcelBorderStyle.Thin; }
            else if (borderLineStyle == BorderLineStyle.Medium)
            { return ExcelBorderStyle.Medium; }
            else if (borderLineStyle == BorderLineStyle.Thick)
            { return ExcelBorderStyle.Thick; }
            else if (borderLineStyle == BorderLineStyle.Double)
            { return ExcelBorderStyle.Double; }
            else
            { throw new InvalidOperationException("Unrecognized BorderLineStyle encountered."); }
        }
    }
}