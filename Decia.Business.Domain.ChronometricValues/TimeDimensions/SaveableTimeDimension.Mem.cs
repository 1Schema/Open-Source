using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Domain.ChronometricValues.TimeDimensions
{
    public partial class SaveableTimeDimension : IValueObject<SaveableTimeDimension>
    {
        public static IEqualityComparer<SaveableTimeDimension> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<SaveableTimeDimension>(); }
        }

        IEqualityComparer<SaveableTimeDimension> IValueObject<SaveableTimeDimension>.ValueWiseComparer
        {
            get { return SaveableTimeDimension.ValueWiseComparer; }
        }

        public virtual SaveableTimeDimension Copy()
        {
            SaveableTimeDimension otherObject = new SaveableTimeDimension(ChronometricValueId.DefaultId, TimeDimensionType, null, null, null, null);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual SaveableTimeDimension CopyNew()
        {
            SaveableTimeDimension otherObject = new SaveableTimeDimension(ChronometricValueId.DefaultId, TimeDimensionType, null, null, null, null);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public virtual void CopyTo(SaveableTimeDimension otherObject)
        {
            if (otherObject.m_ParentChronometricValueId != m_ParentChronometricValueId)
            { otherObject.m_ParentChronometricValueId = m_ParentChronometricValueId; }
            if (otherObject.m_TimeDimensionType != m_TimeDimensionType)
            { otherObject.m_TimeDimensionType = m_TimeDimensionType; }

            this.CopyValuesTo(otherObject);
        }

        public virtual void CopyValuesTo(SaveableTimeDimension otherObject)
        {
            if (otherObject.m_TimeValueType != m_TimeValueType)
            { otherObject.m_TimeValueType = m_TimeValueType; }
            if (otherObject.m_TimePeriodType != m_TimePeriodType)
            { otherObject.m_TimePeriodType = m_TimePeriodType; }
            if (otherObject.m_FirstPeriodStartDate != m_FirstPeriodStartDate)
            { otherObject.m_FirstPeriodStartDate = m_FirstPeriodStartDate; }
            if (otherObject.m_LastPeriodEndDate != m_LastPeriodEndDate)
            { otherObject.m_LastPeriodEndDate = m_LastPeriodEndDate; }
        }

        public virtual bool Equals(SaveableTimeDimension otherObject)
        {
            if (otherObject.m_ParentChronometricValueId != m_ParentChronometricValueId)
            { return false; }
            if (otherObject.m_TimeDimensionType != m_TimeDimensionType)
            { return false; }

            return this.EqualsValues(otherObject);
        }

        public virtual bool EqualsValues(SaveableTimeDimension otherObject)
        {
            if (otherObject.m_TimeValueType != m_TimeValueType)
            { return false; }
            if (otherObject.m_TimePeriodType != m_TimePeriodType)
            { return false; }
            if (otherObject.m_FirstPeriodStartDate != m_FirstPeriodStartDate)
            { return false; }
            if (otherObject.m_LastPeriodEndDate != m_LastPeriodEndDate)
            { return false; }
            return true;
        }

        public string GetPropertyName(Expression<Func<SaveableTimeDimension, object>> propertyGetter)
        {
            return ClassReflector.GetPropertyName<SaveableTimeDimension, object>(propertyGetter);
        }

        #region Object Overrides

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type objType = obj.GetType();
            Type thisType = typeof(SaveableTimeDimension);

            if (!thisType.Equals(objType))
            { return false; }

            SaveableTimeDimension typedObject = (SaveableTimeDimension)obj;
            return Equals(typedObject);
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        #endregion
    }
}