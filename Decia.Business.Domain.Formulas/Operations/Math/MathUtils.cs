using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Domain.ChronometricValues;

namespace Decia.Business.Domain.Formulas.Operations.Math
{
    public static class MathUtils
    {
        public const string MathCategoryName = "Basic Math";
        public const int TimeSpan_Ticks_to_Seconds_Divisor = (1000 * 1000 * 10);
        public const int TimeSpan_Seconds_to_Days_Divisor = (24 * 60 * 60);
        public const long TimeSpan_Ticks_to_Days_Divisor = ((long)TimeSpan_Ticks_to_Seconds_Divisor * (long)TimeSpan_Seconds_to_Days_Divisor);
    }
}