using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting
{
    public class ReportElementId_DefaultComparer : IEqualityComparer<ReportElementId>, IStringGenerator<ReportElementId>
    {
        public bool Equals(ReportElementId x, ReportElementId y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ReportElementId obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ReportElementId obj)
        {
            return obj.ToString();
        }
    }

    public class ReportElementId_ModelScopedComparer : IEqualityComparer<ReportElementId>, IStringGenerator<ReportElementId>
    {
        public bool Equals(ReportElementId x, ReportElementId y)
        {
            if (!x.ReportGuid.Equals(y.ReportGuid))
            { return false; }
            if (!x.ReportElementNumber.Equals(y.ReportElementNumber))
            { return false; }
            return true;
        }

        public int GetHashCode(ReportElementId obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ReportElementId obj)
        {
            string item1 = TypedIdUtils.StructToString(ReportId.ReportGuid_Prefix, obj.ReportGuid);
            string item2 = TypedIdUtils.StructToString(ReportElementId.ReportElementNumber_Prefix, obj.ReportElementNumber);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }
    }

    public class ReportElementId_ReportScopedComparer : IEqualityComparer<ReportElementId>, IStringGenerator<ReportElementId>
    {
        public bool Equals(ReportElementId x, ReportElementId y)
        {
            return x.ReportElementNumber.Equals(y.ReportElementNumber);
        }

        public int GetHashCode(ReportElementId obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ReportElementId obj)
        {
            return TypedIdUtils.StructToString(ReportElementId.ReportElementNumber_Prefix, obj.ReportElementNumber);
        }
    }
}