using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Time
{
    public interface ITimeDimensionSetProvider
    {
        ITimeDimensionSet GetTimeDimensionSet(ITimeDimension baseTimeDimension);
    }
}