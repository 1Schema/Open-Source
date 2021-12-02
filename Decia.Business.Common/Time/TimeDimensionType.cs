using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Time
{
    public enum TimeDimensionType
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        PrimaryAndSecondary = Primary | Secondary
    }

    public static class TimeDimensionTypeUtils
    {
        public static readonly IEnumerable<TimeDimensionType> DirectlyUsableTimeDimensionTypes = new TimeDimensionType[] { TimeDimensionType.Primary, TimeDimensionType.Secondary };

        public const int MinimumTimeDimensionNumber = 1;
        public const int MaximumTimeDimensionNumber = 2;
        public const int MinimumTimeDimensionCount = 0;
        public const int MaximumTimeDimensionCount = MaximumTimeDimensionNumber;

        public const bool Default_ThrowExceptionOnError = true;
        public static readonly Color Default_Primary_Color = Color.Yellow;
        public static readonly Color Default_Secondary_Color = Color.LightYellow;

        public static int GetTimeDimensionNumberForType(this TimeDimensionType timeDimensionType)
        {
            return GetTimeDimensionNumberForType(timeDimensionType, Default_ThrowExceptionOnError);
        }

        public static int GetTimeDimensionNumberForType(this TimeDimensionType timeDimensionType, bool throwExceptionOnError)
        {
            if (timeDimensionType == TimeDimensionType.Primary)
            { return 1; }
            else if (timeDimensionType == TimeDimensionType.Secondary)
            { return 2; }
            else
            {
                if (throwExceptionOnError)
                { throw new InvalidOperationException("The specified TimeDimensionType is not supported."); }
                else
                { return 0; }
            }
        }

        public static TimeDimensionType GetTimeDimensionTypeForNumber(this int timeDimensionNumber)
        {
            return GetTimeDimensionTypeForNumber(timeDimensionNumber, Default_ThrowExceptionOnError);
        }

        public static TimeDimensionType GetTimeDimensionTypeForNumber(this int timeDimensionNumber, bool throwExceptionOnError)
        {
            if (timeDimensionNumber == 1)
            { return TimeDimensionType.Primary; }
            else if (timeDimensionNumber == 2)
            { return TimeDimensionType.Secondary; }
            else
            {
                if (throwExceptionOnError)
                { throw new InvalidOperationException("The specified TimeDimension number is not supported."); }
                else
                { return TimeDimensionType.None; }
            }
        }

        public static bool TryAssertTimeDimensionTypeIsDirectlyUsable(this TimeDimensionType timeDimensionType)
        {
            if (!DirectlyUsableTimeDimensionTypes.Contains(timeDimensionType))
            { return false; }
            return true;
        }

        public static bool TryAssertTimeDimensionTypesAreDirectlyUsable(this IEnumerable<TimeDimensionType> timeDimensionTypes)
        {
            foreach (var timeDimensionType in timeDimensionTypes)
            {
                if (!TryAssertTimeDimensionTypeIsDirectlyUsable(timeDimensionType))
                { return false; }
            }
            return true;
        }

        public static void AssertTimeDimensionTypeIsDirectlyUsable(this TimeDimensionType timeDimensionType)
        {
            if (!TryAssertTimeDimensionTypeIsDirectlyUsable(timeDimensionType))
            { throw new InvalidOperationException("The specified TimeDimensionType is invalid."); }
        }

        public static void AssertTimeDimensionTypesAreDirectlyUsable(this IEnumerable<TimeDimensionType> timeDimensionTypes)
        {
            foreach (var timeDimensionType in timeDimensionTypes)
            { AssertTimeDimensionTypeIsDirectlyUsable(timeDimensionType); }
        }

        public static Color GetDefaultColor(this TimeDimensionType timeDimensionType)
        {
            if (timeDimensionType == TimeDimensionType.Primary)
            { return Default_Primary_Color; }
            else if (timeDimensionType == TimeDimensionType.Secondary)
            { return Default_Secondary_Color; }
            else
            { return Color.Wheat; }
        }
    }
}