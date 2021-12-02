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
    public partial class VariableTitleBox : IValueObject<VariableTitleBox>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<VariableTitleBox> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<VariableTitleBox>(); }
        }

        IEqualityComparer<VariableTitleBox> IValueObject<VariableTitleBox>.ValueWiseComparer
        {
            get { return VariableTitleBox.ValueWiseComparer; }
        }

        public override VariableTitleBox Copy()
        {
            VariableTitleBox otherObject = new VariableTitleBox();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual VariableTitleBox CopyNew()
        {
            VariableTitleBox otherObject = new VariableTitleBox();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(VariableTitleBox otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(VariableTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(VariableTitleBox otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(VariableTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(VariableTitleBox otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (otherObject.m_ContainedVariableTitleRangeNumber != m_ContainedVariableTitleRangeNumber)
            { otherObject.m_ContainedVariableTitleRangeNumber = m_ContainedVariableTitleRangeNumber; }
            if (!otherObject.m_ContainedStructuralTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedStructuralTitleRangeNumbers))
            { otherObject.m_ContainedStructuralTitleRangeNumbers = new List<int>(m_ContainedStructuralTitleRangeNumbers); }
            if (!otherObject.m_ContainedTimeTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedTimeTitleRangeNumbers))
            { otherObject.m_ContainedTimeTitleRangeNumbers = new List<int>(m_ContainedTimeTitleRangeNumbers); }

            if (otherObject.m_RepeatGroup != m_RepeatGroup)
            { otherObject.m_RepeatGroup = m_RepeatGroup; }
            if (otherObject.m_RepeatMode != m_RepeatMode)
            { otherObject.m_RepeatMode = m_RepeatMode; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { otherObject.m_StyleGroup = m_StyleGroup; }

            if (otherObject.m_VariableTemplateRef != m_VariableTemplateRef)
            { otherObject.m_VariableTemplateRef = m_VariableTemplateRef; }
        }

        public override bool Equals(VariableTitleBox otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(VariableTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(VariableTitleBox otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(VariableTitleBox otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(VariableTitleBox otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (otherObject.m_ContainedVariableTitleRangeNumber != m_ContainedVariableTitleRangeNumber)
            { return false; }
            if (!otherObject.m_ContainedStructuralTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedStructuralTitleRangeNumbers))
            { return false; }
            if (!otherObject.m_ContainedTimeTitleRangeNumbers.AreUnorderedCollectionsEqual(m_ContainedTimeTitleRangeNumbers))
            { return false; }

            if (otherObject.m_RepeatGroup != m_RepeatGroup)
            { return false; }
            if (otherObject.m_RepeatMode != m_RepeatMode)
            { return false; }
            if (otherObject.m_StyleGroup != m_StyleGroup)
            { return false; }

            if (otherObject.m_VariableTemplateRef != m_VariableTemplateRef)
            { return false; }

            return true;
        }
    }
}