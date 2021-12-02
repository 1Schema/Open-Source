using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IContainer : IReadOnlyContainer
    {
        void AddChild(IReportElement element);
        void RemoveChild(IReportElement element);
    }
}