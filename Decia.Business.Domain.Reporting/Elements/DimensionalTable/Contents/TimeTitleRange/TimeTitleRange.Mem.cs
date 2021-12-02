using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class TimeTitleRange : IValueObject<TimeTitleRange>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<TimeTitleRange> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<TimeTitleRange>(); }
        }

        IEqualityComparer<TimeTitleRange> IValueObject<TimeTitleRange>.ValueWiseComparer
        {
            get { return TimeTitleRange.ValueWiseComparer; }
        }

        public override TimeTitleRange Copy()
        {
            TimeTitleRange otherObject = new TimeTitleRange();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual TimeTitleRange CopyNew()
        {
            TimeTitleRange otherObject = new TimeTitleRange();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(TimeTitleRange otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(TimeTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(TimeTitleRange otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(TimeTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(TimeTitleRange otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_IsVariableTitleRelated != m_IsVariableTitleRelated)
            { otherObject.m_IsVariableTitleRelated = m_IsVariableTitleRelated; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (otherObject.m_IsHidden != m_IsHidden)
            { otherObject.m_IsHidden = m_IsHidden; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { otherObject.m_StyleGroup = m_StyleGroup; }

            if (otherObject.m_OnlyRepeatOnChange != m_OnlyRepeatOnChange)
            { otherObject.m_OnlyRepeatOnChange = m_OnlyRepeatOnChange; }
            if (otherObject.m_MergeRepeatedValues != m_MergeRepeatedValues)
            { otherObject.m_MergeRepeatedValues = m_MergeRepeatedValues; }

            if (otherObject.m_TimeDimensionType != m_TimeDimensionType)
            { otherObject.m_TimeDimensionType = m_TimeDimensionType; }
            if (otherObject.m_TimePeriodType != m_TimePeriodType)
            { otherObject.m_TimePeriodType = m_TimePeriodType; }
        }

        public override bool Equals(TimeTitleRange otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(TimeTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(TimeTitleRange otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(TimeTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(TimeTitleRange otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_IsVariableTitleRelated != m_IsVariableTitleRelated)
            { return false; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (otherObject.m_IsHidden != m_IsHidden)
            { return false; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { return false; }

            if (otherObject.m_OnlyRepeatOnChange != m_OnlyRepeatOnChange)
            { return false; }
            if (otherObject.m_MergeRepeatedValues != m_MergeRepeatedValues)
            { return false; }

            if (otherObject.m_TimeDimensionType != m_TimeDimensionType)
            { return false; }
            if (otherObject.m_TimePeriodType != m_TimePeriodType)
            { return false; }

            return true;
        }
    }
}