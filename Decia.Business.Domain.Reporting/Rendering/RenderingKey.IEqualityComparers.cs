using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public class RenderingKey_DefaultComparer : IEqualityComparer<RenderingKey>, IStringGenerator<RenderingKey>
    {
        public bool Equals(RenderingKey x, RenderingKey y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(RenderingKey obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(RenderingKey obj)
        {
            return obj.ToString();
        }
    }

    public class RenderingKey_ModelScopedComparer : IEqualityComparer<RenderingKey>, IStringGenerator<RenderingKey>
    {
        public bool Equals(RenderingKey x, RenderingKey y)
        {
            if (x.ReportGuid != y.ReportGuid)
            { return false; }
            if (x.ReportElementNumber != y.ReportElementNumber)
            { return false; }
            if (x.StructuralPoint != y.StructuralPoint)
            { return false; }
            if (x.TimeKey != y.TimeKey)
            { return false; }
            if (x.IsGroup != y.IsGroup)
            { return false; }
            if (x.GroupingDimension != y.GroupingDimension)
            { return false; }
            if (x.GroupNumber != y.GroupNumber)
            { return false; }
            return true;
        }

        public int GetHashCode(RenderingKey obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(RenderingKey obj)
        {
            string item1 = obj.ReportGuid.ToString();
            string item2 = obj.ReportElementNumber.ToString();
            string item3 = obj.StructuralPoint.HasValue ? obj.StructuralPoint.Value.ToString() : RenderingKey.NullStructuralPointText;
            string item4 = obj.TimeKey.ToString();
            string item5 = obj.IsGroup.ToString();
            string item6 = obj.GroupingDimension.HasValue ? ((int)obj.GroupingDimension).ToString() : RenderingKey.NullGroupingDimensionText;
            string item7 = obj.GroupNumber.HasValue ? obj.GroupNumber.ToString() : RenderingKey.NullGroupNumberText;

            string value = string.Format(ConversionUtils.SevenItemListFormat, item1, item2, item3, item4, item5, item6, item7);
            return value;
        }
    }

    public class RenderingKey_ReportScopedComparer : IEqualityComparer<RenderingKey>, IStringGenerator<RenderingKey>
    {
        public bool Equals(RenderingKey x, RenderingKey y)
        {
            if (x.ReportElementNumber != y.ReportElementNumber)
            { return false; }
            if (x.StructuralPoint != y.StructuralPoint)
            { return false; }
            if (x.TimeKey != y.TimeKey)
            { return false; }
            if (x.IsGroup != y.IsGroup)
            { return false; }
            if (x.GroupingDimension != y.GroupingDimension)
            { return false; }
            if (x.GroupNumber != y.GroupNumber)
            { return false; }
            return true;
        }

        public int GetHashCode(RenderingKey obj)
        {
            return ToString(obj).GetHashCode();
        }

        public string ToString(RenderingKey obj)
        {
            string item1 = obj.ReportElementNumber.ToString();
            string item2 = obj.StructuralPoint.HasValue ? obj.StructuralPoint.Value.ToString() : RenderingKey.NullStructuralPointText;
            string item3 = obj.TimeKey.ToString();
            string item4 = obj.IsGroup.ToString();
            string item5 = obj.GroupingDimension.HasValue ? ((int)obj.GroupingDimension).ToString() : RenderingKey.NullGroupingDimensionText;
            string item6 = obj.GroupNumber.HasValue ? obj.GroupNumber.ToString() : RenderingKey.NullGroupNumberText;

            string value = string.Format(ConversionUtils.SixItemListFormat, item1, item2, item3, item4, item5, item6);
            return value;
        }
    }
}