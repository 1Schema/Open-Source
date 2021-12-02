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
    public partial class DimensionalTable : IValueObject<DimensionalTable>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<DimensionalTable> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<DimensionalTable>(); }
        }

        IEqualityComparer<DimensionalTable> IValueObject<DimensionalTable>.ValueWiseComparer
        {
            get { return DimensionalTable.ValueWiseComparer; }
        }

        public override DimensionalTable Copy()
        {
            DimensionalTable otherObject = new DimensionalTable();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual DimensionalTable CopyNew()
        {
            DimensionalTable otherObject = new DimensionalTable();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(DimensionalTable otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(DimensionalTable otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(DimensionalTable otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(DimensionalTable otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(DimensionalTable otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);

            if (otherObject.m_IsTransposed != m_IsTransposed)
            { otherObject.m_IsTransposed = m_IsTransposed; }

            if (otherObject.m_TableHeaderNumber != m_TableHeaderNumber)
            { otherObject.m_TableHeaderNumber = m_TableHeaderNumber; }
            if (otherObject.m_RowHeaderNumber != m_RowHeaderNumber)
            { otherObject.m_RowHeaderNumber = m_RowHeaderNumber; }
            if (otherObject.m_ColumnHeaderNumber != m_ColumnHeaderNumber)
            { otherObject.m_ColumnHeaderNumber = m_ColumnHeaderNumber; }
            if (otherObject.m_DataAreaNumber != m_DataAreaNumber)
            { otherObject.m_DataAreaNumber = m_DataAreaNumber; }
            if (otherObject.m_CommonTitleContainerNumber != m_CommonTitleContainerNumber)
            { otherObject.m_CommonTitleContainerNumber = m_CommonTitleContainerNumber; }
            if (otherObject.m_VariableTitleContainerNumber != m_VariableTitleContainerNumber)
            { otherObject.m_VariableTitleContainerNumber = m_VariableTitleContainerNumber; }
            if (otherObject.m_VariableDataContainerNumber != m_VariableDataContainerNumber)
            { otherObject.m_VariableDataContainerNumber = m_VariableDataContainerNumber; }
        }

        public override bool Equals(DimensionalTable otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(DimensionalTable otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(DimensionalTable otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(DimensionalTable otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(DimensionalTable otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            if (otherObject.m_IsTransposed != m_IsTransposed)
            { return false; }

            if (otherObject.m_TableHeaderNumber != m_TableHeaderNumber)
            { return false; }
            if (otherObject.m_RowHeaderNumber != m_RowHeaderNumber)
            { return false; }
            if (otherObject.m_ColumnHeaderNumber != m_ColumnHeaderNumber)
            { return false; }
            if (otherObject.m_DataAreaNumber != m_DataAreaNumber)
            { return false; }
            if (otherObject.m_CommonTitleContainerNumber != m_CommonTitleContainerNumber)
            { return false; }
            if (otherObject.m_VariableTitleContainerNumber != m_VariableTitleContainerNumber)
            { return false; }
            if (otherObject.m_VariableDataContainerNumber != m_VariableDataContainerNumber)
            { return false; }

            return true;
        }
    }
}