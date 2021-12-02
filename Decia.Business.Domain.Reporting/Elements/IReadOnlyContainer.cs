using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IReadOnlyContainer : IReportElement
    {
        ICollection<ReportElementId> ChildIds { get; }
        ICollection<int> ChildNumbers { get; }
    }
}