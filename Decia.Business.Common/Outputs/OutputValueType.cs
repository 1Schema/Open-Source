using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum OutputValueType
    {
        [Description("Text")]
        DirectValue,
        [Description("Reference")]
        ReferencedId,
        [Description("Formula")]
        AnonymousFormula
    }
}