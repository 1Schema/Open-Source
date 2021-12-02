using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Styling
{
    public enum DeciaFontFamily
    {
        /* ADD FONT FAMILIES*/
    }

    public enum DeciaFontCategory
    {
        SanSerif,
        Serif,
        Monospace,
    }

    public static class DeciaFontFamilyUtils
    {
        public static readonly Func<string, string> NameSimplifier = ((string s) => s.Replace("\"", ""));
        public static readonly IEnumerable<DeciaFontFamily> Fonts_Serif = new DeciaFontFamily[] { /* ADD FONT FAMILIES*/ };
        public static readonly IEnumerable<DeciaFontFamily> Fonts_SansSerif = new DeciaFontFamily[] { /* ADD FONT FAMILIES*/ };
        public static readonly IEnumerable<DeciaFontFamily> Fonts_Monospace = new DeciaFontFamily[] { /* ADD FONT FAMILIES*/ };
        public static readonly Dictionary<DeciaFontFamily, string> FontFamily_Names = new Dictionary<DeciaFontFamily, string>() { /* ADD FONT FAMILIES*/ };
        public static Dictionary<string, DeciaFontFamily> FontName_Families { get { return FontFamily_Names.ToDictionary(x => x.Value, x => x.Key); } }
        public static Dictionary<string, DeciaFontFamily> FontName_Families_Simplified { get { return FontFamily_Names.ToDictionary(x => NameSimplifier(x.Value), x => x.Key); } }


        public static string GetFontName(this DeciaFontFamily fontFamily)
        {
            if (FontFamily_Names.ContainsKey(fontFamily))
            { return FontFamily_Names[fontFamily]; }
            else
            { return string.Empty; }
        }

        public static string GetFontName_Simplified(this DeciaFontFamily fontFamily)
        {
            var fontName = GetFontName(fontFamily);
            return NameSimplifier(fontName);
        }

        public static DeciaFontFamily? GetFontFamily(this string fontName)
        {
            var fontName_Families = FontName_Families;

            if (fontName_Families.ContainsKey(fontName))
            { return fontName_Families[fontName]; }
            else
            { return null; }
        }

        public static DeciaFontFamily? GetFontFamily_Simplified(this string fontName_Simplified)
        {
            var fontName_Families = FontName_Families_Simplified;

            if (fontName_Families.ContainsKey(fontName_Simplified))
            { return fontName_Families[fontName_Simplified]; }
            else
            { return null; }
        }

        public static DeciaFontCategory GetFontCategory(this DeciaFontFamily fontFamily)
        {
            if (Fonts_Serif.Contains(fontFamily))
            { return DeciaFontCategory.Serif; }
            else if (Fonts_SansSerif.Contains(fontFamily))
            { return DeciaFontCategory.SanSerif; }
            else if (Fonts_Monospace.Contains(fontFamily))
            { return DeciaFontCategory.Monospace; }
            else
            { throw new InvalidOperationException("Unrecognized FontFamily encountered."); }
        }

        public static ICollection<DeciaFontFamily> GetFontsForCategory(this DeciaFontCategory fontCategory)
        {
            if (fontCategory == DeciaFontCategory.Serif)
            { return Fonts_Serif.ToList(); }
            else if (fontCategory == DeciaFontCategory.SanSerif)
            { return Fonts_SansSerif.ToList(); }
            else if (fontCategory == DeciaFontCategory.Monospace)
            { return Fonts_Monospace.ToList(); }
            else
            { throw new InvalidOperationException("Unrecognized FontCategory encountered."); }
        }
    }
}