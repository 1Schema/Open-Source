using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public partial class CommonTitleBox
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            ICurrentState reportState = renderingState.ReportState;
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Validation);

            if (!renderingState.GroupingState.ElementRepeatGroups.ContainsKey(this.Key))
            {
                var setterElement = "CommonTitleContainer";
                var message = "The Common Title Range's Repeat Group should have already been set by the " + setterElement + ".";
                throw new InvalidOperationException(message);
            }

            var repeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];
            var childElements = renderingState.ReportElements.Values.Where(x => this.ChildNumbers.Contains(x.Key.ReportElementNumber)).ToList();

            foreach (var element in childElements)
            {
                if (renderingState.GroupingState.ProcessedElementIds.Contains(element.Key))
                { throw new InvalidOperationException("Encountered a Report Element that was handled out of order."); }

                repeatGroup.AddElementToGroup(element);
                var elementResult = element.Validate(dataProvider, renderingState, elementTree);

                if (!elementResult.IsValid)
                {
                    result.SetErrorState(elementResult);
                    return result;
                }
                else
                { result.SetValidatedState(elementResult); }
            }

            if (childElements.Count <= 0)
            { result.SetValidatedState(); }
            return result;
        }

        public override RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderContainerForDesign(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }

        public override RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderContainerForProduction(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }
    }
}