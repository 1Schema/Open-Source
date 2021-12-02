using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public class RenderingGroup
    {
        private Nullable<Dimension> m_Dimension;

        public RenderingGroup()
        {
            FastId = Guid.NewGuid();

            SourceLayout = null;

            ContainedLayouts = new Dictionary<RenderingKey, RenderedLayout>(ReportRenderingEngine.EqualityComparer_RenderingKey);
            ContainedGroups = new Dictionary<RenderingKey, SortedDictionary<Dimension, SortedDictionary<int, RenderingGroup>>>(ReportRenderingEngine.EqualityComparer_RenderingKey);

            m_Dimension = null;
            GroupNumber = 0;
        }

        public Guid FastId { get; protected set; }

        public RenderingKey RenderingKey
        {
            get
            {
                var sourceKey = SourceRenderingKey;
                return new RenderingKey(sourceKey.ReportElementId, sourceKey.StructuralPoint, sourceKey.TimeKey, Dimension, GroupNumber);
            }
        }

        public RenderedLayout SourceLayout { get; set; }
        public RenderingKey SourceRenderingKey { get { return SourceLayout.RenderingKey; } }

        public bool IsContainer { get { return SourceLayout.IsContainer; } }
        public bool IsHidden { get { return SourceLayout.IsHidden; } }

        public Dictionary<RenderingKey, RenderedLayout> ContainedLayouts { get; protected set; }
        public Dictionary<RenderingKey, SortedDictionary<Dimension, SortedDictionary<int, RenderingGroup>>> ContainedGroups { get; protected set; }

        public Dimension Dimension
        {
            get { return m_Dimension.Value; }
            set { m_Dimension = value; }
        }

        public int GroupNumber { get; set; }

        public bool CanUtilizeExtraSpace
        {
            get
            {
                if (ContainedLayouts.Count <= 0)
                { return true; }

                var nestedStretches = ContainedLayouts.Values.Select(l => l.TemplateLayout.GetDimensionLayout(Dimension).AlignmentMode_Value == AlignmentMode.Stretch).Count();
                if (nestedStretches > 0)
                { return true; }

                var maxGroupNumber = SourceLayout.NestedGroups[Dimension].Keys.Max();

                if (GroupNumber != maxGroupNumber)
                { return false; }

                var otherCanStretch = false;
                foreach (var group in SourceLayout.NestedGroups[Dimension].Values)
                {
                    if (group.GroupNumber == maxGroupNumber)
                    { continue; }

                    if (group.CanUtilizeExtraSpace)
                    { otherCanStretch = true; }
                }
                return !otherCanStretch;
            }
        }

        public Size ActualSize { get; set; }

        public bool IgnoreLayoutWhenSizing(RenderingKey nestedLayoutKey)
        {
            if (!ContainedLayouts.ContainsKey(nestedLayoutKey))
            { throw new InvalidOperationException("The specified NestedLayout in not contained within the current Group."); }

            var nestedLayout = ContainedLayouts[nestedLayoutKey];
            return (!this.IsHidden && nestedLayout.IsHidden);
        }
    }
}