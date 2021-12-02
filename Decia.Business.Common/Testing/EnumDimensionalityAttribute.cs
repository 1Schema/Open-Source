using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Testing
{
    public class EnumDimensionalityAttribute : Attribute
    {
        private ReadOnlyList<EnumDimensionValue> m_Dimensionality;

        public EnumDimensionalityAttribute()
            : this(new EnumDimensionValue[] { })
        { }

        public EnumDimensionalityAttribute(Type dimensionEnumType, int? dimensionNumber, int? dimensionEnumValue)
            : this(new EnumDimensionValue[] { new EnumDimensionValue(dimensionEnumType, dimensionNumber, dimensionEnumValue) })
        { }

        public EnumDimensionalityAttribute(IEnumerable<EnumDimensionValue> dimensionality)
        {
            LoadDimensionValues(dimensionality);
        }

        public EnumDimensionalityAttribute(IEnumerable<Type> enumTypes, IEnumerable<int?> enumValues)
            : this(enumTypes, enumValues, new List<int?>())
        { }

        public EnumDimensionalityAttribute(IEnumerable<Type> enumTypes, IEnumerable<int?> enumValues, IEnumerable<int?> dimensionNumbers)
        {
            var dimensionality = new List<EnumDimensionValue>();

            for (int i = 0; i < enumTypes.Count(); i++)
            {
                var enumType = enumTypes.ElementAt(i);
                var enumValue = enumValues.ElementAt(i);
                var dimensionNumber = (i < dimensionNumbers.Count()) ? dimensionNumbers.ElementAt(i) : (int?)null;
            }
            LoadDimensionValues(dimensionality);
        }

        private void LoadDimensionValues(IEnumerable<EnumDimensionValue> dimensionality)
        {
            var tempDimensionality = new List<EnumDimensionValue>();
            var lastDimensionValue = (EnumDimensionValue)null;

            foreach (var dimensionValue in dimensionality.OrderBy(x => x.EnumType.Name + ", " + x.DimensionNumber))
            {
                if ((lastDimensionValue != null) && ((lastDimensionValue.EnumType == dimensionValue.EnumType) && (lastDimensionValue.DimensionNumber == dimensionValue.DimensionNumber)))
                { throw new InvalidOperationException("Duplicate Dimension encountered."); }

                tempDimensionality.Add(dimensionValue);
                lastDimensionValue = dimensionValue;
            }

            m_Dimensionality = new ReadOnlyList<EnumDimensionValue>(tempDimensionality);
            m_Dimensionality.IsReadOnly = true;
        }

        public ICollection<EnumDimensionValue> Dimensionality
        {
            get { return m_Dimensionality; }
        }
    }
}