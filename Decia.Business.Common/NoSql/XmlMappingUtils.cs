using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.NoSql
{
    public enum FieldMappingType
    {
        PassThrough,
        Single_Simple,
        Single_Complex,
        Multiple_Complex
    }

    public static class XmlMappingUtils
    {
        public const string Item_Name = "ArrayItem";

        public static readonly ModelObjectReference TpId_Key_D1 = new ModelObjectReference(TimeDimensionType.Primary);
        public static readonly ModelObjectReference TpId_Key_D2 = new ModelObjectReference(TimeDimensionType.Secondary);
        public const string TpId_Name_Format = "OS_TD{0}_TP_ID";
        public static readonly string TpId_Name_D1 = string.Format(TpId_Name_Format, 1);
        public static readonly string TpId_Name_D2 = string.Format(TpId_Name_Format, 2);

        public const string Value_Name = "Value";
        public const string DocId_Name = GenericDatabaseUtils.DocId_Name;
        public const string CachedDoc_Name = GenericDatabaseUtils.CachedDoc_Name;

        public const string MaxOccurs_Unbounded = "unbounded";
    }
}