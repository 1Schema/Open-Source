using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Time
{
    public interface ITimeDimensionSet : ITimeComparable<ITimeDimensionSet>
    {
        ITimeDimension PrimaryTimeDimension { get; }
        ITimeDimension SecondaryTimeDimension { get; }

        int MinimumDimensionNumber { get; }
        int MaximumDimensionNumber { get; }
        int UsedDimensionCount { get; }

        ITimeDimension GetTimeDimension(int dimensionNumber);
        ITimeDimension GetTimeDimension(int dimensionNumber, bool throwExceptionOnError);
        ITimeDimension GetTimeDimension(TimeDimensionType dimensionType);
        ITimeDimension GetTimeDimension(TimeDimensionType dimensionType, bool throwExceptionOnError);
    }
}