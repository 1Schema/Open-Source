using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public static class OutputUtils
    {
        public const string DynamicStructuralPositionMarker = "@StructuralName";
        public const string DynamicTimePositionMarker = "@TimeName";

        public static string GetFullTextValue(this string baseTextValue, string structuralName, string timeName, StructuralNamePositionType positionType)
        {
            if (positionType == StructuralNamePositionType.DontShow)
            { return baseTextValue; }
            else if (positionType == StructuralNamePositionType.Prepend)
            {
                string fullTextValue = structuralName;
                if (!string.IsNullOrWhiteSpace(timeName))
                { fullTextValue += ", " + timeName; }

                if (!baseTextValue.StartsWith(" "))
                { fullTextValue += " " + baseTextValue; }
                else
                { fullTextValue += baseTextValue; }

                return fullTextValue;
            }
            else if (positionType == StructuralNamePositionType.Append)
            {
                string fullTextValue = structuralName;
                if (!string.IsNullOrWhiteSpace(timeName))
                { fullTextValue += ", " + timeName; }

                if (!baseTextValue.EndsWith(" "))
                { fullTextValue = baseTextValue + " " + fullTextValue; }
                else
                { fullTextValue = baseTextValue + fullTextValue; }

                return fullTextValue;
            }
            else if (positionType == StructuralNamePositionType.Dynamic)
            {
                string fullTextValue = InsertStructuralNameAtMarker(baseTextValue, structuralName);
                fullTextValue = InsertTimeNameAtMarker(fullTextValue, timeName);
                return fullTextValue;
            }
            else
            { throw new InvalidOperationException("Unrecognized StructuralNamePositionType encountered."); }
        }

        private static string InsertStructuralNameAtMarker(string baseTextValue, string structuralName)
        {
            string[] parts = baseTextValue.Split(new string[] { DynamicStructuralPositionMarker }, StringSplitOptions.None);
            string fullTextValue = null;
            foreach (string part in parts)
            {
                if (fullTextValue == null)
                { fullTextValue += part; }
                else
                { fullTextValue += structuralName + part; }
            }
            return fullTextValue;
        }

        private static string InsertTimeNameAtMarker(string baseTextValue, string timeName)
        {
            string[] parts = baseTextValue.Split(new string[] { DynamicTimePositionMarker }, StringSplitOptions.None);
            string fullTextValue = null;
            foreach (string part in parts)
            {
                if (fullTextValue == null)
                { fullTextValue += part; }
                else
                { fullTextValue += timeName + part; }
            }
            return fullTextValue;
        }
    }
}