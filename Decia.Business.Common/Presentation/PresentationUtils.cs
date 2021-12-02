using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Presentation
{
    public static class PresentationUtils
    {
        public const string NonBreakingSpace = "&nbsp;";
        public const string IndentValue = NonBreakingSpace + NonBreakingSpace + NonBreakingSpace + NonBreakingSpace;

        public static readonly Color Default_Normal_BackColor = Color.White;
        public static readonly Color Default_Normal_ForeColor = Color.Black;
        public static readonly Color Default_ReadOnly_BackColor = ColorTranslator.FromHtml("#DCDCDC");
        public static readonly Color Default_Duplicate_BackColor = ColorTranslator.FromHtml("#666666");
        public static readonly Color Default_Duplicate_ForeColor = ColorTranslator.FromHtml("#666666");

        public static Nullable<T> GetNextSelectedItem<T>(this IEnumerable<T> values, T currentSelection)
            where T : struct
        {
            Nullable<int> nextSelectedIndex = GetNextSelectedIndex(values, currentSelection);
            if (!nextSelectedIndex.HasValue)
            { return null; }
            return values.ElementAt(nextSelectedIndex.Value);
        }

        public static Nullable<int> GetNextSelectedIndex<T>(this IEnumerable<T> values, T currentSelection)
        {
            Nullable<int> currSelectedIndex = null;
            Nullable<int> nextSelectedIndex = null;
            int count = values.Count();

            if (count < 1)
            { return null; }

            for (int i = 0; i < count; i++)
            {
                if (values.ElementAt(i).Equals(currentSelection))
                {
                    currSelectedIndex = i;
                    break;
                }
            }

            if ((count < 2) && (currSelectedIndex.HasValue))
            { return null; }

            if (!currSelectedIndex.HasValue)
            { return 0; }

            if (currSelectedIndex.Value >= (count - 1))
            { return (currSelectedIndex.Value - 1); }

            return currSelectedIndex.Value;
        }
    }
}