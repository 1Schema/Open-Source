using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Computation
{
    public interface ICurrentState_Anonymous : ICurrentState_Normal
    {
        StructuralSpace StructuralSpace { get; }
        Nullable<StructuralSpace> NullableStructuralSpace { get; }
        StructuralPoint StructuralPoint { get; }
        Nullable<StructuralPoint> NullableStructuralPoint { get; }

        ICollection<StructuralDimension> StructuralAggregationDimensions { get; }
        ICollection<TimeDimensionType> TimeAggregationDimensions { get; }

        void AddStructuralAggregationDimension(StructuralDimension structuralDimension);
        void AddTimeAggregationDimension(TimeDimensionType timeDimensionType);
    }
}