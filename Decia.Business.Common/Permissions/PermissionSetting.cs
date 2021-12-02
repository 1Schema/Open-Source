using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum PermissionSetting
    {
        [Description("Allow")]
        Allow,
        [Description("Deny")]
        Deny,
        [Description("Floating")]
        Floating,
    }

    public static class PermissionSettingUtils
    {
        public static Nullable<bool> ConvertToBool(this PermissionSetting setting)
        {
            if (setting == PermissionSetting.Floating)
            { return null; }
            else if (setting == PermissionSetting.Deny)
            { return false; }
            else if (setting == PermissionSetting.Allow)
            { return true; }
            else
            { throw new InvalidOperationException("Unrecognized PermissionSetting encountered."); }
        }

        public static PermissionSetting CombinePermission(this PermissionSetting firstSetting, PermissionSetting secondSetting)
        {
            if ((firstSetting == PermissionSetting.Allow) || (secondSetting == PermissionSetting.Allow))
            { return PermissionSetting.Allow; }
            else if ((firstSetting == PermissionSetting.Deny) || (secondSetting == PermissionSetting.Deny))
            { return PermissionSetting.Deny; }
            else
            { return PermissionSetting.Floating; }
        }

        public static PermissionSetting FinalizePermission(this PermissionSetting setting)
        {
            if (setting == PermissionSetting.Floating)
            { return PermissionSetting.Deny; }
            return setting;
        }

        public static Dictionary<K, PermissionSetting> CombinePermissions<K>(this IDictionary<K, PermissionSetting> firstSettings, IDictionary<K, PermissionSetting> secondSettings)
            where K : struct
        {
            var allKeys = firstSettings.Keys.Union(secondSettings.Keys).Distinct().ToList();
            var result = new Dictionary<K, PermissionSetting>();

            foreach (var key in allKeys)
            {
                var firstValue = firstSettings.ContainsKey(key) ? firstSettings[key] : PermissionSetting.Floating;
                var secondValue = secondSettings.ContainsKey(key) ? secondSettings[key] : PermissionSetting.Floating;
                var combinedValue = firstValue.CombinePermission(secondValue);
                result.Add(key, combinedValue);
            }
            return result;
        }

        public static Dictionary<K, PermissionSetting> FinalizePermissions<K>(this IDictionary<K, PermissionSetting> settings)
            where K : struct
        {
            var result = new Dictionary<K, PermissionSetting>();

            foreach (var key in settings.Keys)
            {
                var originalValue = settings[key];
                var finalizedValue = originalValue.FinalizePermission();
                result.Add(key, finalizedValue);
            }
            return result;
        }
    }
}