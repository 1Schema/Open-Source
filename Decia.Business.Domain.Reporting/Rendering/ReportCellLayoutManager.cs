using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public class ReportCellLayoutManager : CellLayoutManager<Report, RenderedReport>
    {
        public ReportCellLayoutManager(Report orginalReport, RenderedReport renderedReport)
            : base(orginalReport, renderedReport, (RenderedReport r) => r.GetColumnWidths(), (RenderedReport r) => r.GetRowHeights())
        { }

        public Report OrginalReport { get { return OrginalContainer; } }
        public RenderedReport RenderedReport { get { return RenderedContainer; } }
    }
}