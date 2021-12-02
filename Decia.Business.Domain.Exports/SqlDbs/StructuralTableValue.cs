using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Exports.SqlDbs
{
    public class StructuralTableValue
    {
        public StructuralTableValue(string tableAlias, GenericTable table)
        {
            TableAlias = tableAlias;
            Table = table;
            ArgInfos = new HashSet<SqlArgumentInfo>();
        }

        public string TableAlias { get; protected set; }
        public GenericTable Table { get; protected set; }
        public HashSet<SqlArgumentInfo> ArgInfos { get; protected set; }

        public string GetJoinConditionsText(string structuralIdColumnRef, string resultSetColumnRef)
        {
            var firstArgInfo = ArgInfos.First();
            var joinConditionsText = string.Empty;

            joinConditionsText += string.Format("({0} = {1}.[{2}])", structuralIdColumnRef, TableAlias, Table.Column_ForStructure.Name);
            if (Table.HasSubTable_ResultTrigger)
            { joinConditionsText += string.Format(" AND ({0} = {1}.[{2}])", resultSetColumnRef, TableAlias, Table.Column_ForResultSet.Name); }
            if (Table.HasSubTable_TimeD1Trigger)
            { joinConditionsText += string.Format(" AND ({0}.[{1}] = {2}.[{3}])", firstArgInfo.TD1_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_Id_ColumnName, TableAlias, Table.Column_ForTimeD1.Name); }
            if (Table.HasSubTable_TimeD2Trigger)
            { joinConditionsText += string.Format(" AND ({0}.[{1}] = {2}.[{3}])", firstArgInfo.TD2_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_Id_ColumnName, TableAlias, Table.Column_ForTimeD2.Name); }

            return joinConditionsText;
        }
    }
}