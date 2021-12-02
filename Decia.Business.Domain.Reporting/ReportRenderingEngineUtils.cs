using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public static class ReportRenderingEngineUtils
    {
        public const int NonContainerGroupNumber = 0;
        public const int DefaultNonContainerAutoSize = 1;

        public static void SetRenderingMode(this IReport report, IEnumerable<IReportElement> elements, RenderingMode renderingMode)
        {
            if (true)
            {
                var reportLayout = report.ReportAreaLayout;
                foreach (var dimension in reportLayout.Dimensions)
                {
                    reportLayout.GetDimensionLayout(dimension).RenderingMode = renderingMode;
                }
            }

            foreach (var element in elements)
            {
                var elementLayout = element.ElementLayout;
                foreach (var dimension in elementLayout.Dimensions)
                {
                    elementLayout.GetDimensionLayout(dimension).RenderingMode = renderingMode;
                }
            }
        }

        public static void CreateRenderingGroups(this RenderedLayout currentLayout, Dimension dimension, IDictionary<Guid, object> renderingObjects, IDictionary<Guid, Guid?> renderingHierarchy)
        {
            if (!currentLayout.IsContainer)
            { return; }

            var groupsForCurrent = currentLayout.NestedGroups[dimension];

            if (!currentLayout.ContainerDimensions.Contains(dimension))
            {
                var group = new RenderingGroup();
                group.SourceLayout = currentLayout;
                group.Dimension = dimension;
                group.GroupNumber = NonContainerGroupNumber;
                groupsForCurrent.Add(group.GroupNumber, group);

                renderingObjects.Add(group.FastId, group);
                renderingHierarchy.Add(group.FastId, currentLayout.FastId);

                foreach (var childLayout in currentLayout.NestedLayouts)
                {
                    group.ContainedLayouts.Add(childLayout.RenderingKey, childLayout);
                    group.ContainedGroups.Add(childLayout.RenderingKey, childLayout.NestedGroups);

                    if (!renderingObjects.ContainsKey(childLayout.FastId))
                    { renderingObjects.Add(childLayout.FastId, childLayout); }
                    renderingHierarchy.Add(childLayout.FastId, group.FastId);

                    CreateRenderingGroups(childLayout, dimension, renderingObjects, renderingHierarchy);
                }
            }
            else
            {
                foreach (var childLayout in currentLayout.NestedLayouts)
                {
                    var childGroupNumber = childLayout.MergedContentGroups[dimension];

                    if (!groupsForCurrent.ContainsKey(childGroupNumber))
                    {
                        var newGroup = new RenderingGroup();
                        newGroup.SourceLayout = currentLayout;
                        newGroup.Dimension = dimension;
                        newGroup.GroupNumber = childGroupNumber;
                        groupsForCurrent.Add(newGroup.GroupNumber, newGroup);

                        renderingObjects.Add(newGroup.FastId, newGroup);
                        renderingHierarchy.Add(newGroup.FastId, currentLayout.FastId);
                    }

                    var group = groupsForCurrent[childGroupNumber];

                    group.ContainedLayouts.Add(childLayout.RenderingKey, childLayout);
                    group.ContainedGroups.Add(childLayout.RenderingKey, childLayout.NestedGroups);

                    if (!renderingObjects.ContainsKey(childLayout.FastId))
                    { renderingObjects.Add(childLayout.FastId, childLayout); }
                    renderingHierarchy.Add(childLayout.FastId, group.FastId);

                    CreateRenderingGroups(childLayout, dimension, renderingObjects, renderingHierarchy);
                }
            }
        }

        public static void HandleRenderedLayout(RenderedLayout layout, Dimension dimension, Nullable<int> parentGroupSize, ITree<Guid> renderingTree, ICollection<Guid> intializedKeys)
        {
            var groups = layout.NestedGroups[dimension];
            var dimensionLayout = layout.TemplateLayout.GetDimensionLayout(dimension);

            var isParentLayoutAutoSized = true;
            var parentGroupKey = renderingTree.GetParent(layout.FastId);

            if (parentGroupKey.HasValue)
            {
                var parentLayoutKey = renderingTree.GetParent(parentGroupKey.Value);

                if (!parentLayoutKey.HasValue)
                { throw new InvalidOperationException(); }

                var parentLayout = renderingTree.GetCachedValue<RenderedLayout>(parentLayoutKey.Value);
                isParentLayoutAutoSized = (parentLayout.TemplateLayout.GetDimensionLayout(dimension).SizeMode_Value == SizeMode.Auto);
            }

            var initialLocation = GetLocation(layout, dimension);
            var initialSize = GetActualSize(layout, dimension);
            var updatedLocation = 0;
            var updatedSize = 0;

            if (dimensionLayout.SizeMode_Value == SizeMode.Cell)
            {
                updatedSize = dimensionLayout.RangeSize_Value;
                ApplyMinAndMax(layout.TemplateLayout, dimension, ref updatedSize);
            }
            else if (dimensionLayout.SizeMode_Value == SizeMode.Auto)
            {
                if (!layout.IsContainer)
                {
                    updatedSize = DefaultNonContainerAutoSize;
                    ApplyMinAndMax(layout.TemplateLayout, dimension, ref updatedSize);
                }
                else if (layout.ContainerDimensions.Contains(dimension))
                {
                    var totalSize = 0;
                    foreach (var groupNumber in groups.Keys)
                    {
                        var group = groups[groupNumber];
                        var groupSize = GetActualSize(group, dimension);
                        totalSize += groupSize;
                    }

                    updatedSize = totalSize;
                    ApplyMinAndMax(layout.TemplateLayout, dimension, ref updatedSize);
                }
                else
                {
                    var maxSize = 0;
                    foreach (var groupNumber in groups.Keys)
                    {
                        var group = groups[groupNumber];
                        var groupSize = GetActualSize(group, dimension);

                        if (groupSize > maxSize)
                        { maxSize = groupSize; }
                    }

                    updatedSize = maxSize;
                    ApplyMinAndMax(layout.TemplateLayout, dimension, ref updatedSize);
                }
            }
            else
            { throw new InvalidOperationException("Unexpected SizeMode encountered."); }


            var alignmentMode = dimensionLayout.AlignmentMode_Value;
            var margin = dimensionLayout.Margin_Value;

            if (alignmentMode == AlignmentMode.Lesser)
            {
                updatedLocation = dimensionLayout.Margin_Value.LesserSide;
            }
            else if (alignmentMode == AlignmentMode.Greater)
            {
                if (parentGroupSize.HasValue)
                { updatedLocation = (parentGroupSize.Value - (dimensionLayout.Margin_Value.GreaterSide + updatedSize)); }
                else
                { updatedLocation = 0; }
            }
            else if (alignmentMode == AlignmentMode.Center)
            {
                int offset = 0;
                var difference = (parentGroupSize.HasValue) ? (parentGroupSize.Value - updatedSize) : 0;

                if (isParentLayoutAutoSized)
                {
                    offset = dimensionLayout.Margin_Value.LesserSide;
                    difference -= (dimensionLayout.Margin_Value.LesserSide + dimensionLayout.Margin_Value.GreaterSide);
                }

                var lesserWeight = dimensionLayout.Margin_Value.LesserSide;
                var greaterWeight = dimensionLayout.Margin_Value.GreaterSide;

                if ((lesserWeight == 0) && (greaterWeight == 0))
                {
                    lesserWeight = 1;
                    greaterWeight = 1;
                }
                else if ((lesserWeight + greaterWeight) == 0)
                {
                    var min = (lesserWeight < 0) ? lesserWeight : greaterWeight;
                    lesserWeight += Math.Abs(min);
                    greaterWeight += Math.Abs(min);
                }

                var totalWeight = (lesserWeight + greaterWeight);
                if (totalWeight == 0)
                { throw new InvalidOperationException("The Positioning values are not valid."); }

                var weightPart = (int)(((double)lesserWeight / (double)totalWeight) * difference);
                updatedLocation = offset + weightPart;
            }
            else if (alignmentMode == AlignmentMode.Stretch)
            {
                var totalMargin = dimensionLayout.Margin_Value.LesserSide + dimensionLayout.Margin_Value.GreaterSide;

                updatedLocation = dimensionLayout.Margin_Value.LesserSide;

                if (parentGroupSize.HasValue)
                { updatedSize = parentGroupSize.Value - totalMargin; }
            }
            else
            { throw new InvalidOperationException("Unrecognized AlignmentMode encountered."); }

            UpdateLocation(layout, dimension, updatedLocation);
            UpdateActualSize(layout, dimension, updatedSize);

            var layoutChanged = ((updatedLocation != initialLocation) || (updatedSize != initialSize));

            if (!intializedKeys.Contains(layout.FastId))
            {
                intializedKeys.Add(layout.FastId);
            }

            if (layout.IsContainer && layoutChanged)
            {
                if (groups.Count < 1)
                { /* do nothing */ }
                else if (layout.ContainerDimensions.Contains(dimension))
                {
                    var maxGroupNumber = groups.Keys.Max();
                    var groupsUtilizingExtraSpace = groups.Values.Where(g => g.CanUtilizeExtraSpace).ToDictionary(g => (maxGroupNumber - g.GroupNumber), g => g);
                    var stretchGroup = groupsUtilizingExtraSpace[groupsUtilizingExtraSpace.Keys.Min()];

                    var remainingSize = updatedSize;
                    foreach (var groupNumber in groups.Keys)
                    {
                        if (groupNumber == stretchGroup.GroupNumber)
                        { continue; }

                        var group = groups[groupNumber];
                        var groupSize = (GetActualSize(group, dimension) <= remainingSize) ? GetActualSize(group, dimension) : remainingSize;

                        HandleRenderingGroup(group, dimension, groupSize, renderingTree, intializedKeys);
                        remainingSize -= groupSize;
                    }

                    HandleRenderingGroup(stretchGroup, dimension, remainingSize, renderingTree, intializedKeys);
                }
                else
                {
                    foreach (var groupNumber in groups.Keys)
                    {
                        var group = groups[groupNumber];
                        HandleRenderingGroup(group, dimension, updatedSize, renderingTree, intializedKeys);
                    }
                }
            }
        }

        public static void HandleRenderingGroup(RenderingGroup group, Dimension dimension, Nullable<int> groupSizeFromParent, ITree<Guid> renderingTree, ICollection<Guid> intializedKeys)
        {
            var isParentLayoutAutoSized = true;
            var parentLayoutKey = renderingTree.GetParent(group.FastId);

            if (!parentLayoutKey.HasValue)
            { throw new InvalidOperationException(); }

            var parentLayout = renderingTree.GetCachedValue<RenderedLayout>(parentLayoutKey.Value);
            isParentLayoutAutoSized = (parentLayout.TemplateLayout.GetDimensionLayout(dimension).SizeMode_Value == SizeMode.Auto);

            var initialSize = GetActualSize(group, dimension);
            var updatedSize = 0;

            if (groupSizeFromParent.HasValue)
            {
                updatedSize = groupSizeFromParent.Value;
            }
            else
            {
                foreach (var nestedLayout in group.ContainedLayouts.Values)
                {
                    var contentsSize = GetActualSize(nestedLayout, dimension);
                    var marginSize = GetMarginContribution(nestedLayout, dimension, isParentLayoutAutoSized);
                    var totalSize = contentsSize + marginSize;

                    if (group.IgnoreLayoutWhenSizing(nestedLayout.RenderingKey))
                    { continue; }

                    if (totalSize > updatedSize)
                    { updatedSize = totalSize; }
                }
            }

            UpdateActualSize(group, dimension, updatedSize);

            if (!intializedKeys.Contains(group.FastId))
            {
                intializedKeys.Add(group.FastId);
            }

            var groupChanged = (updatedSize != initialSize);
            if (groupChanged)
            {
                foreach (var nestedLayout in group.ContainedLayouts.Values)
                {
                    HandleRenderedLayout(nestedLayout, dimension, updatedSize, renderingTree, intializedKeys);
                }
            }
        }

        public static int GetGroupSizeUsingDelayedSizing(this RenderingGroup group, Dimension dimension, ITree<Guid> renderingTree, IDictionary<RenderingKey, object> renderingLookup)
        {
            if (group.SourceLayout.Element == null)
            { throw new InvalidOperationException("Null ReportElements are not allowed to use Delayed Sizing."); }
            if (group.SourceLayout.Element.GetType() != typeof(VariableDataContainer))
            { throw new InvalidOperationException("Currently, only VariableDataContainer are allowed to use Delayed Sizing."); }

            var renderingKey = group.RenderingKey;
            var sourceElement = (VariableDataContainer)group.SourceLayout.Element;

            var siblingRenderingKey = new RenderingKey(sourceElement.VariableTitleContainerId, renderingKey.StructuralPoint, renderingKey.TimeKey, dimension, group.GroupNumber);
            if (dimension != sourceElement.StackingDimension)
            { siblingRenderingKey = new RenderingKey(sourceElement.CommonTitleContainerId, renderingKey.StructuralPoint, renderingKey.TimeKey, dimension, ReportRenderingEngineUtils.NonContainerGroupNumber); }

            var hasSiblingGroup = renderingLookup.ContainsKey(siblingRenderingKey);
            var siblingGroup = hasSiblingGroup ? (RenderingGroup)renderingLookup[siblingRenderingKey] : group;

            var sizeToUse = siblingGroup.GetActualSize(dimension);
            return sizeToUse;
        }

        #region Template Helper Methods

        public static void ApplyMinAndMax(this IElementLayout layout, Dimension dimension, ref int size)
        {
            if (size < layout.GetDimensionLayout(dimension).MinRangeSizeInCells_Value)
            { size = layout.GetDimensionLayout(dimension).MinRangeSizeInCells_Value; }
            if (size > layout.GetDimensionLayout(dimension).MaxRangeSizeInCells_Value)
            { size = layout.GetDimensionLayout(dimension).MaxRangeSizeInCells_Value; }
        }

        #endregion

        #region Rendering Helper Methods

        public static bool IsDelayedSized(this RenderedLayout layout)
        {
            if (layout.Element == null)
            { return false; }
            return layout.Element.RequiresDelayedSizing;
        }

        public static bool IsDelayedSized(this RenderingGroup group)
        {
            if (group.SourceLayout.Element == null)
            { return false; }
            return group.SourceLayout.Element.RequiresDelayedSizing;
        }

        public static int GetMarginContribution(this RenderedLayout layout, Dimension dimension, bool isParentAutoSized)
        {
            var dimLayout = layout.TemplateLayout.GetDimensionLayout(dimension);
            var alignmentMode = dimLayout.AlignmentMode_Value;
            var margin = dimLayout.Margin_Value;

            if (alignmentMode == AlignmentMode.Lesser)
            {
                return margin.LesserSide;
            }
            else if (alignmentMode == AlignmentMode.Greater)
            {
                return margin.GreaterSide;
            }
            else if (alignmentMode == AlignmentMode.Center)
            {
                if (!isParentAutoSized)
                { return 0; }
                else
                { return (margin.LesserSide + margin.GreaterSide); }
            }
            else if (alignmentMode == AlignmentMode.Stretch)
            {
                return (margin.LesserSide + margin.GreaterSide);
            }
            else
            { throw new InvalidOperationException("Unrecognized AlignmentMode encountered."); }
        }

        public static int GetActualSize(this RenderedLayout layout, Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return layout.ActualSize.Width; }
            else if (dimension == Dimension.Y)
            { return layout.ActualSize.Height; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public static int GetLocation(this RenderedLayout layout, Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return layout.Location.X; }
            else if (dimension == Dimension.Y)
            { return layout.Location.Y; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public static void UpdateActualSize(this RenderedLayout layout, Dimension dimension, int size)
        {
            if (dimension == Dimension.X)
            { layout.ActualSize = new Size(size, layout.ActualSize.Height); }
            else if (dimension == Dimension.Y)
            { layout.ActualSize = new Size(layout.ActualSize.Width, size); }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public static void UpdateLocation(this RenderedLayout layout, Dimension dimension, int coordinate)
        {
            if (dimension == Dimension.X)
            { layout.Location = new Point(coordinate, layout.Location.Y); }
            else if (dimension == Dimension.Y)
            { layout.Location = new Point(layout.Location.X, coordinate); }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public static int GetActualSize(this RenderingGroup group, Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return group.ActualSize.Width; }
            else if (dimension == Dimension.Y)
            { return group.ActualSize.Height; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public static void UpdateActualSize(this RenderingGroup group, Dimension dimension, int size)
        {
            if (dimension == Dimension.X)
            { group.ActualSize = new Size(size, group.ActualSize.Height); }
            else if (dimension == Dimension.Y)
            { group.ActualSize = new Size(group.ActualSize.Width, size); }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        #endregion
    }
}