using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public partial class Report
    {
        public RenderingResult Initialize(IReportingDataProvider dataProvider, IRenderingState renderingState)
        {
            ICurrentState reportState = renderingState.ReportState;
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Initialization);

            if (renderingState.Report != this)
            {
                result.SetErrorState(RenderingResultType.ReportInitializationFailed, "The current Report is not the same as the Report being rendered.");
                return result;
            }
            if (reportState.ModelTemplateRef != this.ModelTemplateRef)
            {
                result.SetErrorState(RenderingResultType.ReportInitializationFailed, "The current Model Template is not the same as the Model Template of the Report being rendered.");
                return result;
            }
            if (dataProvider.StructuralMap.ModelTemplateRef != this.ModelTemplateRef)
            {
                result.SetErrorState(RenderingResultType.ReportInitializationFailed, "The processed Model Template Report is not the same as the Model Template of the Report being rendered.");
                return result;
            }

            result.SetInitializedState();
            return result;
        }

        public RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            ICurrentState reportState = renderingState.ReportState;
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Validation);

            if (!dataProvider.StructuralMap.ContainsTypeRef(this.StructuralTypeRef))
            {
                result.SetErrorState(RenderingResultType.StructuralReferenceIsInvalid, "The current Report's Structural Type reference is not valid.");
                return result;
            }

            var structuralContexts = DimensioningUtils.BuildStructuralContexts(dataProvider, reportState);

            if (structuralContexts.Count != 1)
            {
                result.SetErrorState(RenderingResultType.StructuralReferenceIsInvalid, "The current Report does not have a valid Structural Context.");
                return result;
            }

            var structuralContext = structuralContexts.First().Value;
            var relevantTimeDimensions = this.TimeDimensionUsages;
            var repeatGroupName = this.Name;
            var repeatGroup = new DimensionalRepeatGroup(repeatGroupName, reportState, structuralContext, relevantTimeDimensions);

            repeatGroup.AddElementToGroup(this);
            renderingState.GroupingState.AddRepeatGroup(repeatGroup);

            var directElements = renderingState.ReportElements.Values.Where(x => x.ParentElementOrReportId.Equals_Revisionless(this.KeyAsElementId)).ToList();

            foreach (var element in directElements)
            {
                var elementResult = element.Validate(dataProvider, renderingState, elementTree);

                if (!elementResult.IsValid)
                {
                    result.SetErrorState(elementResult);
                    return result;
                }
                else
                { result.SetValidatedState(elementResult); }
            }

            if (directElements.Count < 1)
            { result.SetValidatedState(); }
            return result;
        }

        public RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults)
        {
            ICurrentState reportState = renderingState.ReportState;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.KeyAsElementId];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.DesignRendering);

            var renderedLayout = new RenderedLayout();
            renderedLayout.Report = this;
            renderedLayout.StructuralSpace = repeatGroup.DesignContext.ResultingSpace;
            renderedLayout.ActualSize = this.ReportAreaLayout.GetDesiredSize(renderedLayout.ActualSize);
            layoutResults.Add(renderedLayout.RenderingKey, renderedLayout);

            result.SetRenderedState();
            return result;
        }

        public RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults)
        {
            ICurrentState reportState = renderingState.ReportState;
            DimensionalRepeatGroup repeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.KeyAsElementId];
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.DesignRendering);

            var productionContext = repeatGroup.ProductionContexts.Values.First();

            if (productionContext.ResultingPoints.Count > 1)
            { throw new InvalidOperationException("The Report Context should yield a single Structural Point."); }

            var productionSpace = productionContext.ResultingSpace;
            var productionPoint = productionContext.ResultingPoints.First();
            var productionTimeKey = MultiTimePeriodKey.DimensionlessTimeKey;

            if (repeatGroup.RelevantTimeDimensions.Values.Where(x => (x != null)).Count() > 0)
            {
                throw new InvalidOperationException("Currently, Time-Specific Reports are not supported.");
            }

            renderingState.CurrentTimeBindings = productionTimeKey;

            if (renderingState.OverriddenContentGroups.Count > 0)
            { throw new InvalidOperationException("Report objects should not have Overridden Content Groups."); }

            var renderedLayout = new RenderedLayout();
            renderedLayout.Report = this;
            renderedLayout.StructuralSpace = productionSpace;
            renderedLayout.StructuralPoint = productionPoint;
            renderedLayout.TimeKey = productionTimeKey;
            renderedLayout.OverriddenContentGroups = new SortedDictionary<Dimension, int>(renderingState.OverriddenContentGroups);
            renderedLayout.ActualSize = this.ReportAreaLayout.GetDesiredSize(renderedLayout.ActualSize);
            layoutResults.Add(renderedLayout.RenderingKey, renderedLayout);

            result.SetRenderedState();
            return result;
        }
    }
}