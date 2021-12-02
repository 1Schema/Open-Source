using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public partial class DimensionalTable
    {
        public override RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            if (!m_TableHeaderNumber.HasValue)
            { throw new InvalidOperationException("The related TableHeader has not been set."); }
            if (!m_ColumnHeaderNumber.HasValue)
            { throw new InvalidOperationException("The related ColumnHeader has not been set."); }
            if (!m_RowHeaderNumber.HasValue)
            { throw new InvalidOperationException("The related RowHeader has not been set."); }
            if (!m_DataAreaNumber.HasValue)
            { throw new InvalidOperationException("The related DataArea has not been set."); }
            if (!m_CommonTitleContainerNumber.HasValue)
            { throw new InvalidOperationException("The related CommonTitleContainer has not been set."); }
            if (!m_VariableTitleContainerNumber.HasValue)
            { throw new InvalidOperationException("The related VariableTitleContainer has not been set."); }
            if (!m_VariableDataContainerNumber.HasValue)
            { throw new InvalidOperationException("The related VariableDataContainer has not been set."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_TableHeaderNumber.Value)))
            { throw new InvalidOperationException("The related TableHeader does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_ColumnHeaderNumber.Value)))
            { throw new InvalidOperationException("The related ColumnHeader does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_RowHeaderNumber.Value)))
            { throw new InvalidOperationException("The related RowHeader does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_DataAreaNumber.Value)))
            { throw new InvalidOperationException("The related DataArea does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_CommonTitleContainerNumber.Value)))
            { throw new InvalidOperationException("The related CommonTitleContainer does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_VariableTitleContainerNumber.Value)))
            { throw new InvalidOperationException("The related VariableTitleContainer does not exist in the current Report."); }
            if (!renderingState.ReportElements.ContainsKey(new ReportElementId(this.ParentReportId, m_VariableDataContainerNumber.Value)))
            { throw new InvalidOperationException("The related VariableDataContainer does not exist in the current Report."); }

            var tableHeader = (TableHeader)renderingState.ReportElements[this.TableHeaderId];
            var rowHeader = (RowHeader)renderingState.ReportElements[this.RowHeaderId];
            var columnHeader = (ColumnHeader)renderingState.ReportElements[this.ColumnHeaderId];
            var dataArea = (DataArea)renderingState.ReportElements[this.DataAreaId];
            var commonTitleContainer = (CommonTitleContainer)renderingState.ReportElements[this.CommonTitleContainerId];
            var variableTitleContainer = (VariableTitleContainer)renderingState.ReportElements[this.VariableTitleContainerId];
            var variableDataContainer = (VariableDataContainer)renderingState.ReportElements[this.VariableDataContainerId];

            var commonTitleElementIds = elementTree.GetDecendants(commonTitleContainer.Key);
            var variableTitleElementIds = elementTree.GetDecendants(variableTitleContainer.Key);
            var variableDataElementIds = elementTree.GetDecendants(variableDataContainer.Key);

            var commonTitleElements = commonTitleElementIds.Select(x => (ITransposableElement)renderingState.ReportElements[x]).ToList();
            var variableTitleElements = variableTitleElementIds.Select(x => (ITransposableElement)renderingState.ReportElements[x]).ToList();
            var variableDataElements = variableDataElementIds.Select(x => (ITransposableElement)renderingState.ReportElements[x]).ToList();

            this.AssertAllRequiredElementsArePresentAndProperlyConfigured(commonTitleContainer, commonTitleElements, variableTitleContainer, variableTitleElements, variableDataContainer, variableDataElements);
            this.AssertDimensionalTableElementsHaveMatchingIsTransposedSettings(commonTitleContainer, commonTitleElements, variableTitleContainer, variableTitleElements, variableDataContainer, variableDataElements);

            var commonHeaderKey = rowHeader.ChildNumbers.Contains(this.CommonTitleContainerNumber) ? rowHeader.Key : columnHeader.Key;
            var variableHeaderKey = (commonHeaderKey == rowHeader.Key) ? columnHeader.Key : rowHeader.Key;

            commonTitleContainer.AssertContainersDoNotHaveOverlappingDimensions(variableTitleContainer);

            var result = ReportElementUtils.ValidateContainer(this, dataProvider, renderingState, elementTree, new ReportElementId[] { commonHeaderKey, variableHeaderKey, this.DataAreaId }, new ReportElementId[] { });
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