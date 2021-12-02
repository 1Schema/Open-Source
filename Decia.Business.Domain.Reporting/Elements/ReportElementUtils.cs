using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public static class ReportElementUtils
    {
        public static RenderingResult ValidateContainer<T>(T container, IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
            where T : IReadOnlyContainer
        {
            return ValidateContainer(container, dataProvider, renderingState, elementTree, new ReportElementId[] { }, new ReportElementId[] { });
        }

        public static RenderingResult ValidateContainer<T>(T container, IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IEnumerable<ReportElementId> preLoopElementIds, IEnumerable<ReportElementId> postLoopElementIds)
            where T : IReadOnlyContainer
        {
            var preLoopElementNumbers = preLoopElementIds.Select(x => x.ReportElementNumber).ToList();
            var postLoopElementNumbers = postLoopElementIds.Select(x => x.ReportElementNumber).ToList();

            ICurrentState reportState = renderingState.ReportState;
            DimensionalRepeatGroup parentRepeatGroup = renderingState.GroupingState.ElementRepeatGroups[container.ParentElementOrReportId];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, container.Key, ProcessingAcivityType.Validation);

            parentRepeatGroup.AddElementToGroup(container);

            var preLoopElements = renderingState.ReportElements.Values.Where(x => preLoopElementNumbers.Contains(x.Key.ReportElementNumber)).ToDictionary(x => x.Key.ReportElementNumber, x => x);
            var childElements = renderingState.ReportElements.Values.Where(x => container.ChildNumbers.Contains(x.Key.ReportElementNumber)).ToDictionary(x => x.Key.ReportElementNumber, x => x);
            var postLoopElements = renderingState.ReportElements.Values.Where(x => postLoopElementNumbers.Contains(x.Key.ReportElementNumber)).ToDictionary(x => x.Key.ReportElementNumber, x => x);

            var loopElements = new List<IReportElement>();
            foreach (var preLoopElementNumber in preLoopElementNumbers)
            { loopElements.Add(preLoopElements[preLoopElementNumber]); }
            foreach (var childElementNumber in container.ChildNumbers)
            { loopElements.Add(childElements[childElementNumber]); }
            foreach (var postLoopElementNumber in postLoopElementNumbers)
            { loopElements.Add(postLoopElements[postLoopElementNumber]); }

            foreach (var element in loopElements)
            {
                if (renderingState.GroupingState.ProcessedElementIds.Contains(element.Key))
                { continue; }

                var elementResult = element.Validate(dataProvider, renderingState, elementTree);

                if (!elementResult.IsValid)
                {
                    result.SetErrorState(elementResult);
                    return result;
                }
                else
                { result.SetValidatedState(elementResult); }
            }

            if (loopElements.Count <= 0)
            { result.SetValidatedState(); }
            return result;
        }

        public static void AssertVariableDimensionalityMatchesElementDimensionality(this ModelObjectReference variableTemplateRef, IReportingDataProvider dataProvider, ICurrentState reportState, DimensionalRepeatGroup repeatGroup)
        {
            var variableStructuralType = dataProvider.GetStructuralType(variableTemplateRef);
            var variableStructuralSpace = dataProvider.StructuralMap.GetStructuralSpace(variableStructuralType, reportState.UseExtendedStructure);
            var variableTimeDimensionSet = dataProvider.GetValidatedTimeDimensions(variableTemplateRef);

            var groupStructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            var isGroupRelatedAndMoreGeneral = StructuralSpaceUtils.IsRelatedAndMoreGeneral(variableStructuralSpace, dataProvider.StructuralMap, groupStructuralSpace, reportState.UseExtendedStructure, true);

            if (!isGroupRelatedAndMoreGeneral)
            { throw new InvalidOperationException("The Variable has different Structural Dimensionality than the Table expects."); }

            foreach (var timeDimensionType in repeatGroup.RelevantTimeDimensions.Keys)
            {
                var timeDimension = variableTimeDimensionSet.GetTimeDimension(timeDimensionType);

                if (timeDimension.NullableTimePeriodType != repeatGroup.RelevantTimeDimensions[timeDimensionType])
                { throw new InvalidOperationException("The Variable has different Time Dimensionality than the Table expects."); }
            }
        }

        public static RenderingResult RenderRangeForDesign<T>(T range, IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
            where T : IValueRange
        {
            ICurrentState reportState = renderingState.ReportState;
            IReport report = renderingState.Report;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[range.Key];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, range.Key, ProcessingAcivityType.DesignRendering);

            var childElementIds = elementTree.GetChildren(range.Key);
            if (range.IsContainer || childElementIds.Count > 0)
            {
                result.SetErrorState(RenderingResultType.InvalidChildElementsFound, "Value Ranges cannot contain other Elements.");
                return result;
            }

            var currentLayout = new RenderedLayout();
            currentLayout.Report = report;
            currentLayout.Element = range;
            currentLayout.StructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            currentLayout.Value = null;
            currentLayout.ActualSize = range.ElementLayout.GetDesiredSize(currentLayout.ActualSize);
            layoutResults.Add(currentLayout.RenderingKey, currentLayout);

            var rangeSuccess = range.TryGetDesignValue(dataProvider, renderingState, currentLayout);

            var parentLayout = layoutResults[parentRenderingKey];
            parentLayout.NestedLayouts.Add(currentLayout);

            result.SetRenderedState();
            return result;
        }

        public static RenderingResult RenderContainerForDesign<T>(T container, IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
            where T : IReadOnlyContainer
        {
            ICurrentState reportState = renderingState.ReportState;
            IReport report = renderingState.Report;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[container.Key];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, container.Key, ProcessingAcivityType.DesignRendering);

            var currentLayout = new RenderedLayout();
            currentLayout.Report = report;
            currentLayout.Element = container;
            currentLayout.StructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            currentLayout.ActualSize = container.ElementLayout.GetDesiredSize(currentLayout.ActualSize);
            layoutResults.Add(currentLayout.RenderingKey, currentLayout);

            var parentLayout = layoutResults[parentRenderingKey];
            parentLayout.NestedLayouts.Add(currentLayout);

            var childElements = container.ChildIds.Select(x => elementTree.GetCachedValue<IReportElement>(x)).OrderBy(x => x.ZOrder).ToList();

            if (childElements.Count <= 0)
            {
                result.SetRenderedState();
            }
            else
            {
                foreach (var childElement in childElements)
                {
                    AssertChildMatchesParentSettings(container, childElement);

                    var childElementResult = childElement.RenderForDesign(dataProvider, renderingState, elementTree, layoutResults, currentLayout.RenderingKey);

                    if (!childElementResult.IsValid)
                    {
                        result.SetErrorState(childElementResult);
                        return result;
                    }
                    else
                    { result.SetRenderedState(childElementResult); }
                }
            }

            return result;
        }

        public static RenderingResult RenderRangeForProduction<T>(T range, IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
           where T : IValueRange
        {
            ICurrentState reportState = renderingState.ReportState;
            IReport report = renderingState.Report;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[range.Key];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.ModelInstanceRef, reportState.StructuralTypeRef, reportState.StructuralInstanceRef, range.Key, ProcessingAcivityType.ProductionRendering);

            var childElementIds = elementTree.GetChildren(range.Key);
            if (range.IsContainer || childElementIds.Count > 0)
            {
                result.SetErrorState(RenderingResultType.InvalidChildElementsFound, "Value Ranges cannot contain other Elements.");
                return result;
            }

            var currentLayout = new RenderedLayout();
            currentLayout.Report = report;
            currentLayout.Element = range;
            currentLayout.StructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            currentLayout.StructuralPoint = renderingState.CurrentStructuralPosition.Value;
            currentLayout.TimeKey = renderingState.CurrentTimeBindings;
            currentLayout.OverriddenContentGroups = new SortedDictionary<Dimension, int>(renderingState.OverriddenContentGroups);
            currentLayout.Value = null;
            currentLayout.ActualSize = range.ElementLayout.GetDesiredSize(currentLayout.ActualSize);
            layoutResults.Add(currentLayout.RenderingKey, currentLayout);

            var rangeSuccess = range.TryGetProductionValue(dataProvider, renderingState, currentLayout);

            var parentLayout = layoutResults[parentRenderingKey];
            parentLayout.NestedLayouts.Add(currentLayout);

            result.SetRenderedState();
            return result;
        }

        public static RenderingResult RenderContainerForProduction<T>(T container, IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
            where T : IReadOnlyContainer
        {
            ICurrentState reportState = renderingState.ReportState;
            IReport report = renderingState.Report;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[container.Key];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.ModelInstanceRef, reportState.StructuralTypeRef, reportState.StructuralInstanceRef, container.Key, ProcessingAcivityType.ProductionRendering);

            var currentLayout = new RenderedLayout();
            currentLayout.Report = report;
            currentLayout.Element = container;
            currentLayout.StructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            currentLayout.StructuralPoint = renderingState.CurrentStructuralPosition.Value;
            currentLayout.TimeKey = renderingState.CurrentTimeBindings;
            currentLayout.OverriddenContentGroups = new SortedDictionary<Dimension, int>(renderingState.OverriddenContentGroups);
            currentLayout.ActualSize = container.ElementLayout.GetDesiredSize(currentLayout.ActualSize);
            layoutResults.Add(currentLayout.RenderingKey, currentLayout);

            var parentLayout = layoutResults[parentRenderingKey];
            parentLayout.NestedLayouts.Add(currentLayout);

            var childElements = container.ChildIds.Select(x => elementTree.GetCachedValue<IReportElement>(x)).OrderBy(x => x.ZOrder).ToList();

            if (childElements.Count <= 0)
            {
                result.SetRenderedState();
            }
            else
            {
                foreach (var childElement in childElements)
                {
                    AssertChildMatchesParentSettings(container, childElement);

                    var childElementResult = childElement.RenderForProduction(dataProvider, renderingState, elementTree, layoutResults, currentLayout.RenderingKey);

                    if (!childElementResult.IsValid)
                    {
                        result.SetErrorState(childElementResult);
                        return result;
                    }
                    else
                    { result.SetRenderedState(childElementResult); }
                }
            }

            return result;
        }

        public static void AssertChildMatchesParentSettings(this IReportElement parentElement, IReportElement childElement)
        {
            if (!childElement.ParentReportId.Equals_Revisionless(parentElement.ParentReportId))
            { throw new InvalidOperationException("The Report Elements belong to different Reports."); }
            if (childElement.ParentElementNumber != parentElement.Key.ReportElementNumber)
            { throw new InvalidOperationException("The nested Report Element belong to different parent Report Element."); }
        }

        public static void AssertIsReport(this IReportElement element)
        {
            if (element.ModelObjectType != ModelObjectType.ReportTemplate)
            { throw new InvalidOperationException("The ReportElement is not of type \"ReportTemplate\"."); }
        }

        public static void AssertIsElement(this IReportElement element)
        {
            if (element.ModelObjectType != ModelObjectType.ReportElementTemplate)
            { throw new InvalidOperationException("The ReportElement is not of type \"ReportElementTemplate\"."); }
        }

        public static void AssertIsElementOfSpecficType(this IReportElement element, ReportElementType_New desiredReportElementType)
        {
            element.AssertIsElement();

            if (element.ReportElementType != desiredReportElementType)
            { throw new InvalidOperationException("The ReportElement is not of type \"" + desiredReportElementType.ToString() + "\"."); }
        }

        public static void AssertIsNotLocked(this IReportElement element)
        {
            if (element.IsLocked)
            { throw new InvalidOperationException("The ReportElement is locked for editing."); }
        }

        public static void AssertCanContainType(this IReportElement element, ReportElementType_New typeToContain)
        {
            if (!element.AcceptableContentTypes.Contains(typeToContain))
            { throw new InvalidOperationException("The specified Report Element Type cannot be contained within the specified Element."); }
        }
    }
}