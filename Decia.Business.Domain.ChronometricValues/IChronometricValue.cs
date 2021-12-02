using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.ChronometricValues
{
    public interface IChronometricValue<T> : IProjectMember_Deleteable<T>, IModelObjectWithRef
        where T : IChronometricValue<T>
    {
        ChronometricValueId Key { get; }
        DeciaDataType DataType { get; set; }
        TimeDimensionType ValidTimeDimensions { get; }
        ITimeDimensionSet TimeDimensionSet { get; }
        ITimeDimension PrimaryTimeDimension { get; }
        ITimeDimension SecondaryTimeDimension { get; }
        bool HasTimeSpecificValue { get; }

        DynamicValue DefaultValue { get; set; }
        IList<MultiTimePeriodKey> TimeKeys { get; }
        IList<MultiTimePeriodKey> StoredTimeKeys { get; }

        void ReDimension(ITimeDimensionSet timeDimensionSet);
        void ReDimension(ITimeDimension primaryTimeDimension, ITimeDimension secondaryTimeDimension);

        DynamicValue GetValue(MultiTimePeriodKey timeKey);
        DynamicValue GetValue(MultiTimePeriodKey timeKey, out MultiTimePeriodKey actualTimeKey);
        DynamicValue GetValue(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod);
        void SetValue(MultiTimePeriodKey timeKey, object value);
        void SetValue(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod, object value);
        void RemoveValue(MultiTimePeriodKey timeKey);
        void RemoveValue(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod);
        void ClearValues();
        void ClearValues(TimeDimensionType timeDimensionType, TimePeriod timePeriod);
        void MergeValues(T otherObject, MultiTimePeriodKey otherTimeKey, bool aggregateValues);
        void MergeValues(T otherObject, IEnumerable<MultiTimePeriodKey> otherTimeKeys, bool aggregateValues);
        void AssignValuesTo(T otherObject);

        bool IsSameValue(T otherObject);
        bool IsNotSameValue(T otherObject);

        IList<MultiTimePeriodKey> GetTimeKeysForIteration(T other);
        ITimeDimensionSet GetDimensionsForResult(T other);
        T CreateDimensionedResult(T other, DeciaDataType resultingDataType);

        T Not();
        T Equivalent(T otherObject);
        T NotEquivalent(T otherObject);
        T LessThan(T otherObject);
        T LessThanOrEquivalent(T otherObject);
        T GreaterThan(T otherObject);
        T GreaterThanOrEquivalent(T otherObject);
        T And(T otherObject);
        T Or(T otherObject);
        T Xor(T otherObject);

        T Negate();
        T Invert();
        T Add(T otherObject);
        T Subtract(T otherObject);
        T Multiply(T otherObject);
        T Divide(T otherObject);
        T Modulo(T otherObject);
    }
}