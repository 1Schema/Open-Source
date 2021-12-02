using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting
{
    public interface IHidableElement
    {
        bool IsHidden { get; set; }
    }
}