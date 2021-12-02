using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum Dimension_RepeatMode
    {
        [Description("Repeat For All Cells")]
        RepeatForAllCells,
        [Description("Show For First Cell")]
        ShowForFirstCell,
        [Description("Show For Last Cell")]
        ShowForLastCell,
    }
}