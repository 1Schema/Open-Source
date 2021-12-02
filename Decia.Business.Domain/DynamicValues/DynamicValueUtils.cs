using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.DynamicValues
{
    public static class DynamicValueUtils
    {
        public static bool TryToConvertToNumber(DeciaDataType dataType, string stringValue, out Nullable<double> doubleValue)
        {
            if (stringValue == DynamicValue.NullInternalValue)
            {
                doubleValue = null;
                return true;
            }
            else if ((dataType == DeciaDataType.Boolean)
                || (dataType == DeciaDataType.Integer)
                || (dataType == DeciaDataType.Decimal)
                || (dataType == DeciaDataType.DateTime)
                || (dataType == DeciaDataType.TimeSpan))
            {
                bool success = false;
                double result;

                success = double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                if (success)
                {
                    doubleValue = result;
                    return true;
                }
            }
            else if (dataType == DeciaDataType.Text)
            {
                doubleValue = null;
                return true;
            }

            doubleValue = null;
            return false;
        }

        public static IEnumerable<DeciaDataType> GetDataTypesForBaseValues(this IEnumerable<DynamicValue> baseValues)
        {
            List<DeciaDataType> dataTypes = new List<DeciaDataType>();

            for (int i = 0; i < baseValues.Count(); i++)
            { dataTypes.Add(baseValues.ElementAt(i).DataType); }

            return dataTypes;

        }

        public static IDictionary<int, DeciaDataType> GetDataTypesForBaseValues(this IDictionary<int, DynamicValue> baseValues)
        {
            Dictionary<int, DeciaDataType> dataTypes = new Dictionary<int, DeciaDataType>();

            for (int i = 0; i < baseValues.Count(); i++)
            { dataTypes.Add(i, baseValues[i].DataType); }

            return dataTypes;
        }
    }
}