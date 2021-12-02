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
    public partial class VariableDataRange : IValueObject<VariableDataRange>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<VariableDataRange> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<VariableDataRange>(); }
        }

        IEqualityComparer<VariableDataRange> IValueObject<VariableDataRange>.ValueWiseComparer
        {
            get { return VariableDataRange.ValueWiseComparer; }
        }

        public override VariableDataRange Copy()
        {
            VariableDataRange otherObject = new VariableDataRange();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual VariableDataRange CopyNew()
        {
            VariableDataRange otherObject = new VariableDataRange();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(VariableDataRange otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(VariableDataRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(VariableDataRange otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(VariableDataRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(VariableDataRange otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (otherObject.m_IsHidden != m_IsHidden)
            { otherObject.m_IsHidden = m_IsHidden; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { otherObject.m_StyleGroup = m_StyleGroup; }

            if (otherObject.m_ValueVariableTemplateRef != m_ValueVariableTemplateRef)
            { otherObject.m_ValueVariableTemplateRef = m_ValueVariableTemplateRef; }
        }

        public override bool Equals(VariableDataRange otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(VariableDataRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(VariableDataRange otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(VariableDataRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(VariableDataRange otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (otherObject.m_IsHidden != m_IsHidden)
            { return false; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { return false; }

            if (otherObject.m_ValueVariableTemplateRef != m_ValueVariableTemplateRef)
            { return false; }

            return true;
        }
    }
}