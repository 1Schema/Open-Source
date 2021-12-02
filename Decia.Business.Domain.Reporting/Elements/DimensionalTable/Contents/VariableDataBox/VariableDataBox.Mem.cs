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
    public partial class VariableDataBox : IValueObject<VariableDataBox>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<VariableDataBox> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<VariableDataBox>(); }
        }

        IEqualityComparer<VariableDataBox> IValueObject<VariableDataBox>.ValueWiseComparer
        {
            get { return VariableDataBox.ValueWiseComparer; }
        }

        public override VariableDataBox Copy()
        {
            VariableDataBox otherObject = new VariableDataBox();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual VariableDataBox CopyNew()
        {
            VariableDataBox otherObject = new VariableDataBox();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(VariableDataBox otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(VariableDataBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(VariableDataBox otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(VariableDataBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(VariableDataBox otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_RelatedVariableTitleBoxNumber != m_RelatedVariableTitleBoxNumber)
            { otherObject.m_RelatedVariableTitleBoxNumber = m_RelatedVariableTitleBoxNumber; }
            if (otherObject.m_RelatedCommonTitleBoxNumber != m_RelatedCommonTitleBoxNumber)
            { otherObject.m_RelatedCommonTitleBoxNumber = m_RelatedCommonTitleBoxNumber; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (otherObject.m_ContainedVariableDataRangeNumber != m_ContainedVariableDataRangeNumber)
            { otherObject.m_ContainedVariableDataRangeNumber = m_ContainedVariableDataRangeNumber; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { otherObject.m_StyleGroup = m_StyleGroup; }

            if (otherObject.m_VariableTemplateRef != m_VariableTemplateRef)
            { otherObject.m_VariableTemplateRef = m_VariableTemplateRef; }
        }

        public override bool Equals(VariableDataBox otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(VariableDataBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(VariableDataBox otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(VariableDataBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(VariableDataBox otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_RelatedVariableTitleBoxNumber != m_RelatedVariableTitleBoxNumber)
            { return false; }
            if (otherObject.m_RelatedCommonTitleBoxNumber != m_RelatedCommonTitleBoxNumber)
            { return false; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (otherObject.m_ContainedVariableDataRangeNumber != m_ContainedVariableDataRangeNumber)
            { return false; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { return false; }

            if (otherObject.m_VariableTemplateRef != m_VariableTemplateRef)
            { return false; }

            return true;
        }
    }
}