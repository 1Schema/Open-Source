using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Testing
{
    public static class EnumDimensionValueUtils
    {
        public static EnumDimensionValue ToDimensionValue<T>(this T enumValue, int? dimensionNumber)
           where T : struct
        { return new EnumDimensionValue<T>(dimensionNumber, enumValue); }
    }
}