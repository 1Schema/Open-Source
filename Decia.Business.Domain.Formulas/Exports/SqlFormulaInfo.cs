using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Formulas.Exports
{
    public class SqlFormulaInfo
    {
        public const string RowNumber_ColumnName = "RowNumber";
        public const string StartDate_ColumnName = "StartDate";
        public const string DatePartValue_ColumnName = "DatePart_Value";
        public const string DatePartMultiplier_ColumnName = "DatePart_Multiplier";

        public SqlFormulaInfo(SqlDb_TargetType dbType, ModelObjectReference resultingVariableTemplateRef, GenericDatabase genericDb)
        {
            DbType = dbType;
            ResultingVariableTemplateRef = resultingVariableTemplateRef;
            GenericDb = genericDb;

            ArgumentInfos = new Dictionary<ModelObjectReference, SqlArgumentInfo>(ModelObjectReference.DimensionalComparer);
            ArgumentOverrideInfos = new Dictionary<MultiPartKey<ModelObjectReference, Guid>, SqlArgumentOverrideInfo>();
            QueryType = SqlQueryType.SimpleSelect;

            Metadata_IsUsed = false;
        }

        public SqlDb_TargetType DbType { get; protected set; }
        public ModelObjectReference ResultingVariableTemplateRef { get; protected set; }
        public GenericDatabase GenericDb { get; protected set; }

        public Dictionary<ModelObjectReference, SqlArgumentInfo> ArgumentInfos { get; protected set; }
        public Dictionary<MultiPartKey<ModelObjectReference, Guid>, SqlArgumentOverrideInfo> ArgumentOverrideInfos { get; protected set; }
        public SqlQueryType QueryType { get; set; }

        public bool Metadata_IsUsed { get; set; }
        public string Metadata_TableAlias { get { return "mt"; } }

        public bool HasFilter { get { return !string.IsNullOrWhiteSpace(Filtering_WhereText); } }
        public string Filtering_WhereText { get; set; }
        public string Filtering_ResultColumnText { get; set; }

        public bool HasRowLimiter { get { return !string.IsNullOrWhiteSpace(RowLimiting_OrderByText); } }
        public string RowLimiting_OrderByText { get; set; }
        public string RowLimiting_WhereText { get; set; }

        public bool HasAggregation_PrimaryDate { get { return !string.IsNullOrWhiteSpace(Aggregation_PrimaryDateText); } }
        public string Aggregation_PrimaryDateText { get; set; }
        public bool HasAggregation_SecondaryDate { get { return !string.IsNullOrWhiteSpace(Aggregation_SecondaryDateText); } }
        public string Aggregation_SecondaryDateText { get; set; }
    }
}