using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Exports
{
    public enum TargetTypeCategory
    {
        Spreadsheet,
        SqlDb,
        NoSqlDb,
    }

    public static class TargetTypeCategoryUtils
    {
        private static readonly KeyValuePair<TargetTypeCategory, Type>[] TargetTypeValues = new KeyValuePair<TargetTypeCategory, Type>[] { new KeyValuePair<TargetTypeCategory, Type>(TargetTypeCategory.Spreadsheet, typeof(Spreadsheet_TargetType)), new KeyValuePair<TargetTypeCategory, Type>(TargetTypeCategory.SqlDb, typeof(SqlDb_TargetType)), new KeyValuePair<TargetTypeCategory, Type>(TargetTypeCategory.NoSqlDb, typeof(NoSqlDb_TargetType)) };
        public static readonly Dictionary<TargetTypeCategory, Type> TargetTypesByCategory = TargetTypeValues.ToDictionary(x => x.Key, x => x.Value);
    }
}