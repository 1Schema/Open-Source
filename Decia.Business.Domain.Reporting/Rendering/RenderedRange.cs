using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public class RenderedRange : RenderedObjectBase<RenderedReport, RenderedRange>
    {
        public RenderedRange(RenderedLayout layout)
            : this(layout, new Point(0, 0))
        { }

        public RenderedRange(RenderedLayout layout, Point groupOffset)
            : base(layout, groupOffset)
        {
            if (layout.Element == null)
            { throw new InvalidOperationException("The specified Layout does not have a Report Element as its Template."); }

            DirectValue = layout.Value;
        }

        public bool HasFormula { get { return false; } }
        public bool CanRenderFormula { get { return false; } }
        public string ExcelFormula { get { return string.Empty; } }

        public object DirectValue { get; set; }
    }
}