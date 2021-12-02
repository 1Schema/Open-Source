using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Presentation
{
    public enum DateFormattingType
    {
        [Description("Month-Year")]
        Month_Year,
        [Description("Year-Month")]
        Year_Month,
        [Description("Custom")]
        Custom
    }
}