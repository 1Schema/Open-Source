using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum StructuralNamePositionType
    {
        [Description("Do not show {0}")]
        DontShow,
        [Description("Prepend {0}")]
        Prepend,
        [Description("Append {0}")]
        Append,
        [Description("Specify exact position of {0}")]
        Dynamic
    }
}