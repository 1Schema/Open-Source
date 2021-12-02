using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum Dimension_DisplayMode
    {
        [Description("With Common Titles")]
        DisplayIn_CommonTitleContainer,
        [Description("With Variable Titles")]
        DisplayIn_VariableTitleContainer
    }
}