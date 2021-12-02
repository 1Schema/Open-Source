using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Exports
{
    public enum SqlDb_TargetType
    {
        [Description("Microsoft SQL Server®")]
        [IsImplemented(true)]
        MsSqlServer,
    }

    public static class SqlDb_TargetTypeUtils
    {
        public static readonly string NullValueAsSql = DeciaDataTypeUtils.NullValueString_Uppered;

        public static string ConvertToSqlValue(this SqlDb_TargetType targetType, Type dataType, object value)
        {
            var deciaDataType = dataType.GetDataTypeForSystemType();
            return ConvertToSqlValue(targetType, deciaDataType, value);
        }

        public static string ConvertToSqlValue(this SqlDb_TargetType targetType, DeciaDataType dataType, object value)
        {
            var result = string.Empty;

            if (targetType == SqlDb_TargetType.MsSqlServer)
            {
                if (value == null)
                { result = NullValueAsSql; }
                else
                {
                    var valueAsString = dataType.ToStringValue(value, true);

                    if (dataType == DeciaDataType.DateTime)
                    { result = string.Format("CONVERT(datetime, '{0}', 126)", valueAsString.Substring(0, valueAsString.Length - 4)); }
                    else if (dataType == DeciaDataType.TimeSpan)
                    { result = string.Format("CONVERT(bigint, {0})", ((TimeSpan)value).Ticks); }
                    else if ((dataType == DeciaDataType.UniqueID) || (dataType == DeciaDataType.Text))
                    { result = string.Format("'{0}'", valueAsString); }
                    else
                    { result = valueAsString; }
                }
            }
            else
            { throw new InvalidOperationException("Unsupported DbType encountered."); }

            return result;
        }

        public static string ConvertToSqlTextForColumn(this SqlDb_TargetType targetType, DeciaDataType dataType, string columnName)
        {
            var systemDataType = dataType.GetSystemTypeForDataType();
            return ConvertToSqlTextForColumn(targetType, systemDataType, columnName);
        }

        public static string ConvertToSqlTextForColumn(this SqlDb_TargetType targetType, Type dataType, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            { throw new InvalidOperationException("The ColumnName must not be null."); }

            var result = string.Empty;

            if (targetType == SqlDb_TargetType.MsSqlServer)
            {
                if ((dataType == typeof(DateTime)) || (dataType == typeof(DateTime?)))
                { result = string.Format("CONVERT(NVARCHAR(max), {0}, {1})", columnName, AdoNetUtils.Decia_DbFormatNumber_ForDateTime); }
                else if (dataType != typeof(string))
                { result = string.Format("CONVERT(NVARCHAR(max), {0})", columnName); }
                else
                { result = columnName; }
                return result;
            }
            else
            { throw new InvalidOperationException("Unsupported DbType encountered."); }
        }
    }
}