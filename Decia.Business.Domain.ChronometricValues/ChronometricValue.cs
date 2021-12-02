using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues.TimeAssessments;
using Decia.Business.Domain.ChronometricValues.TimeDimensions;

namespace Decia.Business.Domain.ChronometricValues
{
    public partial class ChronometricValue : ProjectDomainObjectBase_Deleteable<ChronometricValueId, ChronometricValue>, IChronometricValue<ChronometricValue>, IParentIdProvider
    {
        #region Static Members

        public const ChronometricValue NullInstance = null;
        public const object NullInstanceAsObject = null;
        public static readonly DeciaDataType DefaultDataType = DeciaDataType.Decimal;

        public static CompoundUnit GetGlobalScalarUnit(Guid projectGuid, long revisionNumber)
        { return CompoundUnit.GetGlobalScalarUnit(projectGuid, revisionNumber); }

        #endregion

        #region Members

        protected Nullable<ModelObjectType> m_ParentModelObjectType;
        protected SortedDictionary<ModelObjectType, ModelObjectReference> m_ParentModelObjectRefs;

        protected DeciaDataType m_DataType;
        protected SaveableTimeDimension m_PrimaryTimeDimension;
        protected SaveableTimeDimension m_SecondaryTimeDimension;
        protected SortedDictionary<MultiTimePeriodKey, TimeAssessment> m_TimeAssessments;
        protected IList<MultiTimePeriodKey> m_ImpliedTimeKeys;

        protected DynamicValue m_DefaultValue;
        protected CompoundUnit m_Unit;

        #endregion

        #region Constructors

        public ChronometricValue()
            : this(ChronometricValueId.DefaultId.ProjectGuid, ChronometricValueId.DefaultId.RevisionNumber_NonNull)
        { }

        public ChronometricValue(Guid projectGuid, long revisionNumber)
            : this(projectGuid, revisionNumber, DefaultDataType)
        { }

        public ChronometricValue(Guid projectGuid, long revisionNumber, DeciaDataType dataType)
            : this(projectGuid, revisionNumber, Guid.NewGuid(), dataType)
        { }

        public ChronometricValue(Guid projectGuid, long revisionNumber, Guid chronometricValueGuid, DeciaDataType dataType)
            : this(projectGuid, revisionNumber, chronometricValueGuid, dataType, null, null)
        { }

        public ChronometricValue(Guid projectGuid, long revisionNumber, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, DefaultDataType, parentModelObjectType, parentModelObjectRefs)
        { }

        public ChronometricValue(Guid projectGuid, long revisionNumber, DeciaDataType dataType, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, Guid.NewGuid(), dataType, parentModelObjectType, parentModelObjectRefs)
        { }

        public ChronometricValue(Guid projectGuid, long revisionNumber, Guid chronometricValueGuid, DeciaDataType dataType, ModelObjectType parentModelObjectType, IEnumerable<ModelObjectReference> parentModelObjectRefs)
            : this(projectGuid, revisionNumber, chronometricValueGuid, dataType, parentModelObjectType, parentModelObjectRefs.ToDictionary(x => x.ModelObjectType, x => x))
        { }

        protected ChronometricValue(Guid projectGuid, long revisionNumber, Guid chronometricValueGuid, DeciaDataType dataType, Nullable<ModelObjectType> parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
            : base(projectGuid, revisionNumber)
        {
            m_Key = new ChronometricValueId(projectGuid, revisionNumber, chronometricValueGuid);

            if (!parentModelObjectType.HasValue)
            { ClearParent(); }
            else
            { SetParent(parentModelObjectType.Value, parentModelObjectRefs); }

            m_DataType = dataType;
            m_DefaultValue = new DynamicValue(InternalDataTypeGetter);

            m_PrimaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Primary);
            m_SecondaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Secondary);
            m_TimeAssessments = new SortedDictionary<MultiTimePeriodKey, TimeAssessment>();
            m_ImpliedTimeKeys = ChronometricValueUtils.GetImpliedTimeKeys(this);

            m_Unit = null;
        }

        #endregion

        #region Base-Class Method Overrides

        protected override Guid GetProjectGuid()
        {
            return EF_ProjectGuid;
        }

        protected override long? GetRevisionNumber()
        {
            return EF_RevisionNumber;
        }

        protected override void SetProjectGuid(Guid projectGuid)
        {
            EF_ProjectGuid = projectGuid;
        }

        protected override void SetRevisionNumber(long revisionNumber)
        {
            EF_RevisionNumber = revisionNumber;
        }

        #endregion

        internal Nullable<DeciaDataType> InternalDataTypeGetter()
        { return m_DataType; }

        internal DynamicValue NullValueGetter()
        { return new DynamicValue(DataType, null); }

        internal DynamicValue DefaultValueGetter()
        {
            if (m_DefaultValue == DynamicValue.NullInstanceAsObject)
            { return new DynamicValue(InternalDataTypeGetter); }
            else
            { return m_DefaultValue.Copy(); }
        }

        [NotMapped]
        public int Count
        {
            get { return TimeKeys.Count; }
        }

        [NotMapped]
        public DeciaDataType DataType
        {
            get { return m_DataType; }
            set
            {
                if (m_DataType == value)
                { return; }

                m_DataType = value;
                m_DefaultValue.ResetToDefault();

                foreach (TimeAssessment timeAssessment in m_TimeAssessments.Values)
                { timeAssessment.Value.ResetToDefault(); }
            }
        }

        [NotMapped]
        public TimeDimensionType ValidTimeDimensions
        {
            get
            {
                TimeDimensionType timeDimensionType = TimeDimensionType.None;
                if (m_PrimaryTimeDimension.HasTimeValue)
                { timeDimensionType = timeDimensionType | TimeDimensionType.Primary; }
                if (m_SecondaryTimeDimension.HasTimeValue)
                { timeDimensionType = timeDimensionType | TimeDimensionType.Secondary; }
                return timeDimensionType;
            }
        }

        [NotMapped]
        public ITimeDimensionSet TimeDimensionSet
        {
            get { return new TimeDimensionSet(m_PrimaryTimeDimension, m_SecondaryTimeDimension); }
        }

        [NotMapped]
        public ITimeDimension PrimaryTimeDimension
        {
            get { return m_PrimaryTimeDimension; }
        }

        [NotMapped]
        public ITimeDimension SecondaryTimeDimension
        {
            get { return m_SecondaryTimeDimension; }
        }

        [NotMapped]
        public bool HasTimeSpecificValue
        {
            get
            {
                var timeDimensionSet = TimeDimensionSet;

                for (int dimNum = timeDimensionSet.MinimumDimensionNumber; dimNum <= timeDimensionSet.MaximumDimensionNumber; dimNum++)
                {
                    if (timeDimensionSet.GetTimeDimension(dimNum).HasTimeValue)
                    { return true; }
                }
                return false;
            }
        }

        [NotMapped]
        public DynamicValue DefaultValue
        {
            get
            { return m_DefaultValue; }
            set
            {
                if (value == DynamicValue.NullInstanceAsObject)
                {
                    m_DefaultValue = new DynamicValue(InternalDataTypeGetter, null);
                    return;
                }

                DeciaDataTypeUtils.AssertTypesAreCompatible(value.DataType, m_DataType);

                m_DefaultValue = new DynamicValue(InternalDataTypeGetter, value.GetValue());
            }
        }

        [NotMapped]
        public CompoundUnit Unit
        {
            get { return m_Unit; }
            set { m_Unit = value; }
        }

        [NotMapped]
        public IList<MultiTimePeriodKey> TimeKeys
        {
            get { return m_ImpliedTimeKeys.ToList(); }
        }

        [NotMapped]
        public IList<MultiTimePeriodKey> StoredTimeKeys
        {
            get { return m_TimeAssessments.Keys.ToList(); }
        }

        public void ReDimension(ITimeDimensionSet timeDimensionSet)
        {
            if (timeDimensionSet == null)
            { ReDimension(null, null); }
            else
            { ReDimension(timeDimensionSet.PrimaryTimeDimension, timeDimensionSet.SecondaryTimeDimension); }
        }

        public void ReDimension(ITimeDimension primaryTimeDimension, ITimeDimension secondaryTimeDimension)
        {
            if (primaryTimeDimension == null)
            { primaryTimeDimension = TimeframeUtils.CreateEmptyTimeDimension(m_PrimaryTimeDimension.TimeDimensionType); }
            if (secondaryTimeDimension == null)
            { secondaryTimeDimension = TimeframeUtils.CreateEmptyTimeDimension(m_SecondaryTimeDimension.TimeDimensionType); }

            if (secondaryTimeDimension.HasTimeValue)
            { ITimeDimensionUtils.AssertSecondaryTimeDimensionIsValid(TimeDimensionType.Secondary, secondaryTimeDimension.TimeValueType); }

            m_TimeAssessments.Clear();
            m_ImpliedTimeKeys.Clear();


            if (!primaryTimeDimension.HasTimeValue)
            {
                m_PrimaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Primary, primaryTimeDimension);
            }
            else
            {
                if (primaryTimeDimension.TimeValueType == TimeValueType.SpotValue)
                {
                    m_PrimaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Primary, primaryTimeDimension);
                }
                else if (primaryTimeDimension.TimeValueType == TimeValueType.PeriodValue)
                {
                    m_PrimaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Primary, primaryTimeDimension);
                }
                else
                { throw new InvalidOperationException("Unexpected PrimaryTimeDimension values encountered."); }
            }

            if (!secondaryTimeDimension.HasTimeValue)
            {
                m_SecondaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Secondary, secondaryTimeDimension);
            }
            else
            {
                if (secondaryTimeDimension.TimeValueType == TimeValueType.PeriodValue)
                {
                    m_SecondaryTimeDimension = new SaveableTimeDimension(m_Key, TimeDimensionType.Secondary, secondaryTimeDimension);
                }
                else
                { throw new InvalidOperationException("Unexpected SecondaryTimeDimension values encountered."); }
            }

            m_ImpliedTimeKeys = ChronometricValueUtils.GetImpliedTimeKeys(this);
        }

        public DynamicValue GetValue(MultiTimePeriodKey timeKey)
        {
            MultiTimePeriodKey actualTimeKey;
            return GetValue(timeKey, out actualTimeKey);
        }

        public DynamicValue GetValue(MultiTimePeriodKey timeKey, out MultiTimePeriodKey actualTimeKey)
        {
            MultiTimePeriodKey? matching = TimeKeys.GetCorrespondingTimeKey(timeKey);

            if (!matching.HasValue)
            {
                actualTimeKey = timeKey;
                return DefaultValueGetter();
            }

            actualTimeKey = matching.Value;

            if (!m_TimeAssessments.ContainsKey(actualTimeKey))
            { return DefaultValueGetter(); }
            return m_TimeAssessments[matching.Value].Value.Copy();
        }

        public DynamicValue GetValue(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod)
        {
            MultiTimePeriodKey timeKey = new MultiTimePeriodKey(primaryTimePeriod, secondaryTimePeriod);
            return GetValue(timeKey);
        }

        public void SetValue(MultiTimePeriodKey timeKey, object value)
        {
            AssertTimeKeyMatchesTimeDimensions(this, timeKey);

            if (value is DynamicValue)
            { value = ((DynamicValue)value).GetValue(); }

            if (!m_TimeAssessments.ContainsKey(timeKey))
            {
                TimeAssessment timeAssessment = new TimeAssessment(this.Key, Guid.NewGuid(), timeKey, InternalDataTypeGetter);
                timeAssessment.Value.SetValue(value);
                m_TimeAssessments.Add(timeKey, timeAssessment);
            }
            else
            {
                TimeAssessment timeAssessment = m_TimeAssessments[timeKey];
                timeAssessment.Value.SetValue(value);
            }
        }

        public void SetValue(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod, object value)
        {
            MultiTimePeriodKey timeKey = new MultiTimePeriodKey(primaryTimePeriod, secondaryTimePeriod);
            SetValue(timeKey, value);
        }

        public void RemoveValue(MultiTimePeriodKey timeKey)
        {
            if (!m_TimeAssessments.ContainsKey(timeKey))
            { throw new InvalidOperationException("The specified time key does not have an assessment."); }

            m_TimeAssessments.Remove(timeKey);
        }

        public void RemoveValue(Nullable<TimePeriod> primaryTimePeriod, Nullable<TimePeriod> secondaryTimePeriod)
        {
            MultiTimePeriodKey timeKey = new MultiTimePeriodKey(primaryTimePeriod, secondaryTimePeriod);
            RemoveValue(timeKey);
        }

        public void ClearValues()
        {
            m_TimeAssessments.Clear();
        }

        public void ClearValues(TimeDimensionType timeDimensionType, TimePeriod timePeriod)
        {
            ITimeDimensionUtils.AssertTimeDimensionTypeIsValid(timeDimensionType);

            foreach (MultiTimePeriodKey timeKey in m_TimeAssessments.Keys.ToList())
            {
                if (timeDimensionType == TimeDimensionType.Primary)
                {
                    if (timeKey.NullablePrimaryTimePeriod != timePeriod)
                    { continue; }
                }
                else if (timeDimensionType == TimeDimensionType.Secondary)
                {
                    if (timeKey.NullableSecondaryTimePeriod != timePeriod)
                    { continue; }
                }
                else
                { throw new InvalidOperationException("Invalid TimeDimensionType encountered."); }

                m_TimeAssessments.Remove(timeKey);
            }
        }

        public void MergeValues(ChronometricValue otherObject, MultiTimePeriodKey otherTimeKey, bool aggregateValues)
        {
            var otherTimeKeys = new MultiTimePeriodKey[] { otherTimeKey };
            MergeValues(otherObject, otherTimeKeys, aggregateValues);
        }

        public void MergeValues(ChronometricValue otherObject, IEnumerable<MultiTimePeriodKey> otherTimeKeys, bool aggregateValues)
        {
            if (((object)this) == ((object)otherObject))
            { return; }

            foreach (var otherTimeKey in otherTimeKeys)
            {
                var incomingValue = otherObject.GetValue(otherTimeKey);

                if (!aggregateValues)
                {
                    this.SetValue(otherTimeKey, incomingValue);
                    continue;
                }

                MultiTimePeriodKey relevantTimeKey;
                var existingValue = this.GetValue(otherTimeKey, out relevantTimeKey);

                if (existingValue.IsNull)
                {
                    this.SetValue(relevantTimeKey, incomingValue);
                }
                else
                {
                    existingValue += incomingValue;
                    this.SetValue(relevantTimeKey, existingValue);
                }
            }
        }

        public void AssignValuesTo(ChronometricValue otherObject)
        {
            if (((object)this) == ((object)otherObject))
            { return; }

            otherObject.m_TimeAssessments.Clear();

            foreach (MultiTimePeriodKey timeKey in m_TimeAssessments.Keys)
            {
                DynamicValue thisValue = this.GetValue(timeKey);
                otherObject.SetValue(timeKey, thisValue);
            }
        }

        #region Value-wise Comparison Methods

        public bool IsSameValue(ChronometricValue otherObject)
        {
            if (!otherObject.m_TimeAssessments.Keys.AreUnorderedCollectionsEqual(m_TimeAssessments.Keys))
            { return false; }

            foreach (MultiTimePeriodKey timeKey in m_TimeAssessments.Keys)
            {
                DynamicValue thisValue = this.GetValue(timeKey);
                DynamicValue otherValue = otherObject.GetValue(timeKey);
                if (!otherValue.Equals(thisValue))
                { return false; }
            }
            return true;
        }

        public bool IsNotSameValue(ChronometricValue otherObject)
        {
            return !IsSameValue(otherObject);
        }

        #endregion

        #region Computation-Related Methods

        public IList<MultiTimePeriodKey> GetTimeKeysForIteration(ChronometricValue other)
        {
            return ChronometricValueUtils.GetTimeKeysForIteration(this, other);
        }

        public ITimeDimensionSet GetDimensionsForResult(ChronometricValue other)
        {
            return ChronometricValueUtils.GetDimensionsForResult(this, other);
        }

        public ChronometricValue CreateDimensionedResult(ChronometricValue other, DeciaDataType resultingDataType)
        {
            return ChronometricValueUtils.CreateDimensionedResult(this, other, resultingDataType);
        }

        #endregion

        #region Logical Operation Methods

        public ChronometricValue Not()
        {
            List<MultiTimePeriodKey> timeKeys = this.m_TimeAssessments.Keys.ToList();
            ChronometricValue result = this.CopyNew();

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue originalValue = this.GetValue(timeKey);
                DynamicValue invertedValue = !originalValue;
                result.SetValue(timeKey, invertedValue);
            }
            return result;
        }

        public ChronometricValue Equivalent(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue == secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue NotEquivalent(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue != secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue LessThan(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue < secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue LessThanOrEquivalent(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue <= secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue GreaterThan(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue > secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue GreaterThanOrEquivalent(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue >= secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue And(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue & secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue Or(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue | secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        public ChronometricValue Xor(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue ^ secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }
            return result;
        }

        #endregion

        #region Mathematical Operation Methods

        public ChronometricValue Negate()
        {
            List<MultiTimePeriodKey> timeKeys = this.m_TimeAssessments.Keys.ToList();
            ChronometricValue result = this.CopyNew();

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue originalValue = this.GetValue(timeKey);
                DynamicValue invertedValue = -originalValue;
                result.SetValue(timeKey, invertedValue);
            }

            if (this.Unit != null)
            { result.Unit = this.Unit.CopyNew(); }

            return result;
        }

        public ChronometricValue Invert()
        {
            List<MultiTimePeriodKey> timeKeys = this.m_TimeAssessments.Keys.ToList();
            ChronometricValue result = this.CopyNew();

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue oneValue = new DynamicValue(DeciaDataType.Decimal, 1.0);
                DynamicValue originalValue = this.GetValue(timeKey);
                DynamicValue invertedValue = oneValue / originalValue;
                result.SetValue(timeKey, invertedValue);
            }

            if (this.Unit != null)
            { result.Unit = this.Unit.Invert(); }

            return result;
        }

        public ChronometricValue Add(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue addedValue = firstValue + secondValue;

                resultingDataType = addedValue.DataType;
                resultingValues.Add(timeKey, addedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }

            if ((this.Unit != null) && (otherObject.Unit != null))
            { result.Unit = this.Unit.Add(otherObject.Unit); }
            else if (this.Unit != null)
            { result.Unit = this.Unit.CopyNew(); }
            else if (otherObject.Unit != null)
            { result.Unit = otherObject.Unit.CopyNew(); }

            return result;
        }

        public ChronometricValue Subtract(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue subtractedValue = firstValue - secondValue;

                resultingDataType = subtractedValue.DataType;
                resultingValues.Add(timeKey, subtractedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }

            if ((this.Unit != null) && (otherObject.Unit != null))
            { result.Unit = this.Unit.Subtract(otherObject.Unit); }
            else if (this.Unit != null)
            { result.Unit = this.Unit.CopyNew(); }
            else if (otherObject.Unit != null)
            { result.Unit = otherObject.Unit.CopyNew(); }

            return result;
        }

        public ChronometricValue Multiply(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue multipliedValue = firstValue * secondValue;

                resultingDataType = multipliedValue.DataType;
                resultingValues.Add(timeKey, multipliedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }

            if ((this.Unit != null) && (otherObject.Unit != null))
            { result.Unit = this.Unit.Multiply(otherObject.Unit); }
            else if (this.Unit != null)
            { result.Unit = this.Unit.CopyNew(); }
            else if (otherObject.Unit != null)
            { result.Unit = otherObject.Unit.CopyNew(); }

            return result;
        }

        public ChronometricValue Divide(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue dividedValue = firstValue / secondValue;

                resultingDataType = dividedValue.DataType;
                resultingValues.Add(timeKey, dividedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }

            if ((this.Unit != null) && (otherObject.Unit != null))
            { result.Unit = this.Unit.Divide(otherObject.Unit); }
            else if (this.Unit != null)
            { result.Unit = this.Unit.CopyNew(); }
            else if (otherObject.Unit != null)
            { result.Unit = otherObject.Unit.CopyNew(); }

            return result;
        }

        public ChronometricValue Modulo(ChronometricValue otherObject)
        {
            IList<MultiTimePeriodKey> timeKeys = this.GetTimeKeysForIteration(otherObject);
            Dictionary<MultiTimePeriodKey, DynamicValue> resultingValues = new Dictionary<MultiTimePeriodKey, DynamicValue>();
            DeciaDataType resultingDataType = DeciaDataType.Text;

            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                DynamicValue firstValue = this.GetValue(timeKey);
                DynamicValue secondValue = otherObject.GetValue(timeKey);
                DynamicValue moduloedValue = firstValue % secondValue;

                resultingDataType = moduloedValue.DataType;
                resultingValues.Add(timeKey, moduloedValue);
            }

            ChronometricValue result = this.CreateDimensionedResult(otherObject, resultingDataType);
            foreach (MultiTimePeriodKey timeKey in timeKeys)
            {
                result.SetValue(timeKey, resultingValues[timeKey]);
            }

            if ((this.Unit != null) && (otherObject.Unit != null))
            { result.Unit = this.Unit.Modulo(otherObject.Unit); }
            else if (this.Unit != null)
            { result.Unit = this.Unit.CopyNew(); }
            else if (otherObject.Unit != null)
            { result.Unit = otherObject.Unit.CopyNew(); }

            return result;
        }

        #endregion

        #region Basic Equivalence Operators

        public static bool operator ==(ChronometricValue firstArg, object secondArg)
        {
            if (((object)firstArg) == null)
            { return (secondArg == null); }

            return (firstArg.Equals(secondArg));
        }

        public static bool operator !=(ChronometricValue firstArg, object secondArg)
        {
            return (!(firstArg == secondArg));
        }

        #endregion

        #region Logical Operators

        public static ChronometricValue operator !(ChronometricValue firstArg)
        {
            return firstArg.Not();
        }

        public static ChronometricValue operator ==(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Equivalent(secondArg);
        }

        public static ChronometricValue operator !=(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.NotEquivalent(secondArg);
        }

        public static ChronometricValue operator <(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.LessThan(secondArg);
        }

        public static ChronometricValue operator <=(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.LessThanOrEquivalent(secondArg);
        }

        public static ChronometricValue operator >(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.GreaterThan(secondArg);
        }

        public static ChronometricValue operator >=(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.GreaterThanOrEquivalent(secondArg);
        }

        public static ChronometricValue operator &(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.And(secondArg);
        }

        public static ChronometricValue operator |(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Or(secondArg);
        }

        public static ChronometricValue operator ^(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Xor(secondArg);
        }

        #endregion

        #region Mathematical Operators

        public static ChronometricValue operator -(ChronometricValue firstArg)
        {
            return firstArg.Negate();
        }

        public static ChronometricValue operator +(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Add(secondArg);
        }

        public static ChronometricValue operator -(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Subtract(secondArg);
        }

        public static ChronometricValue operator *(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Multiply(secondArg);
        }

        public static ChronometricValue operator /(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Divide(secondArg);
        }

        public static ChronometricValue operator %(ChronometricValue firstArg, ChronometricValue secondArg)
        {
            return firstArg.Modulo(secondArg);
        }

        #endregion

        #region IProjectMember<ChronometricValue> Implementation

        public ChronometricValue CopyForRevision(long newRevisionNumber)
        {
            if (this.Key.RevisionNumber >= newRevisionNumber)
            { throw new InvalidOperationException("A ChronometricValue created for a new revision must have a greater revision number."); }

            return (this as IProjectMember<ChronometricValue>).CopyForProject(this.Key.ProjectGuid, newRevisionNumber);
        }

        ChronometricValue IProjectMember<ChronometricValue>.CopyForProject(Guid projectGuid, long revisionNumber)
        {
            var newValue = this.Copy();

            newValue.EF_ProjectGuid = projectGuid;
            newValue.EF_RevisionNumber = revisionNumber;

            newValue.m_PrimaryTimeDimension.EF_ProjectGuid = projectGuid;
            newValue.m_PrimaryTimeDimension.EF_RevisionNumber = revisionNumber;

            newValue.m_SecondaryTimeDimension.EF_ProjectGuid = projectGuid;
            newValue.m_SecondaryTimeDimension.EF_RevisionNumber = revisionNumber;

            foreach (var newTimeAssessment in newValue.m_TimeAssessments.Values)
            {
                newTimeAssessment.EF_ProjectGuid = projectGuid;
                newTimeAssessment.EF_RevisionNumber = revisionNumber;
            }
            return newValue;
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return Key.ModelObjectType; }
        }

        public Guid ModelObjectId
        {
            get { return Key.ModelObjectId; }
        }

        public bool IdIsInt
        {
            get { return Key.IdIsInt; }
        }

        public int ModelObjectIdAsInt
        {
            get { return Key.ModelObjectIdAsInt; }
        }

        public string ComplexId
        {
            get { return Key.ComplexId; }
        }

        public ModelObjectReference ModelObjectRef
        {
            get { return Key.ModelObjectRef; }
        }

        #endregion

        #region IParentIdProvider Implementation

        [NotMapped]
        public bool HasParent
        {
            get { return m_ParentModelObjectType.HasValue; }
        }

        [NotMapped]
        public ModelObjectType ParentModelObjectType
        {
            get
            {
                this.AssertHasParent();
                return m_ParentModelObjectType.Value;
            }
        }

        [NotMapped]
        public IDictionary<ModelObjectType, ModelObjectReference> ParentModelObjectRefs
        {
            get
            {
                this.AssertHasParent();
                return m_ParentModelObjectRefs;
            }
        }

        public void SetParent(IParentId parentId)
        {
            SetParent(parentId.ModelObjectType, parentId.IdReferences.ToDictionary(x => x.ModelObjectType, x => x));
        }

        public void SetParent(ModelObjectType parentModelObjectType, IDictionary<ModelObjectType, ModelObjectReference> parentModelObjectRefs)
        {
            parentModelObjectType.AssertHasParentTypeValue(parentModelObjectRefs);

            m_ParentModelObjectType = parentModelObjectType;
            m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(parentModelObjectRefs);
        }

        public void ClearParent()
        {
            m_ParentModelObjectType = null;
            m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>();
        }

        public T GetParentId<T>(Func<IParentIdProvider, T> idConversionMethod)
        {
            this.AssertHasParent();
            return idConversionMethod(this);
        }

        #endregion

        #region Assertion-related Methods

        public static bool TryAssertTimeKeyMatchesTimeDimensions(ChronometricValue chronometricValue, MultiTimePeriodKey timeKey, ref string message)
        {
            if (chronometricValue.PrimaryTimeDimension.HasTimeValue != timeKey.HasPrimaryTimePeriod)
            {
                if (chronometricValue.PrimaryTimeDimension.HasTimeValue)
                { message = "The PrimaryTimeDimension must exist."; }
                else
                { message = "The PrimaryTimeDimension must not exist."; }
                return false;
            }
            if (chronometricValue.SecondaryTimeDimension.HasTimeValue != timeKey.HasSecondaryTimePeriod)
            {
                if (chronometricValue.PrimaryTimeDimension.HasTimeValue)
                { message = "The SecondaryTimeDimension must exist."; }
                else
                { message = "The SecondaryTimeDimension must not exist."; }
                return false;
            }

            if (chronometricValue.PrimaryTimeDimension.HasTimeValue)
            {
                if (chronometricValue.PrimaryTimeDimension.TimeValueType != timeKey.PrimaryTimePeriod.TimeValueType)
                {
                    message = "The specified PrimaryTimeValueType does not correspond to the required TimeValueType.";
                    return false;
                }
                if (chronometricValue.PrimaryTimeDimension.UsesTimePeriods)
                {
                    DateTime periodStartDate = timeKey.PrimaryTimePeriod.StartDate;
                    DateTime periodEndDate = timeKey.PrimaryTimePeriod.EndDate;
                    DateTime expectedPeriodEndDate = chronometricValue.PrimaryTimeDimension.TimePeriodType.GetCurrentEndDate(periodStartDate);

                    if (periodEndDate != expectedPeriodEndDate)
                    {
                        message = "The specified PrimaryTimePeriod does not correspond to the required TimePeriodType.";
                        return false;
                    }
                }
            }
            if (chronometricValue.SecondaryTimeDimension.HasTimeValue)
            {
                if (chronometricValue.SecondaryTimeDimension.TimeValueType != timeKey.SecondaryTimePeriod.TimeValueType)
                {
                    message = "The specified SecondaryTimeValueType does not correspond to the required TimeValueType.";
                    return false;
                }
                if (chronometricValue.SecondaryTimeDimension.UsesTimePeriods)
                {
                    DateTime periodStartDate = timeKey.SecondaryTimePeriod.StartDate;
                    DateTime periodEndDate = timeKey.SecondaryTimePeriod.EndDate;
                    DateTime expectedPeriodEndDate = chronometricValue.SecondaryTimeDimension.TimePeriodType.GetCurrentEndDate(periodStartDate);

                    if (periodEndDate != expectedPeriodEndDate)
                    {
                        message = "The specified SecondaryTimePeriod does not correspond to the required TimePeriodType.";
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryAssertTimeKeyMatchesTimeDimensions(ChronometricValue chronometricValue, MultiTimePeriodKey timeKey)
        {
            string message = string.Empty;
            return TryAssertTimeKeyMatchesTimeDimensions(chronometricValue, timeKey, ref message);
        }

        public static void AssertTimeKeyMatchesTimeDimensions(ChronometricValue chronometricValue, MultiTimePeriodKey timeKey)
        {
            string message = string.Empty;
            if (!TryAssertTimeKeyMatchesTimeDimensions(chronometricValue, timeKey, ref message))
            { throw new InvalidOperationException(message); }
        }

        #endregion

        #region Debugging Methods

        public bool[] GetDebugTimeDims()
        {
            bool[] hasDims = new bool[2];
            hasDims[0] = PrimaryTimeDimension.HasTimeValue;
            hasDims[1] = SecondaryTimeDimension.HasTimeValue;
            return hasDims;
        }

        public object GetDebugValue_0D()
        {
            return GetValue(null, null).GetValue();
        }

        public object[] GetDebugValue_1D()
        {
            object[] values = new object[TimeKeys.Count];
            int i = 0;

            foreach (var timeKey in TimeKeys)
            {
                values[i] = GetValue(timeKey).GetValue();
                i++;
            }
            return values;
        }

        public object[,] GetDebugValue_2D()
        {
            var primaryPeriods = PrimaryTimeDimension.GeneratePeriodsForTimeDimension();
            var secondaryPeriods = SecondaryTimeDimension.GeneratePeriodsForTimeDimension();
            object[,] values = new object[primaryPeriods.Count, secondaryPeriods.Count];
            int i = 0;
            int j = 0;

            foreach (var primaryPeriod in primaryPeriods)
            {
                j = 0;
                foreach (var secondaryPeriod in secondaryPeriods)
                {
                    var timeKey = new MultiTimePeriodKey(primaryPeriod, secondaryPeriod);
                    values[i, j] = GetValue(timeKey).GetValue();
                    j++;
                }
                i++;
            }
            return values;
        }

        #endregion
    }
}