using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Domain.Formulas;
using OfficeOpenXml;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public interface IExcelExporter
    {
        IFormulaDataProvider DataProvider { get; }

        ExcelPackage ExportReport(RenderedReport report);
    }
}