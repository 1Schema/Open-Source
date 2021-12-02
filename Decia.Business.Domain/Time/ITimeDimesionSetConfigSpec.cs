using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public interface ITimeDimesionSetConfigSpec
    {
        bool HasTimeDimension(int timeDimensionNumber);
        bool HasTimeDimension(TimeDimensionType timeDimensionType);

        TimeValueType GetTimeValueType(int timeDimensionNumber);
        TimeValueType GetTimeValueType(TimeDimensionType timeDimensionType);

        TimePeriodType? GetTimePeriodType(int timeDimensionNumber);
        TimePeriodType? GetTimePeriodType(TimeDimensionType timeDimensionType);
    }
}