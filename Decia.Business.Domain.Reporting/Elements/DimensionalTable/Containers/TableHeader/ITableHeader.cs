using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface ITableHeader : IContainer
    {
        void AddChild(ICell element);
        void RemoveChild(ICell element);
    }
}