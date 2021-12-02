using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Structure
{
    public enum StructuralDimensionType
    {
        NotSet = 0x0000,
        ReferenceMember = 0x0001,
        ExistenceMember = 0x0010,
        KeyMember = 0x0100
    }

    public static class StructuralDimensionTypeUtils
    {
        public const StructuralDimensionType DefaultDimensionType = StructuralDimensionType.ExistenceMember;

        public static IList<T> SortByDimensionType<T>(this IEnumerable<T> unsorted, Func<T, StructuralDimensionType> dimensionTypeGetter)
        {
            return unsorted.OrderBy(t => (-1 * ((int)dimensionTypeGetter(t)))).ToList();
        }

        public static bool IsReferenceMember(this StructuralDimensionType dimensionType)
        {
            return (dimensionType == (dimensionType | StructuralDimensionType.ReferenceMember));
        }

        public static bool IsReferenceMember(this StructuralDimension dimension)
        {
            return dimension.DimensionType.IsReferenceMember();
        }

        public static bool IsExistenceMember(this StructuralDimensionType dimensionType)
        {
            return (dimensionType == (dimensionType | StructuralDimensionType.ExistenceMember));
        }

        public static bool IsExistenceMember(this StructuralDimension dimension)
        {
            return dimension.DimensionType.IsExistenceMember();
        }

        public static bool IsKeyMember(this StructuralDimensionType dimensionType)
        {
            return (dimensionType == (dimensionType | StructuralDimensionType.KeyMember));
        }

        public static bool IsKeyMember(this StructuralDimension dimension)
        {
            return dimension.DimensionType.IsKeyMember();
        }
    }
}