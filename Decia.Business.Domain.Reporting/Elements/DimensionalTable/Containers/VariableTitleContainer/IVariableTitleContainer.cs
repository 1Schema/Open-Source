using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public interface IVariableTitleContainer : IReadOnlyContainer, IDimensionalContainer, ITransposableElement
    {
        ReportElementId VariableDataContainerId { get; }
        int VariableDataContainerNumber { get; }

        ICollection<ReportElementId> VariableTitleBoxIds { get; }
        ICollection<int> VariableTitleBoxNumbers { get; }

        void AddVariableTitleBox(IVariableTitleBox element, IVariableTitleRange containedVariableRange, IEnumerable<IDimensionalRange> containedDimensionalRanges);
        void RemoveVariableTitleBox(IVariableTitleBox element);
    }
}