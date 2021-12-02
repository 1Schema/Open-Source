using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.TypedIds
{
    public enum KeyProcessingMode
    {
        Optimize_Debugging,
        Optimize_Performance
    }

    public static class KeyProcessingModeUtils
    {
        public const KeyProcessingMode CurrentMode = KeyProcessingMode.Optimize_Performance;
        public const bool ShowDebugInfo = (CurrentMode == KeyProcessingMode.Optimize_Debugging);

        public static KeyProcessingMode GetCurrentMode<T>(this T key)
            where T : struct
        { return CurrentMode; }

        public static bool OptimizeDebugging()
        { return (CurrentMode == KeyProcessingMode.Optimize_Debugging); }

        public static bool OptimizeDebugging<T>(this T key)
            where T : struct
        { return OptimizeDebugging(); }

        public static bool OptimizePerformance()
        { return (CurrentMode == KeyProcessingMode.Optimize_Performance); }

        public static bool OptimizePerformance<T>(this T key)
            where T : struct
        { return OptimizePerformance(); }

        #region GetModelText Methods

        public const string Default_ReleaseTextValue = "";

        public static string GetModalDebugText(this string debugTextValue)
        {
            return GetModalDebugText(debugTextValue, Default_ReleaseTextValue);
        }

        public static string GetModalDebugText(this string debugTextValue, string nonDebugTextValue)
        {
            if (!KeyProcessingModeUtils.ShowDebugInfo)
            { return nonDebugTextValue; }
            return debugTextValue;
        }

        #endregion
    }
}