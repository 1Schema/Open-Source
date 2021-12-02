using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public class RenderedReport : RenderedObjectBase<RenderedReport, RenderedRange>
    {
        public RenderedReport(RenderedLayout layout)
            : base(layout)
        {
            if ((layout.Report == null) || (layout.Element != null))
            { throw new InvalidOperationException("The specified Layout does not have a Report as its Template."); }

            GridLine_ColorSpec = layout.Report.OutsideAreaStyle.ForeColor_Value;
            OutsideArea_ColorSpec = layout.Report.OutsideAreaStyle.BackColor_Value;
        }

        public RenderedRange GetRangeForElement(ReportElementId elementId)
        {
            if (this.RenderingMode != RenderingMode.Design)
            { throw new InvalidOperationException("This method can only be called in Design Mode."); }

            var rangesForElement = GetRangesForElement(elementId);
            if (rangesForElement.Count != 1)
            { throw new InvalidOperationException("The specified Element did not render correctly."); }

            var range = rangesForElement.Values.First();
            return range;
        }

        public IDictionary<RenderingKey, RenderedRange> GetRangesForElement(ReportElementId elementId)
        {
            var ranges = new Dictionary<RenderingKey, RenderedRange>();

            foreach (var child in Children.Values)
            {
                ExploreChildRanges(child, elementId, ranges);
            }

            return ranges;
        }

        private void ExploreChildRanges(RenderedRange range, ReportElementId elementId, Dictionary<RenderingKey, RenderedRange> results)
        {
            if (range.ElementId == elementId)
            {
                results.Add(range.Key, range);
            }

            foreach (var child in range.Children.Values)
            { ExploreChildRanges(child, elementId, results); }
        }

        #region Display Sizing Methods

        public List<double> GetColumnWidths()
        { return GetDimensionalDisplaySizes(Dimension.X); }

        public List<double> GetRowHeights()
        { return GetDimensionalDisplaySizes(Dimension.Y); }

        public List<double> GetDimensionalDisplaySizes(Dimension dimension)
        {
            var displaySizes = new List<double>();
            var numberOfDisplays = (dimension == Dimension.X) ? (this.Abs_Location.X + this.Abs_ViewArea.Width + this.Abs_Location.X) : (this.Abs_Location.Y + this.Abs_ViewArea.Height + this.Abs_Location.Y);
            var defaultDisplaySize = CellLayoutManagerUtils.GetDefaultDisplaySize(dimension);

            for (int i = 0; i < numberOfDisplays; i++)
            { displaySizes.Add(defaultDisplaySize); }

            (this as IRenderedObject).UpdateDimensionalDisplaySizes(dimension, displaySizes);
            return displaySizes;
        }

        #endregion

        public ColorSpecification GridLine_ColorSpec { get; set; }
        public int GridLine_Color_Alpha { get { return GridLine_ColorSpec.Alpha; } }
        public int GridLine_Color_Red { get { return GridLine_ColorSpec.Red; } }
        public int GridLine_Color_Green { get { return GridLine_ColorSpec.Green; } }
        public int GridLine_Color_Blue { get { return GridLine_ColorSpec.Blue; } }

        public Color GridLine_Color
        {
            get { return GridLine_ColorSpec.GetColor(); }
            set { GridLine_ColorSpec = new ColorSpecification(value); }
        }

        public ColorSpecification OutsideArea_ColorSpec { get; set; }
        public int OutsideArea_Color_Alpha { get { return OutsideArea_ColorSpec.Alpha; } }
        public int OutsideArea_Color_Red { get { return OutsideArea_ColorSpec.Red; } }
        public int OutsideArea_Color_Green { get { return OutsideArea_ColorSpec.Green; } }
        public int OutsideArea_Color_Blue { get { return OutsideArea_ColorSpec.Blue; } }

        public Color OutsideArea_Color
        {
            get { return OutsideArea_ColorSpec.GetColor(); }
            set { OutsideArea_ColorSpec = new ColorSpecification(value); }
        }

        public void Save(IReportingDataProvider dataProvider, string filename)
        {
            if (!AreLayoutValuesCached)
            { SetCachedLayoutValues(); }

            var exporter = new ExcelExporter(dataProvider);
            var excelPackage = exporter.ExportReport(this);

            var fileInfo = new FileInfo(filename);
            var folderInfo = fileInfo.Directory;

            if (!folderInfo.Exists)
            { Directory.CreateDirectory(folderInfo.FullName); }

            excelPackage.SaveAs(fileInfo);
        }
    }
}