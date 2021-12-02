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
    public partial class StructuralTitleRange
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            ICurrentState reportState = renderingState.ReportState;
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Validation);

            if (!renderingState.GroupingState.ElementRepeatGroups.ContainsKey(this.Key))
            {
                var setterElement = this.IsCommonTitleRelated ? "CommonTitleContainer" : "corresponding VariableTitleBox";
                var message = "The Structural Title Range's Repeat Group should have already been set by the " + setterElement + ".";
                throw new InvalidOperationException(message);
            }

            if (this.OutputValueType == OutputValueType.AnonymousFormula)
            {
                var localRepeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];
                var textFormula = dataProvider.GetAnonymousFormula(this.ValueFormulaId);
                var dataType = DeciaDataType.Text;

                dataProvider.AddAnonymousVariable_ReportingState(this.Key, textFormula, reportState.StructuralTypeRef, localRepeatGroup.DesignContext.ResultingSpace, localRepeatGroup.RelevantTimeDimensions, dataType);
            }

            result.SetValidatedState();
            return result;
        }

        public override RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderRangeForDesign(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }

        public override RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            var result = ReportElementUtils.RenderRangeForProduction(this, dataProvider, renderingState, elementTree, layoutResults, parentRenderingKey);
            return result;
        }
    }
}