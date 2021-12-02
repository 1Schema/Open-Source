using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Testing
{
    public class EnumDimensionValue<T> : EnumDimensionValue
        where T : struct
    {
        public EnumDimensionValue(T? enumValue)
            : this(null, enumValue)
        { }

        public EnumDimensionValue(int? dimensionNumber, T? enumValue)
            : base(typeof(T), dimensionNumber, enumValue.HasValue ? (int)((object)enumValue.Value) : (int?)null)
        { }
    }

    public class EnumDimensionValue
    {
        public EnumDimensionValue(Type enumType, int? enumValue)
            : this(enumType, null, enumValue)
        { }

        public EnumDimensionValue(Type enumType, int? dimensionNumber, int? enumValue)
        {
            if (enumType == null)
            { throw new InvalidOperationException("The Enum Type cannot be null."); }
            if (!enumType.IsEnum)
            { throw new InvalidOperationException("The Enum Type must represent an Enum."); }

            EnumType = enumType;
            DimensionNumber = dimensionNumber.HasValue ? dimensionNumber.Value : ModelObjectReference.MinimumAlternateDimensionNumber;
            EnumValue = enumValue;
        }

        public Type EnumType { get; protected set; }
        public int DimensionNumber { get; protected set; }
        public int? EnumValue { get; protected set; }
        public bool IsNull { get { return !EnumValue.HasValue; } }
    }
}