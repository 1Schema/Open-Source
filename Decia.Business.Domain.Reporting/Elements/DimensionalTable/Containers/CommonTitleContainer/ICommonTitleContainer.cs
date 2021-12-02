using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public interface ICommonTitleContainer : IReadOnlyContainer, IDimensionalContainer, ITransposableElement
    {
        ReportElementId VariableDataContainerId { get; }
        int VariableDataContainerNumber { get; }

        ICollection<ReportElementId> CommonTitleBoxIds { get; }
        ICollection<int> CommonTitleBoxNumbers { get; }

        void AddCommonTitleBox(ICommonTitleBox element, IEnumerable<IDimensionalRange> containedDimensionalRanges);
        void RemoveCommonTitleBox(ICommonTitleBox element);
    }
}