using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    public interface ICommonTitleBox : IDimensionalBox, ITransposableOrderableElement
    {
        string StyleGroup { get; set; }
    }
}