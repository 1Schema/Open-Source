using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using ReportClass = Decia.Business.Domain.Reporting.Report;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public class RenderedLayout
    {
        public static readonly int Report_ZOrder = ReportClass.Default_ZOrder;

        public RenderedLayout()
        {
            FastId = Guid.NewGuid();

            StructuralSpace = StructuralSpace.GlobalStructuralSpace;
            StructuralPoint = null;
            TimeKey = MultiTimePeriodKey.DimensionlessTimeKey;

            Report = null;
            Element = null;
            NestedLayouts = new List<RenderedLayout>();
            NestedGroups = new SortedDictionary<Dimension, SortedDictionary<int, RenderingGroup>>();

            foreach (Dimension dimension in Enum.GetValues(typeof(Dimension)))
            { NestedGroups.Add(dimension, new SortedDictionary<int, RenderingGroup>()); }

            OverriddenContentGroups = new SortedDictionary<Dimension, int>();

            Value = null;
            Location = new Point(0, 0);
            ActualSize = new Size(0, 0);
        }

        public Guid FastId { get; protected set; }

        [Key]
        public RenderingKey RenderingKey
        {
            get { return new RenderingKey(ElementId, StructuralPoint, TimeKey); }
        }

        public ReportElementId ElementId
        {
            get
            {
                if (Element != null)
                { return Element.Key; }
                else if (Report != null)
                { return Report.KeyAsElementId; }
                else
                { throw new InvalidOperationException("The RenderedLayout was not properly configured."); }
            }
        }
        public StructuralSpace StructuralSpace { get; set; }
        public Nullable<StructuralPoint> StructuralPoint { get; set; }
        public MultiTimePeriodKey TimeKey { get; set; }

        public IReport Report { get; set; }
        public IReportElement Element { get; set; }
        public List<RenderedLayout> NestedLayouts { get; protected set; }
        public SortedDictionary<Dimension, SortedDictionary<int, RenderingGroup>> NestedGroups { get; protected set; }

        public RenderingMode RenderingMode
        {
            get { return StructuralPoint.HasValue ? RenderingMode.Production : RenderingMode.Design; }
        }

        public bool IsReport
        {
            get
            {
                if (Report == Element)
                { return true; }
                return (Element == null);
            }
        }

        public bool IsContainer
        {
            get
            {
                if (Element != null)
                { return Element.IsContainer; }
                else if (Report != null)
                { return Report.IsContainer; }
                else
                { throw new InvalidOperationException("The RenderedLayout was not properly configured."); }
            }
        }

        public bool IsHidden
        {
            get
            {
                if (IsReport)
                { return false; }
                if (!(Element is IHidableElement))
                { return false; }
                return (Element as IHidableElement).IsHidden;
            }
        }

        public int ZOrder
        {
            get
            {
                if (Element != null)
                { return Element.ZOrder; }
                else if (Report != null)
                { return Report_ZOrder; }
                else
                { throw new InvalidOperationException("The RenderedLayout was not properly configured."); }
            }
        }

        public IElementLayout TemplateLayout
        {
            get
            {
                if (Element != null)
                { return Element.ElementLayout; }
                else if (Report != null)
                { return Report.ReportAreaLayout; }
                else
                { throw new InvalidOperationException("The RenderedLayout was not properly configured."); }
            }
        }

        public IElementStyle TemplateStyle
        {
            get
            {
                if (Element != null)
                { return Element.ElementStyle; }
                else if (Report != null)
                { return Report.ReportAreaStyle; }
                else
                { throw new InvalidOperationException("The RenderedLayout was not properly configured."); }
            }
        }

        public ICollection<Dimension> ContainerDimensions
        {
            get { return TemplateLayout.GetContainerDimensions_Grid(); }
        }

        public SortedDictionary<Dimension, int> OverriddenContentGroups { get; set; }

        public SortedDictionary<Dimension, int> MergedContentGroups
        {
            get
            {
                var templateLayout = TemplateLayout;
                var mergedContentGroups = new SortedDictionary<Dimension, int>();

                foreach (var dimension in templateLayout.Dimensions)
                {
                    if (OverriddenContentGroups.ContainsKey(dimension))
                    { mergedContentGroups.Add(dimension, OverriddenContentGroups[dimension]); }
                    else
                    { mergedContentGroups.Add(dimension, templateLayout.GetDimensionLayout(dimension).ContentGroup_Value); }
                }
                return mergedContentGroups;
            }
        }

        public object Value { get; set; }

        public Point Location { get; set; }

        public Size ActualSize { get; set; }

        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public int ActualWidth { get { return ActualSize.Width; } }
        public int ActualHeight { get { return ActualSize.Height; } }
    }
}