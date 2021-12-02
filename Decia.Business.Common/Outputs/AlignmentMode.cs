using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum AlignmentMode
    {
        Lesser,
        Center,
        Stretch,
        Greater,
    }

    public static class AlignmentModeUtils
    {
        public const string X_Lesser_Name = "Left";
        public const string X_Greater_Name = "Right";
        public const string Y_Lesser_Name = "Top";
        public const string Y_Greater_Name = "Bottom";

        public static Dictionary<string, string> GetOptionsList(this Dimension dimension)
        {
            var options = EnumUtils.GetEnumValuesAsSelectOptions<AlignmentMode>();
            if (dimension == Dimension.X)
            {
                options[AlignmentMode.Lesser.ToString()] = X_Lesser_Name;
                options[AlignmentMode.Greater.ToString()] = X_Greater_Name;
            }
            else if (dimension == Dimension.Y)
            {
                options[AlignmentMode.Lesser.ToString()] = Y_Lesser_Name;
                options[AlignmentMode.Greater.ToString()] = Y_Greater_Name;
            }
            else
            { throw new InvalidOperationException("The specified Dimension is not supported."); }
            return options;
        }
    }
}