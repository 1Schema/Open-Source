using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class Cell : IValueObject<Cell>
    {
        public static bool Default_CheckDefaultValues_Setting = true;

        public static IEqualityComparer<Cell> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<Cell>(); }
        }

        IEqualityComparer<Cell> IValueObject<Cell>.ValueWiseComparer
        {
            get { return Cell.ValueWiseComparer; }
        }

        public override Cell Copy()
        {
            Cell otherObject = new Cell();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual Cell CopyNew()
        {
            Cell otherObject = new Cell();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(Cell otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyTo(Cell otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual void CopyValuesTo(Cell otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual void CopyValuesTo(Cell otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            this.CopyValuesTo(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual void CopyValuesTo(Cell otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            this.CopyValuesToBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public override bool Equals(Cell otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool Equals(Cell otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = EqualityComparer<SaveableDimensionLayout>.Default;
            var elementStyleComparer = EqualityComparer<SaveableElementStyle>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        public virtual bool EqualsValues(Cell otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultValues_Setting);
        }

        public virtual bool EqualsValues(Cell otherObject, bool checkDefaultValues)
        {
            var dimensionLayoutComparer = SaveableDimensionLayout.ValueWiseComparer;
            var elementStyleComparer = SaveableElementStyle.ValueWiseComparer;

            return this.EqualsValues(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer);
        }

        protected virtual bool EqualsValues(Cell otherObject, bool checkDefaultValues, IEqualityComparer<SaveableDimensionLayout> dimensionLayoutComparer, IEqualityComparer<SaveableElementStyle> elementStyleComparer)
        {
            if (!this.EqualsValuesBase_VRB(otherObject, checkDefaultValues, dimensionLayoutComparer, elementStyleComparer))
            { return false; }

            return true;
        }
    }
}