using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum RepeatMode
    {
        [Description("Show with FIRST occurrence")]
        StartOfIterationBlock,
        [Description("Show with LAST occurrence")]
        EndOfIterationBlock,
        [Description("Repeat with ALL occurrences")]
        EveryIteration
    }

    public static class RepeatModeUtils
    {
        public const RepeatMode DefaultRepeatMode = RepeatMode.EndOfIterationBlock;
    }
}