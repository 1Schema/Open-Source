using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IDataArea : IReadOnlyContainer
    {
        ReportElementId VariableDataContainerId { get; }
        int VariableDataContainerNumber { get; }
    }
}