using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public static class ExportUtils
    {
        public static string ToPluralized(this string name, bool isRelation)
        {
            var lastChar = name.ToLower().Last();

            if (isRelation)
            { return (name + " Sets"); }

            if (lastChar == 's')
            { return (name + "es"); }
            else if (lastChar == 'y')
            { return name.Substring(0, name.Length - 1) + "ies"; }
            else
            { return (name + "s"); }
        }

        public static string ToEscaped_VarName(this string name)
        {
            name = name.Replace("D-", "D");
            name = name.Replace("--", "_");
            name = name.Replace("<->", "___").Replace("<-", "__").Replace("->", "__");
            name = name.Replace("<", "_").Replace(">", "_");

            name = name.Replace(" ", "_").Replace(".", "_").Replace(",", "_");
            name = name.Replace("(", string.Empty).Replace(")", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            name = name.Replace(":", string.Empty).Replace(";", string.Empty);
            name = name.Replace("+", "Plus").Replace("-", "Minus").Replace("*", "Times").Replace("/", "Divides").Replace("%", "Remainders");
            return name;
        }

        public static string UpdateDeciaPrefix(this string text)
        {
            var updatedText = text.Replace("Decia_", "1Schema_");
            return updatedText;
        }

        #region SaveTextToFile Methods

        public static void SaveTextToFile(this string text, string fileName)
        {
            var directoryPath = Directory.GetCurrentDirectory();
            SaveTextToFile(text, directoryPath, fileName);
        }

        public static void SaveTextToFile(this string text, string directoryPath, string fileName)
        {
            var textEncoding = Encoding.UTF8;
            SaveTextToFile(text, directoryPath, fileName, textEncoding);
        }

        public static void SaveTextToFile(this string text, string directoryPath, string fileName, Encoding textEncoding)
        {
            var filePath = Path.Combine(directoryPath, fileName);

            using (var outputWriter = new StreamWriter(filePath, false, textEncoding))
            { outputWriter.Write(text); }
        }

        #endregion
    }
}