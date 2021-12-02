using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Common.Conversion
{
    public enum DateTimePrecisionType
    {
        Millisecond,
        Second,
        Minute
    }

    public static class DateTimePrecisionTypeUtils
    {
        public const DateTimePrecisionType DefaultPrecision_Running = DateTimePrecisionType.Second;
        public const DateTimePrecisionType DefaultPrecision_Testing = DateTimePrecisionType.Minute;

        public static DateTimePrecisionType GetDomainObjectPrecision<T>(this IDomainObject<T> domainObject)
        {
            if (DeciaTestState.IsTesting)
            { return DefaultPrecision_Testing; }
            else
            { return DefaultPrecision_Running; }
        }

        public static DateTime GetSqlInvariantDateTime(this DateTime originalDateTime)
        {
            var sqlDateTime = new SqlDateTime(originalDateTime);
            return sqlDateTime.Value;
        }
    }
}