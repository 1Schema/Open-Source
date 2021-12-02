using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    public interface IVariableDataContainer : IReadOnlyContainer, ITransposableElement
    {
        ReportElementId VariableTitleContainerId { get; }
        int VariableTitleContainerNumber { get; }
        ReportElementId CommonTitleContainerId { get; }
        int CommonTitleContainerNumber { get; }

        ICollection<ReportElementId> VariableDataBoxIds { get; }
        ICollection<int> VariableDataBoxNumbers { get; }

        void AddVariableDataBox(IVariableDataBox element, IVariableDataRange containedVariableRange, ICommonTitleBox relatedCommonTitleBox, IVariableTitleBox relatedVariableTitleBox);
        void RemoveVariableDataBox(IVariableDataBox element);
    }
}