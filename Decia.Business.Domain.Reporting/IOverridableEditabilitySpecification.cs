using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    internal interface IOverridableEditabilitySpecification : IEditabilitySpecification
    {
        EditMode CurrentMode { get; set; }
    }
}