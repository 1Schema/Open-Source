using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum Dimension
    {
        None = 0,
        X = 1,
        Y = 2,
        Z1 = 4,
        Z2 = 8,
    }

    public static class DimensionUtils
    {
        public static readonly IEnumerable<Dimension> ValidDimensions_2D = new Dimension[] { Dimension.X, Dimension.Y };

        public static bool GetIsValidDimension_2D(this Dimension dimension)
        {
            return ValidDimensions_2D.Contains(dimension);
        }

        public static void AssertIsValidDimension_2D(this Dimension dimension)
        {
            if (!GetIsValidDimension_2D(dimension))
            { throw new InvalidOperationException("The specified Dimension is not usable in 2 Dimensions."); }
        }

        public static Dimension Invert_2D(this Dimension dimension)
        {
            AssertIsValidDimension_2D(dimension);

            if (dimension == Dimension.X)
            { return Dimension.Y; }
            else if (dimension == Dimension.Y)
            { return Dimension.X; }
            else
            { throw new InvalidOperationException("Unexpected error condition encountered."); }
        }

        public static int GetDimensionNumber(this Dimension dimension)
        {
            var names = Enum.GetNames(typeof(Dimension));
            int minDimensionNumber = (int)Dimension.None;

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == dimension.ToString())
                { return (i + minDimensionNumber); }
            }
            throw new InvalidOperationException("Invalid Dimension specified.");
        }
    }
}