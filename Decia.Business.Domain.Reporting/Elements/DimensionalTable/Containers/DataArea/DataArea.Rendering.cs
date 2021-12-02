using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public partial class DataArea
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            if (!m_VariableDataContainerNumber.HasValue)
            { throw new InvalidOperationException("The contained VariableDataContainer has not been set."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_VariableDataContainerNumber.Value)))
            { throw new InvalidOperationException("The contained VariableDataContainer does not exist in the current Report."); }

            var result = ReportElementUtils.ValidateContainer(this, dataProvider, renderingState, elementTree);
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