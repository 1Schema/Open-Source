using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public static class NamingUtils
    {
        public const string DefaultNameFormat = "New {0}";

        public static string GetDefaultNameForType<T>()
        {
            return GetDefaultNameForType<T>(DefaultNameFormat);
        }

        public static string GetDefaultNameForType<T>(string defaultNameFormat)
        {
            return GetDefaultNameForType(typeof(T), defaultNameFormat);
        }

        public static string GetDefaultNameForType(this Type type)
        {
            return string.Format(DefaultNameFormat, type.Name);
        }

        public static string GetDefaultNameForType(this Type type, string defaultNameFormat)
        {
            return string.Format(defaultNameFormat, type.Name);
        }

        public static string GetUniqueNameForType(this IEnumerable<string> existingNames, Type type)
        {
            var nameBase = GetDefaultNameForType(type);
            return GetUniqueName(existingNames, nameBase);
        }

        public static string GetUniqueName(this IEnumerable<string> existingNames, string nameBase)
        {
            bool alreadyHasName = false;
            int postFix = 2;
            string uniqueName = nameBase;

            var existingNames_Ordered = existingNames.OrderBy(x => x).ToList();

            for (int i = 0; i < existingNames_Ordered.Count; i++)
            {
                var existingName = (existingNames_Ordered[i] != null) ? existingNames_Ordered[i] : string.Empty;

                if (existingName.ToLower() != uniqueName.ToLower())
                {
                    if (!alreadyHasName)
                    { continue; }
                    else if (!existingName.ToLower().Contains(nameBase.ToLower()))
                    { break; }
                }

                alreadyHasName = true;
                uniqueName = string.Format("{0} {1}", nameBase, postFix);
                postFix++;
            }
            return uniqueName;
        }
    }
}