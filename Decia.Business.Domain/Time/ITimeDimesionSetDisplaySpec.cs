using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public interface ITimeDimesionSetDisplaySpec
    {
        Dimension_DisplayMode GetDisplayMode(int timeDimensionNumber);
        Dimension_DisplayMode GetDisplayMode(TimeDimensionType timeDimensionType);

        Dimension_RepeatMode GetRepeatMode(int dimensionNumber);
        Dimension_RepeatMode GetRepeatMode(TimeDimensionType timeDimensionType);
    }
}