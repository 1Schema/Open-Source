using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Domain.DynamicValues;

namespace Decia.Business.Domain.ChronometricValues.TimeAssessments
{
    public partial class TimeAssessment : IValueObject<TimeAssessment>
    {
        public static IEqualityComparer<TimeAssessment> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<TimeAssessment>(); }
        }

        IEqualityComparer<TimeAssessment> IValueObject<TimeAssessment>.ValueWiseComparer
        {
            get { return TimeAssessment.ValueWiseComparer; }
        }

        public override TimeAssessment Copy()
        {
            TimeAssessment otherObject = new TimeAssessment(ChronometricValueId.DefaultId, TimeAssessmentGuid, Key, DataTypeGetter);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual TimeAssessment CopyNew()
        {
            TimeAssessment otherObject = new TimeAssessment(ChronometricValueId.DefaultId, TimeAssessmentGuid, Key, DataTypeGetter);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(TimeAssessment otherObject)
        {
            if (otherObject.m_ParentChronometricValueId != m_ParentChronometricValueId)
            { otherObject.m_ParentChronometricValueId = m_ParentChronometricValueId; }

            this.CopyValuesTo(otherObject);
        }

        public virtual void CopyValuesTo(TimeAssessment otherObject)
        {
            if (otherObject.m_HasPrimaryDimension != m_HasPrimaryDimension)
            { otherObject.m_HasPrimaryDimension = m_HasPrimaryDimension; }
            if (otherObject.m_PrimaryStartDate != m_PrimaryStartDate)
            { otherObject.m_PrimaryStartDate = m_PrimaryStartDate; }
            if (otherObject.m_PrimaryEndDate != m_PrimaryEndDate)
            { otherObject.m_PrimaryEndDate = m_PrimaryEndDate; }
            if (otherObject.m_HasSecondaryDimension != m_HasSecondaryDimension)
            { otherObject.m_HasSecondaryDimension = m_HasSecondaryDimension; }
            if (otherObject.m_SecondaryStartDate != m_SecondaryStartDate)
            { otherObject.m_SecondaryStartDate = m_SecondaryStartDate; }
            if (otherObject.m_SecondaryEndDate != m_SecondaryEndDate)
            { otherObject.m_SecondaryEndDate = m_SecondaryEndDate; }
            if (otherObject.m_Value != ((object)m_Value))
            { otherObject.m_Value.SetValue(m_Value.DataType, m_Value.GetValue()); }

            otherObject.IsPartialObject = this.IsPartialObject;
        }

        public override bool Equals(TimeAssessment otherObject)
        {
            if (otherObject.m_ParentChronometricValueId != m_ParentChronometricValueId)
            { return false; }

            return this.EqualsValues(otherObject);
        }

        public virtual bool EqualsValues(TimeAssessment otherObject)
        {
            if (otherObject.m_HasPrimaryDimension != m_HasPrimaryDimension)
            { return false; }
            if (otherObject.m_PrimaryStartDate != m_PrimaryStartDate)
            { return false; }
            if (otherObject.m_PrimaryEndDate != m_PrimaryEndDate)
            { return false; }
            if (otherObject.m_HasSecondaryDimension != m_HasSecondaryDimension)
            { return false; }
            if (otherObject.m_SecondaryStartDate != m_SecondaryStartDate)
            { return false; }
            if (otherObject.m_SecondaryEndDate != m_SecondaryEndDate)
            { return false; }
            if (otherObject.m_Value != ((object)m_Value))
            { return false; }
            return true;
        }
    }
}