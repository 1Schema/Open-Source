using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Formulas
{
    public static class FormulaRenderingUtils
    {
        public static readonly string NullValueAsString = SqlDb_TargetTypeUtils.NullValueAsSql;

        public static string RenderArgumentsAsString(string prefix, string inFix, string postFix, SortedDictionary<int, string> argsAsStrings)
        {
            return RenderArgumentsAsString(prefix, inFix, inFix, postFix, argsAsStrings);
        }

        public static string RenderArgumentsAsString(string prefix, string firstInFix, string subsequentInFix, string postFix, SortedDictionary<int, string> argsAsStrings)
        {
            if (argsAsStrings.Count < 1)
            { return prefix + postFix; }

            int minArgIndex = argsAsStrings.Keys.Min();
            int maxArgIndex = argsAsStrings.Keys.Max();
            string result = string.Empty;
            bool isFirstInfix = true;

            for (int index = minArgIndex; index <= maxArgIndex; index++)
            {
                string argAsString = argsAsStrings.ContainsKey(index) ? argsAsStrings[index] : NullValueAsString;

                if (index == minArgIndex)
                {
                    result += prefix + argAsString;
                }
                else if (index <= maxArgIndex)
                {
                    result += (isFirstInfix ? firstInFix : subsequentInFix) + argAsString;
                    isFirstInfix = false;
                }

                if (index == maxArgIndex)
                {
                    result += postFix;
                }
            }
            return result;
        }
    }
}