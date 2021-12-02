using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum BindingStageType
    {
        [Description("Design-Time")]
        DesignTime,
        [Description("Run-Time")]
        RunTime
    }
}