using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IDimensionalTable : IReadOnlyContainer, ITransposableElement
    {
        ReportElementId TableHeaderId { get; }
        int TableHeaderNumber { get; }
        ReportElementId ColumnHeaderId { get; }
        int ColumnHeaderNumber { get; }
        ReportElementId RowHeaderId { get; }
        int RowHeaderNumber { get; }
        ReportElementId DataAreaId { get; }
        int DataAreaNumber { get; }
        ReportElementId CommonTitleContainerId { get; }
        int CommonTitleContainerNumber { get; }
        ReportElementId VariableTitleContainerId { get; }
        int VariableTitleContainerNumber { get; }
        ReportElementId VariableDataContainerId { get; }
        int VariableDataContainerNumber { get; }

        void SetContainedElements(ITableHeader tableHeader, IColumnHeader columnHeader, IRowHeader rowHeader, IDataArea dataArea, ICommonTitleContainer commonTitleContainer, IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer);

        void TransposeContainedElements(IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> nestedElements);
        void TransposeContainedElements(IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements);
        void SetTransposedForContainedElements(bool transpose, IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> nestedElements);
        void SetTransposedForContainedElements(bool transpose, IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements);
    }
}