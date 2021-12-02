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
    public partial class VariableTitleRange : IValueObject<VariableTitleRange>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<VariableTitleRange> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<VariableTitleRange>(); }
        }

        IEqualityComparer<VariableTitleRange> IValueObject<VariableTitleRange>.ValueWiseComparer
        {
            get { return VariableTitleRange.ValueWiseComparer; }
        }

        public override VariableTitleRange Copy()
        {
            VariableTitleRange otherObject = new VariableTitleRange();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual VariableTitleRange CopyNew()
        {
            VariableTitleRange otherObject = new VariableTitleRange();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(VariableTitleRange otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(VariableTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(VariableTitleRange otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(VariableTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(VariableTitleRange otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (otherObject.m_IsHidden != m_IsHidden)
            { otherObject.m_IsHidden = m_IsHidden; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { otherObject.m_StyleGroup = m_StyleGroup; }

            if (otherObject.m_NameVariableTemplateRef != m_NameVariableTemplateRef)
            { otherObject.m_NameVariableTemplateRef = m_NameVariableTemplateRef; }
        }

        public override bool Equals(VariableTitleRange otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(VariableTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(VariableTitleRange otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(VariableTitleRange otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(VariableTitleRange otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (otherObject.m_IsHidden != m_IsHidden)
            { return false; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { return false; }

            if (otherObject.m_NameVariableTemplateRef != m_NameVariableTemplateRef)
            { return false; }

            return true;
        }
    }
}