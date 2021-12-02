using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Formulas.Parsing
{
    public static class ParsingUtils
    {
        public static bool IsReference(this string value)
        {
            return IsSymbolRef(value, SymbolTable.Symbol_ForRefs);
        }

        public static bool IsValue(this string value)
        {
            return IsSymbolRef(value, SymbolTable.Symbol_ForValues);
        }

        private static bool IsSymbolRef(string value, char refChar)
        {
            var value_Trimmed = value.Trim();

            if (string.IsNullOrWhiteSpace(value_Trimmed))
            { return false; }
            if (value_Trimmed.First() != refChar)
            { return false; }

            for (int i = 1; i < value_Trimmed.Length; i++)
            {
                var currChar = value_Trimmed[i];

                var isFirst = (i == 1);
                var isLast = (i == (value_Trimmed.Length - 1));
                var isMiddle = (!isFirst && !isLast);

                if (isFirst && (currChar != '('))
                { return false; }
                else if (isLast && (currChar != ')'))
                { return false; }
                else if (isMiddle && !(char.IsDigit(currChar) || (currChar == '-')))
                { return false; }
            }
            return true;
        }

        public static void SkipNestedText(this string remainingFormulaText, ref int i)
        {
            int nestingCount = 1;

            while (nestingCount > 0)
            {
                i++;

                if (i >= remainingFormulaText.Length)
                { return; }

                var nextStr = remainingFormulaText.Substring(i, 1);

                if (FormulaParser.CloseMarkers.Contains(nextStr))
                { nestingCount--; }
                else if (FormulaParser.OpenMarkers.Contains(nextStr))
                { nestingCount++; }

                if (nestingCount < 1)
                { return; }
            }
        }

        public static string TrimOpenAndCloseChars(this string originalText)
        {
            var trimmedText = originalText;

            foreach (var openChar in FormulaParser.OpenMarkers)
            { trimmedText = trimmedText.Replace(openChar.ToString(), ""); }

            foreach (var closeChar in FormulaParser.CloseMarkers)
            { trimmedText = trimmedText.Replace(closeChar.ToString(), ""); }

            return trimmedText;
        }
    }
}