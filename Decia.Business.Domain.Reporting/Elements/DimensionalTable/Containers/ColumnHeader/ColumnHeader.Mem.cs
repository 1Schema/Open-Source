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
    public partial class ColumnHeader : IValueObject<ColumnHeader>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<ColumnHeader> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<ColumnHeader>(); }
        }

        IEqualityComparer<ColumnHeader> IValueObject<ColumnHeader>.ValueWiseComparer
        {
            get { return ColumnHeader.ValueWiseComparer; }
        }

        public override ColumnHeader Copy()
        {
            ColumnHeader otherObject = new ColumnHeader();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual ColumnHeader CopyNew()
        {
            ColumnHeader otherObject = new ColumnHeader();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(ColumnHeader otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(ColumnHeader otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(ColumnHeader otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(ColumnHeader otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(ColumnHeader otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_HoldsVariableContainer != m_HoldsVariableContainer)
            { otherObject.m_HoldsVariableContainer = m_HoldsVariableContainer; }
            if (otherObject.m_NestedTitleContainerNumber != m_NestedTitleContainerNumber)
            { otherObject.m_NestedTitleContainerNumber = m_NestedTitleContainerNumber; }
        }

        public override bool Equals(ColumnHeader otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(ColumnHeader otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(ColumnHeader otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(ColumnHeader otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(ColumnHeader otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_HoldsVariableContainer != m_HoldsVariableContainer)
            { return false; }
            if (otherObject.m_NestedTitleContainerNumber != m_NestedTitleContainerNumber)
            { return false; }

            return true;
        }
    }
}