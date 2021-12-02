using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Models;
using Decia.Business.Domain.Time;
using ITimeDimensionality = System.Collections.Generic.IDictionary<Decia.Business.Common.Time.TimeDimensionType, System.Collections.Generic.KeyValuePair<Decia.Business.Common.Time.TimeValueType, Decia.Business.Common.Time.TimePeriodType>>;
using TimeDimensionality = System.Collections.Generic.SortedDictionary<Decia.Business.Common.Time.TimeDimensionType, System.Collections.Generic.KeyValuePair<Decia.Business.Common.Time.TimeValueType, Decia.Business.Common.Time.TimePeriodType>>;

namespace Decia.Business.Domain.Exports
{
    public static class NoSql_ExportUtils
    {
        public static ITimeDimensionality GetTimeDimensionality(this VariableTemplate variableTemplate)
        {
            var timeDimensionality = new TimeDimensionality();

            if (variableTemplate.TimeDimension_Count >= 1)
            {
                timeDimensionality.Add(TimeDimensionType.Primary, new KeyValuePair<TimeValueType, TimePeriodType>(variableTemplate.Specific_TimeValueType_Primary, variableTemplate.Specific_TimePeriodType_Primary));
            }

            if (variableTemplate.TimeDimension_Count >= 2)
            {
                timeDimensionality.Add(TimeDimensionType.Secondary, new KeyValuePair<TimeValueType, TimePeriodType>(variableTemplate.Specific_TimeValueType_Secondary, variableTemplate.Specific_TimePeriodType_Secondary));
            }

            return timeDimensionality;
        }

        public static ITimeDimensionality GetTimeDimensionality(this ITimeDimensionSet timeDimensionSet)
        {
            var timeDimensionality = new TimeDimensionality();

            if (timeDimensionSet.UsedDimensionCount >= 1)
            {
                timeDimensionality.Add(TimeDimensionType.Primary, new KeyValuePair<TimeValueType, TimePeriodType>(timeDimensionSet.PrimaryTimeDimension.TimeValueType, timeDimensionSet.PrimaryTimeDimension.TimePeriodType));
            }

            if (timeDimensionSet.UsedDimensionCount >= 2)
            {
                timeDimensionality.Add(TimeDimensionType.Secondary, new KeyValuePair<TimeValueType, TimePeriodType>(timeDimensionSet.SecondaryTimeDimension.TimeValueType, timeDimensionSet.SecondaryTimeDimension.TimePeriodType));
            }

            return timeDimensionality;
        }
    }
}