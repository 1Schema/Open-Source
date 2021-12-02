using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.ChronometricValues.TimeAssessments;
using Decia.Business.Domain.ChronometricValues.TimeDimensions;

namespace Decia.Business.Domain.ChronometricValues
{
    public partial class ChronometricValue : IValueObject<ChronometricValue>
    {
        public static bool Default_CheckCachedData_Setting = true;

        public static IEqualityComparer<ChronometricValue> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<ChronometricValue>(); }
        }

        IEqualityComparer<ChronometricValue> IValueObject<ChronometricValue>.ValueWiseComparer
        {
            get { return ChronometricValue.ValueWiseComparer; }
        }

        public override ChronometricValue Copy()
        {
            ChronometricValue otherObject = new ChronometricValue(Key.ProjectGuid, Key.RevisionNumber_NonNull);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual ChronometricValue CopyNew()
        {
            ChronometricValue otherObject = new ChronometricValue(Key.ProjectGuid, Key.RevisionNumber_NonNull);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(ChronometricValue otherObject)
        {
            this.CopyTo(otherObject, Default_CheckCachedData_Setting);
        }

        public virtual void CopyTo(ChronometricValue otherObject, bool checkCachedData)
        {
            var timeAssessmentComparer = EqualityComparer<TimeAssessment>.Default;
            var timeDimensionComparer = EqualityComparer<SaveableTimeDimension>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            if (otherObject.m_ParentModelObjectType != m_ParentModelObjectType)
            { otherObject.m_ParentModelObjectType = m_ParentModelObjectType; }
            if (!otherObject.m_ParentModelObjectRefs.AreUnorderedDictionariesEqual(m_ParentModelObjectRefs))
            { otherObject.m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(m_ParentModelObjectRefs); }

            this.CopyValuesTo(otherObject, checkCachedData, timeAssessmentComparer, timeDimensionComparer);
        }

        public virtual void CopyValuesTo(ChronometricValue otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckCachedData_Setting);
        }

        public virtual void CopyValuesTo(ChronometricValue otherObject, bool checkCachedData)
        {
            var timeAssessmentComparer = TimeAssessment.ValueWiseComparer;
            var timeDimensionComparer = SaveableTimeDimension.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkCachedData, timeAssessmentComparer, timeDimensionComparer);
        }

        protected virtual void CopyValuesTo(ChronometricValue otherObject, bool checkCachedData, IEqualityComparer<TimeAssessment> timeAssessmentComparer, IEqualityComparer<SaveableTimeDimension> timeDimensionComparer)
        {
            this.CopyBaseValuesTo(otherObject);

            if (otherObject.m_DataType != m_DataType)
            { otherObject.m_DataType = m_DataType; }
            if (otherObject.m_DefaultValue != ((object)m_DefaultValue))
            { otherObject.DefaultValue = (m_DefaultValue != ((object)null)) ? m_DefaultValue.Copy() : m_DefaultValue; }

            if (!timeDimensionComparer.Equals(otherObject.m_PrimaryTimeDimension, m_PrimaryTimeDimension))
            {
                otherObject.m_PrimaryTimeDimension = m_PrimaryTimeDimension.Copy();
                otherObject.m_PrimaryTimeDimension.ParentChronometricValueId = otherObject.Key;
            }
            if (!timeDimensionComparer.Equals(otherObject.m_SecondaryTimeDimension, m_SecondaryTimeDimension))
            {
                otherObject.m_SecondaryTimeDimension = m_SecondaryTimeDimension.Copy();
                otherObject.m_SecondaryTimeDimension.ParentChronometricValueId = otherObject.Key;
            }
            if (!otherObject.m_TimeAssessments.AreUnorderedDictionariesEqual(m_TimeAssessments, timeAssessmentComparer.GetUntypedEqualityComparer()))
            {
                otherObject.m_TimeAssessments = new SortedDictionary<MultiTimePeriodKey, TimeAssessment>();
                foreach (var thisTimeAssessment in m_TimeAssessments.Values)
                {
                    var thisTimeKey = thisTimeAssessment.Key;
                    var thisTimeAssessmentId = thisTimeAssessment.TimeAssessmentGuid;
                    object thisValue = thisTimeAssessment.Value.GetValue();

                    TimeAssessment otherTimeAssessment = new TimeAssessment(otherObject.Key, thisTimeAssessmentId, thisTimeKey, otherObject.InternalDataTypeGetter);
                    otherTimeAssessment.Value.SetValue(thisValue);
                    otherObject.m_TimeAssessments.Add(thisTimeKey, otherTimeAssessment);
                }
            }
            otherObject.m_ImpliedTimeKeys = m_ImpliedTimeKeys.ToList();

            if (checkCachedData)
            {
                if (otherObject.m_Unit != m_Unit)
                { otherObject.m_Unit = m_Unit; }
            }

            otherObject.IsPartialObject = this.IsPartialObject;
        }

        public override bool Equals(ChronometricValue otherObject)
        {
            return this.Equals(otherObject, Default_CheckCachedData_Setting);
        }

        public virtual bool Equals(ChronometricValue otherObject, bool checkCachedData)
        {
            var timeAssessmentComparer = EqualityComparer<TimeAssessment>.Default;
            var timeDimensionComparer = EqualityComparer<SaveableTimeDimension>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            if (otherObject.m_ParentModelObjectType != m_ParentModelObjectType)
            { return false; }
            if (!otherObject.m_ParentModelObjectRefs.AreUnorderedDictionariesEqual(m_ParentModelObjectRefs))
            { return false; }

            return this.EqualsValues(otherObject, checkCachedData, timeAssessmentComparer, timeDimensionComparer);
        }

        public virtual bool EqualsValues(ChronometricValue otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckCachedData_Setting);
        }

        public virtual bool EqualsValues(ChronometricValue otherObject, bool checkCachedData)
        {
            var timeAssessmentComparer = TimeAssessment.ValueWiseComparer;
            var timeDimensionComparer = SaveableTimeDimension.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkCachedData, timeAssessmentComparer, timeDimensionComparer);
        }

        protected virtual bool EqualsValues(ChronometricValue otherObject, bool checkCachedData, IEqualityComparer<TimeAssessment> timeAssessmentComparer, IEqualityComparer<SaveableTimeDimension> timeDimensionComparer)
        {
            if (!this.EqualsBaseValues(otherObject))
            { return false; }

            if (otherObject.m_DataType != m_DataType)
            { return false; }
            if (otherObject.m_DefaultValue != ((object)m_DefaultValue))
            { return false; }

            if (!timeDimensionComparer.Equals(otherObject.m_PrimaryTimeDimension, m_PrimaryTimeDimension))
            { return false; }
            if (!timeDimensionComparer.Equals(otherObject.m_SecondaryTimeDimension, m_SecondaryTimeDimension))
            { return false; }
            if (!otherObject.m_TimeAssessments.AreUnorderedDictionariesEqual(m_TimeAssessments, timeAssessmentComparer.GetUntypedEqualityComparer()))
            { return false; }

            if (checkCachedData)
            {
                if (otherObject.m_Unit == CompoundUnit.NullInstanceAsObject)
                {
                    if (m_Unit != CompoundUnit.NullInstanceAsObject)
                    { return false; }
                }
                else
                {
                    if (otherObject.m_Unit.EqualsValues(m_Unit))
                    { return false; }
                }
            }
            return true;
        }
    }
}