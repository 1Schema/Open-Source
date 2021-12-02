using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.Time;

namespace Decia.Business.Domain.Formulas.Exports
{
    public class SqlArgumentOverrideInfo : SqlArgumentInfo
    {
        public const string TableAliasFormat = "{0}_{1}";

        public SqlArgumentOverrideInfo(SqlArgumentInfo baseArgumentInfo, ExpressionId sourceExpressionId)
            : base(baseArgumentInfo.VariableRef, baseArgumentInfo.StructuralRef)
        {
            BaseArgumentInfo = baseArgumentInfo;
            SourceExpressionId = sourceExpressionId;

            Str_ShiftExpression_WorksAsJoinOnText = false;
            TD1_ShiftExpression_WorksAsJoinOnText = false;
            TD2_ShiftExpression_WorksAsJoinOnText = false;

            this.DataType_TableAlias = BaseArgumentInfo.DataType_TableAlias;
            this.ObjectType_TableAlias = BaseArgumentInfo.ObjectType_TableAlias;
            this.VariableTemplate_TableAlias = BaseArgumentInfo.VariableTemplate_TableAlias;
            this.StructuralType_TableAlias = BaseArgumentInfo.StructuralType_TableAlias;
        }

        public SqlArgumentInfo BaseArgumentInfo { get; protected set; }
        public ExpressionId SourceExpressionId { get; protected set; }
        public Guid SourceExpressionGuid { get { return SourceExpressionId.ExpressionGuid; } }

        public override string VariableRef_ColumnName { get { return BaseArgumentInfo.VariableRef_ColumnName; } }
        public override string StructuralRef_Id_ColumnName { get { return BaseArgumentInfo.StructuralRef_Id_ColumnName; } }
        public override string StructuralRef_Name_ColumnName { get { return BaseArgumentInfo.StructuralRef_Name_ColumnName; } }
        public override string StructuralRef_Order_ColumnName { get { return BaseArgumentInfo.StructuralRef_Order_ColumnName; } }

        public bool Str_HasShiftExpression { get { return !string.IsNullOrWhiteSpace(Str_ShiftExpression_WhereText); } }
        public bool Str_ShiftExpression_WorksAsJoinOnText { get; set; }
        public string Str_ShiftExpression_WhereText { get; set; }
        public bool TD1_HasShiftExpression { get { return !string.IsNullOrWhiteSpace(TD1_ShiftExpression_WhereText); } }
        public bool TD1_ShiftExpression_WorksAsJoinOnText { get; set; }
        public string TD1_ShiftExpression_WhereText { get; set; }
        public bool TD2_HasShiftExpression { get { return !string.IsNullOrWhiteSpace(TD2_ShiftExpression_WhereText); } }
        public bool TD2_ShiftExpression_WorksAsJoinOnText { get; set; }
        public string TD2_ShiftExpression_WhereText { get; set; }

        public override GenericTable VariableRef_Table
        {
            get { return BaseArgumentInfo.VariableRef_Table; }
        }

        public override string VariableRef_TableAlias
        {
            get
            {
                return string.Format(TableAliasFormat, BaseArgumentInfo.VariableRef_TableAlias, SourceExpressionGuid.GetHashCode().ToString("X"));
            }
        }

        public override GenericTable StructuralRef_Table
        {
            get { return BaseArgumentInfo.StructuralRef_Table; }
        }

        public override string StructuralRef_TableAlias
        {
            get
            {
                if (!Str_HasShiftExpression)
                { return BaseArgumentInfo.StructuralRef_TableAlias; }
                return string.Format(TableAliasFormat, BaseArgumentInfo.StructuralRef_TableAlias, SourceExpressionGuid.GetHashCode().ToString("X"));
            }
        }

        public override string TD1_TimePeriod_TableAlias
        {
            get
            {
                if (!TD1_HasShiftExpression || !BaseArgumentInfo.TD1_IsRelevant)
                { return BaseArgumentInfo.TD1_TimePeriod_TableAlias; }
                return string.Format(TableAliasFormat, BaseArgumentInfo.TD1_TimePeriod_TableAlias, SourceExpressionGuid.GetHashCode().ToString("X"));
            }
        }

        public override string TD2_TimePeriod_TableAlias
        {
            get
            {
                if (!TD2_HasShiftExpression || !BaseArgumentInfo.TD2_IsRelevant)
                { return BaseArgumentInfo.TD2_TimePeriod_TableAlias; }
                return string.Format(TableAliasFormat, BaseArgumentInfo.TD2_TimePeriod_TableAlias, SourceExpressionGuid.GetHashCode().ToString("X"));
            }
        }

        public override TimePeriodType? TD1_TimePeriodType
        {
            get { return BaseArgumentInfo.TD1_TimePeriodType; }
        }

        public virtual string TD1_TimePeriodType_TableAlias
        {
            get { return BaseArgumentInfo.TD1_TimePeriodType_TableAlias; }
        }

        public override TimePeriodType? TD2_TimePeriodType
        {
            get { return BaseArgumentInfo.TD2_TimePeriodType; }
        }

        public virtual string TD2_TimePeriodType_TableAlias
        {
            get { return BaseArgumentInfo.TD2_TimePeriodType_TableAlias; }
        }
    }

    public class SqlArgumentInfo
    {
        public const bool Default_SetIsUsed = true;
        public const string TableAliasFormat_ForAltDims = "{0}_altdim{1}";

        public SqlArgumentInfo(ModelObjectReference variableTemplateRef, ModelObjectReference structuralRef)
        {
            VariableRef = variableTemplateRef;
            StructuralRef = structuralRef;

            VariableRef_IsUsed = false;
            StructuralRef_IsUsed = false;
            DataType_IsUsed = false;
            ObjectType_IsUsed = false;
            VariableTemplate_IsUsed = false;
            StructuralType_IsUsed = false;
            TD1_TimePeriod_IsUsed = false;
            TD1_TimePeriod_UsesMin = false;
            TD1_TimePeriod_UsesMax = false;
            TD1_TimePeriodType_IsUsed = false;
            TD2_TimePeriod_IsUsed = false;
            TD2_TimePeriod_UsesMin = false;
            TD2_TimePeriod_UsesMax = false;
            TD2_TimePeriodType_IsUsed = false;
        }

        public ModelObjectReference VariableRef { get; protected set; }
        public ModelObjectReference StructuralRef { get; protected set; }
        public int? AlternateDimenionNumberToUse { get { return (VariableRef.NonNullAlternateDimensionNumber > ModelObjectReference.MinimumAlternateDimensionNumber) ? VariableRef.NonNullAlternateDimensionNumber : (Nullable<int>)null; } }

        public bool VariableRef_IsUsed { get; set; }
        public virtual GenericTable VariableRef_Table { get; set; }
        public virtual string VariableRef_TableAlias { get { return (AlternateDimenionNumberToUse.HasValue) ? string.Format(SqlArgumentInfo.TableAliasFormat_ForAltDims, VariableRef_Table.Alias, AlternateDimenionNumberToUse) : VariableRef_Table.Alias; } }
        public virtual string VariableRef_ColumnName { get; set; }

        public bool StructuralRef_IsUsed { get; set; }
        public virtual GenericTable StructuralRef_Table { get; set; }
        public virtual string StructuralRef_TableAlias { get { return (AlternateDimenionNumberToUse.HasValue) ? string.Format(SqlArgumentInfo.TableAliasFormat_ForAltDims, StructuralRef_Table.Alias, AlternateDimenionNumberToUse) : StructuralRef_Table.Alias; } }
        public virtual string StructuralRef_Id_ColumnName { get; set; }
        public virtual string StructuralRef_Name_ColumnName { get; set; }
        public virtual string StructuralRef_Order_ColumnName { get; set; }

        public bool DataType_IsUsed { get; set; }
        public string DataType_TableAlias { get; set; }

        public bool ObjectType_IsUsed { get; set; }
        public string ObjectType_TableAlias { get; set; }

        public bool VariableTemplate_IsUsed { get; set; }
        public string VariableTemplate_TableAlias { get; set; }

        public bool StructuralType_IsUsed { get; set; }
        public string StructuralType_TableAlias { get; set; }

        public bool TD1_IsRelevant { get { return !string.IsNullOrWhiteSpace(TD1_TimePeriod_TableAlias); } }
        public bool TD1_TimePeriod_IsUsed { get; set; }
        public bool TD1_TimePeriod_UsesMin { get; set; }
        public bool TD1_TimePeriod_UsesMax { get; set; }
        public virtual string TD1_TimePeriod_TableAlias { get; set; }

        public virtual TimePeriodType? TD1_TimePeriodType { get; set; }
        public bool TD1_TimePeriodType_IsUsed { get; set; }
        public virtual string TD1_TimePeriodType_TableAlias { get; set; }

        public bool TD2_IsRelevant { get { return !string.IsNullOrWhiteSpace(TD2_TimePeriod_TableAlias); } }
        public bool TD2_TimePeriod_IsUsed { get; set; }
        public bool TD2_TimePeriod_UsesMin { get; set; }
        public bool TD2_TimePeriod_UsesMax { get; set; }
        public virtual string TD2_TimePeriod_TableAlias { get; set; }

        public virtual TimePeriodType? TD2_TimePeriodType { get; set; }
        public bool TD2_TimePeriodType_IsUsed { get; set; }
        public virtual string TD2_TimePeriodType_TableAlias { get; set; }

        #region Methods

        public string GetTimePeriod_TableAlias(int dimensionNumber)
        {
            return GetTimePeriod_TableAlias(dimensionNumber, Default_SetIsUsed);
        }

        public string GetTimePeriod_TableAlias(int dimensionNumber, bool setIsUsed)
        {
            var timeDimensionType = dimensionNumber.GetTimeDimensionTypeForNumber(false);
            return GetTimePeriod_TableAlias(timeDimensionType, setIsUsed);
        }

        public string GetTimePeriod_TableAlias(TimeDimensionType dimensionType)
        {
            return GetTimePeriod_TableAlias(dimensionType, Default_SetIsUsed);
        }

        public string GetTimePeriod_TableAlias(TimeDimensionType dimensionType, bool setIsUsed)
        {
            if (dimensionType == TimeDimensionType.Primary)
            {
                if (!TD1_IsRelevant)
                { return null; }

                TD1_TimePeriod_IsUsed = (setIsUsed) ? true : TD1_TimePeriod_IsUsed;
                return TD1_TimePeriod_TableAlias;
            }
            if (dimensionType == TimeDimensionType.Secondary)
            {
                if (!TD2_IsRelevant)
                { return null; }

                TD2_TimePeriod_IsUsed = (setIsUsed) ? true : TD2_TimePeriod_IsUsed;
                return TD2_TimePeriod_TableAlias;
            }
            return null;
        }

        #endregion
    }
}