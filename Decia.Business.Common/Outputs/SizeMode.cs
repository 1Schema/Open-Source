using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum SizeMode
    {
        Cell,
        Ratio,
        Auto
    }

    public enum SizePrecedence
    {
        Self,
        Parent,
        Children
    }

    public static class SizeModeUtils
    {
        public static SizePrecedence GetSizePrecedence(this SizeMode sizeMode)
        {
            if (sizeMode == SizeMode.Cell)
            { return SizePrecedence.Self; }
            else if (sizeMode == SizeMode.Ratio)
            { return SizePrecedence.Parent; }
            else if (sizeMode == SizeMode.Auto)
            { return SizePrecedence.Children; }
            else
            { throw new InvalidOperationException("Invalid SizeMode encountered."); }
        }
    }
}