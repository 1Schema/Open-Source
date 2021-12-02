using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting
{
    public class ReportId_DefaultComparer : IEqualityComparer<ReportId>, IStringGenerator<ReportId>
    {
        public bool Equals(ReportId x, ReportId y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ReportId obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ReportId obj)
        {
            return obj.ToString();
        }
    }

    public class ReportId_ModelScopedComparer : IEqualityComparer<ReportId>, IStringGenerator<ReportId>
    {
        public bool Equals(ReportId x, ReportId y)
        {
            return x.ReportGuid.Equals(y.ReportGuid);
        }

        public int GetHashCode(ReportId obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(ReportId obj)
        {
            return TypedIdUtils.StructToString(ReportId.ReportGuid_Prefix, obj.ReportGuid);
        }
    }
}