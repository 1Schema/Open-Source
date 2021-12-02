using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IDimensionalBox : IReadOnlyContainer
    {
        ICollection<ReportElementId> ContainedStructuralTitleRangeIds { get; }
        ICollection<int> ContainedStructuralTitleRangeNumbers { get; }
        ICollection<ReportElementId> ContainedTimeTitleRangeIds { get; }
        ICollection<int> ContainedTimeTitleRangeNumbers { get; }

        void AddContainedStructuralTitleRange(IStructuralTitleRange element);
        void RemoveContainedStructuralTitleRange(IStructuralTitleRange element);

        void AddContainedTimeTitleRange(ITimeTitleRange element);
        void RemoveContainedTimeTitleRange(ITimeTitleRange element);
    }
}