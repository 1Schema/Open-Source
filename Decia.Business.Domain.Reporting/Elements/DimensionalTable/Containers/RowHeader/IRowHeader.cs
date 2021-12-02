using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IRowHeader : IReadOnlyContainer
    {
        bool HoldsVariableContainer { get; }
        bool HoldsCommonContainer { get; }
        ReportElementId NestedContainerId { get; }
        int NestedContainerNumber { get; }
    }
}