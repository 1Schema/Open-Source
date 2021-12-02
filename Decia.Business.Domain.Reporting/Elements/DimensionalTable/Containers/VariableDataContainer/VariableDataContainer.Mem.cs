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
    public partial class VariableDataContainer : IValueObject<VariableDataContainer>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<VariableDataContainer> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<VariableDataContainer>(); }
        }

        IEqualityComparer<VariableDataContainer> IValueObject<VariableDataContainer>.ValueWiseComparer
        {
            get { return VariableDataContainer.ValueWiseComparer; }
        }

        public override VariableDataContainer Copy()
        {
            VariableDataContainer otherObject = new VariableDataContainer();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual VariableDataContainer CopyNew()
        {
            VariableDataContainer otherObject = new VariableDataContainer();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(VariableDataContainer otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(VariableDataContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(VariableDataContainer otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(VariableDataContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(VariableDataContainer otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_VariableTitleContainerNumber != m_VariableTitleContainerNumber)
            { otherObject.m_VariableTitleContainerNumber = m_VariableTitleContainerNumber; }
            if (otherObject.m_CommonTitleContainerNumber != m_CommonTitleContainerNumber)
            { otherObject.m_CommonTitleContainerNumber = m_CommonTitleContainerNumber; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { otherObject.m_StackingDimension = m_StackingDimension; }

            if (!otherObject.m_ChildNumbers.AreUnorderedCollectionsEqual(m_ChildNumbers))
            { otherObject.m_ChildNumbers = new List<int>(m_ChildNumbers); }
        }

        public override bool Equals(VariableDataContainer otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(VariableDataContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(VariableDataContainer otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(VariableDataContainer otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(VariableDataContainer otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_VariableTitleContainerNumber != m_VariableTitleContainerNumber)
            { return false; }
            if (otherObject.m_CommonTitleContainerNumber != m_CommonTitleContainerNumber)
            { return false; }
            if (otherObject.m_StackingDimension != m_StackingDimension)
            { return false; }

            if (!otherObject.m_ChildNumbers.AreUnorderedCollectionsEqual(m_ChildNumbers))
            { return false; }

            return true;
        }
    }
}