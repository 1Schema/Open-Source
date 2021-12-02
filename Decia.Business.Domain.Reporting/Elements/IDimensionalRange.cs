using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public interface IDimensionalRange : IValueRange, ITransposableElement
    {
        ModelObjectReference DimensionalTypeRef { get; }
    }
}