using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Styling
{
    public static class StylingAssertions
    {
        public static bool AreDimensionsValid(int width, int height)
        {
            if ((width <= 0) || (height <= 0))
            { return false; }
            return true;
        }

        public static void AssertDimensionsAreValid(int width, int height)
        {
            if (!AreDimensionsValid(width, height))
            { throw new InvalidOperationException("The Width and Height must be greater than zero."); }
        }
    }
}