using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Common.Conversion
{
    public static class SpecialCharacterConverter
    {
        public const bool UseAlternateValues_Default = false;

        public static readonly KeyValuePair<string, string> EscapeCode_Quote = new KeyValuePair<string, string>("\"", "&#34;");
        public static readonly KeyValuePair<string, string> EscapeCode_Quote_Alt = new KeyValuePair<string, string>("\"", "&quot;");
        public static readonly KeyValuePair<string, string> EscapeCode_Ampersand = new KeyValuePair<string, string>("&", "&#38;");
        public static readonly KeyValuePair<string, string> EscapeCode_Ampersand_Alt = new KeyValuePair<string, string>("&", "&amp;");
        public static readonly KeyValuePair<string, string> EscapeCode_Apostrophe = new KeyValuePair<string, string>("'", "&#39;");
        public static readonly KeyValuePair<string, string> EscapeCode_Apostrophe_Alt = new KeyValuePair<string, string>("'", "&apos;");
        public static readonly KeyValuePair<string, string> EscapeCode_Comma = new KeyValuePair<string, string>(",", "&#44;");
        public static readonly KeyValuePair<string, string> EscapeCode_Slash = new KeyValuePair<string, string>("/", "&#47;");
        public static readonly KeyValuePair<string, string> EscapeCode_Colon = new KeyValuePair<string, string>(":", "&#58;");
        public static readonly KeyValuePair<string, string> EscapeCode_SemiColon = new KeyValuePair<string, string>(";", "&#59;");
        public static readonly KeyValuePair<string, string> EscapeCode_BackSlash = new KeyValuePair<string, string>("\\", "&#92;");
        public static readonly KeyValuePair<string, string> EscapeCode_LessThan = new KeyValuePair<string, string>("<", "&#60;");
        public static readonly KeyValuePair<string, string> EscapeCode_LessThan_Alt = new KeyValuePair<string, string>("<", "&lt;");
        public static readonly KeyValuePair<string, string> EscapeCode_GreaterThan = new KeyValuePair<string, string>(">", "&#62;");
        public static readonly KeyValuePair<string, string> EscapeCode_GreaterThan_Alt = new KeyValuePair<string, string>(">", "&gt;");

        public static readonly ReadOnlyDictionary<string, string> EscapeCodes;
        public static readonly ReadOnlyDictionary<string, string> EscapeCodes_Alt;

        static SpecialCharacterConverter()
        {
            EscapeCodes = new ReadOnlyDictionary<string, string>();
            EscapeCodes.Add(EscapeCode_Quote.Key, EscapeCode_Quote.Value);
            EscapeCodes.Add(EscapeCode_Ampersand.Key, EscapeCode_Ampersand.Value);
            EscapeCodes.Add(EscapeCode_Apostrophe.Key, EscapeCode_Apostrophe.Value);
            EscapeCodes.Add(EscapeCode_Comma.Key, EscapeCode_Comma.Value);
            EscapeCodes.Add(EscapeCode_Slash.Key, EscapeCode_Slash.Value);
            EscapeCodes.Add(EscapeCode_Colon.Key, EscapeCode_Colon.Value);
            EscapeCodes.Add(EscapeCode_SemiColon.Key, EscapeCode_SemiColon.Value);
            EscapeCodes.Add(EscapeCode_BackSlash.Key, EscapeCode_BackSlash.Value);
            EscapeCodes.Add(EscapeCode_LessThan.Key, EscapeCode_LessThan.Value);
            EscapeCodes.Add(EscapeCode_GreaterThan.Key, EscapeCode_GreaterThan.Value);
            EscapeCodes.IsReadOnly = true;

            EscapeCodes_Alt = new ReadOnlyDictionary<string, string>();
            EscapeCodes_Alt.Add(EscapeCode_Quote_Alt.Key, EscapeCode_Quote_Alt.Value);
            EscapeCodes_Alt.Add(EscapeCode_Ampersand_Alt.Key, EscapeCode_Ampersand_Alt.Value);
            EscapeCodes_Alt.Add(EscapeCode_Apostrophe_Alt.Key, EscapeCode_Apostrophe_Alt.Value);
            EscapeCodes_Alt.Add(EscapeCode_Comma.Key, EscapeCode_Comma.Value);
            EscapeCodes_Alt.Add(EscapeCode_Slash.Key, EscapeCode_Slash.Value);
            EscapeCodes_Alt.Add(EscapeCode_Colon.Key, EscapeCode_Colon.Value);
            EscapeCodes_Alt.Add(EscapeCode_SemiColon.Key, EscapeCode_SemiColon.Value);
            EscapeCodes_Alt.Add(EscapeCode_BackSlash.Key, EscapeCode_BackSlash.Value);
            EscapeCodes_Alt.Add(EscapeCode_LessThan_Alt.Key, EscapeCode_LessThan_Alt.Value);
            EscapeCodes_Alt.Add(EscapeCode_GreaterThan_Alt.Key, EscapeCode_GreaterThan_Alt.Value);
            EscapeCodes_Alt.IsReadOnly = true;
        }

        public static string ApplyEscapeCodes(this string originalText)
        {
            return ApplyEscapeCodes(originalText, UseAlternateValues_Default);
        }

        public static string ApplyEscapeCodes(this string originalText, bool useAlternateValues)
        {
            if (string.IsNullOrEmpty(originalText))
            { return originalText; }

            var escapeCodes = (useAlternateValues) ? EscapeCodes_Alt : EscapeCodes;
            var currentText = string.Empty;

            foreach (var character in originalText.ToCharArray())
            {
                var charAsText = character.ToString();

                if (!escapeCodes.ContainsKey(charAsText))
                { currentText += character; }
                else
                { currentText += escapeCodes[charAsText]; }
            }
            return currentText;
        }

        public static string ApplyEscapeCodesIfNecessary(this string originalText)
        {
            return ApplyEscapeCodesIfNecessary(originalText, UseAlternateValues_Default);
        }

        public static string ApplyEscapeCodesIfNecessary(this string originalText, bool useAlternateValues)
        {
            if (UserState.CurrentThreadState == null)
            { return originalText; }
            if (!UserState.CurrentThreadState.ApplyEscapeChars)
            { return originalText; }
            return ApplyEscapeCodes(originalText, useAlternateValues);
        }
    }
}