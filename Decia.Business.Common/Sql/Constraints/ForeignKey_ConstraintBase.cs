using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Sql.Constraints
{
    public abstract class ForeignKey_ConstraintBase : IForeignKey_Constraint
    {
        #region Constants

        public const string NamePartSpacer = GenericDatabaseUtils.NamePartSpacer;
        public const string NamePrefix = "FK";
        public const string NameFormat = (NamePrefix + "_{0}_{1}");
        public const string NameFormat_WithDimNum = (NamePrefix + "_{0}_{1}_{2}");

        public const ForeignKey_Source Default_ForeignKey_Source = ForeignKey_Source.Self;
        public const ForeignKey_Action Default_ForeignKey_Action = ForeignKey_Action.Nothing;
        public const ForeignKey_Action Cascade_ForeignKey_Action = ForeignKey_Action.Cascade;

        #endregion

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private int? m_DimensionNumber;
        private string m_Name;
        private ForeignKey_Source m_ConstraintSource;

        private bool? m_IncludeCheck;
        private ForeignKey_Action? m_DeleteRule;
        private ForeignKey_Action? m_UpdateRule;

        #endregion

        #region Constructors

        public ForeignKey_ConstraintBase(GenericTable parentTable, int? dimensionNumber, string foreignTableName)
        {
            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_DimensionNumber = dimensionNumber;

            if (!DimensionNumber.HasValue)
            { m_Name = string.Format(NameFormat, ParentTable.Name, foreignTableName); }
            else
            { m_Name = string.Format(NameFormat_WithDimNum, ParentTable.Name, foreignTableName, DimensionNumber_NonNull); }

            m_ConstraintSource = Default_ForeignKey_Source;

            m_IncludeCheck = null;
            m_DeleteRule = null;
            m_UpdateRule = null;
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public int? DimensionNumber { get { return m_DimensionNumber; } }
        public int DimensionNumber_NonNull { get { return m_DimensionNumber.HasValue ? m_DimensionNumber.Value : ModelObjectReference.MinimumAlternateDimensionNumber; } }
        public bool IsAlternateDimension { get { return (DimensionNumber_NonNull > ModelObjectReference.MinimumAlternateDimensionNumber) ? true : false; } }
        public string Name { get { return m_Name; } }

        public abstract ForeignTableData ForeignTableData { get; }
        public abstract ICollection<Guid> LocalColumnIds { get; }
        public abstract ICollection<ForeignColumnData> ForeignColumnDatas { get; }

        public ForeignKey_Source ConstraintSource
        {
            get { return m_ConstraintSource; }
            set { m_ConstraintSource = value; }
        }

        public bool HasMultipleActionPaths
        {
            get
            {
                foreach (var otherForeignKey in ParentTable.ForeignKeys)
                {
                    if (otherForeignKey == this)
                    { continue; }
                    if (this.ForeignTableData.TableName != otherForeignKey.ForeignTableData.TableName)
                    { continue; }

                    var thisColumnNames = this.ForeignColumnDatas.Select(x => x.ColumnName).OrderBy(x => x).ToList();
                    var otherColumnNames = otherForeignKey.ForeignColumnDatas.Select(x => x.ColumnName).OrderBy(x => x).ToList();

                    if (thisColumnNames.Count != otherColumnNames.Count)
                    { continue; }

                    var hasMismatch = false;
                    for (int i = 0; i < thisColumnNames.Count; i++)
                    {
                        var thisColumnName = thisColumnNames[i];
                        var otherColumnName = otherColumnNames[i];

                        if (thisColumnName != otherColumnName)
                        {
                            hasMismatch = true;
                            break;
                        }
                    }

                    if (hasMismatch)
                    { continue; }
                    else
                    { return true; }
                }
                return false;
            }
        }

        public bool RequiresExportAsTriggers
        {
            get
            {
                if (HasMultipleActionPaths)
                { return true; }
                if (ConstraintSource == ForeignKey_Source.NestedList)
                { return true; }
                return false;
            }
        }

        public bool IncludeCheck
        {
            get
            {
                if (!m_IncludeCheck.HasValue)
                { return true; }
                return m_IncludeCheck.Value;
            }
            set { m_IncludeCheck = value; }
        }

        public ForeignKey_Action DeleteRule
        {
            get
            {
                if (!m_DeleteRule.HasValue)
                { return (ConstraintSource != Default_ForeignKey_Source) ? Cascade_ForeignKey_Action : Default_ForeignKey_Action; }
                return m_DeleteRule.Value;
            }
            set { m_DeleteRule = value; }
        }

        public ForeignKey_Action UpdateRule
        {
            get
            {
                if (!m_UpdateRule.HasValue)
                { return Default_ForeignKey_Action; }
                return m_UpdateRule.Value;
            }
            set { m_UpdateRule = value; }
        }

        #endregion

        #region Methods

        public void SetDimensionNumber(int dimensionNumber)
        {
            if (m_DimensionNumber.HasValue)
            { throw new InvalidOperationException("Cannot update DimensionNumber that has already been set."); }

            m_DimensionNumber = dimensionNumber;
        }

        public abstract bool HasLocalColumnId(Guid localColumnId);
        public abstract ForeignColumnData GetCorrespondingForeignColumnData(Guid localColumnId);

        #endregion

        #region Export Methods

        public virtual string ExportSchema(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var fkeyDef = string.Empty;

                if (!RequiresExportAsTriggers)
                { fkeyDef += ExportAsForeignKey(true); }
                else
                { fkeyDef += ExportAsForeignKey(false); }
                return fkeyDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public virtual string ExportProgrammatics(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var fkeyDef = string.Empty;

                if (RequiresExportAsTriggers)
                { fkeyDef += ExportAsTriggers(); }
                return fkeyDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string ExportAsForeignKey(bool includeCheckConstraint)
        {
            includeCheckConstraint = (includeCheckConstraint && IncludeCheck);

            var checkTextValue = (includeCheckConstraint) ? "CHECK" : "NOCHECK";
            var localTableName = ParentTable.Name;
            var localColumnNames = string.Empty;
            var foreignTableName = ForeignTableData.TableName;
            var foreignColumnNames = string.Empty;

            var lastLocalColumnId = LocalColumnIds.Last();
            foreach (var localColumnId in LocalColumnIds)
            {
                var localColumnName = ParentTable.GetColumn(localColumnId).Name;
                var foreignColumnName = GetCorrespondingForeignColumnData(localColumnId).ColumnName;
                var isLast = (localColumnId == lastLocalColumnId);
                var separator = (isLast ? "" : ",");

                localColumnNames += localColumnName + separator;
                foreignColumnNames += foreignColumnName + separator;
            }


            var constraintName = string.Empty;
            if (!DimensionNumber.HasValue)
            { constraintName = string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}", NamePrefix, ParentTable.Name, foreignTableName); }
            else
            { constraintName = string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}" + NamePartSpacer + "{3}", NamePrefix, ParentTable.Name, foreignTableName, DimensionNumber_NonNull); }

            var fkeyDef = string.Format("ALTER TABLE [dbo].[{0}]  WITH {1} ADD  CONSTRAINT [{2}] FOREIGN KEY({3})", localTableName, checkTextValue, constraintName, localColumnNames) + Environment.NewLine;
            fkeyDef += string.Format("REFERENCES [dbo].[{0}] ({1})", foreignTableName, foreignColumnNames) + Environment.NewLine;

            if (includeCheckConstraint)
            {
                if (DeleteRule != Default_ForeignKey_Action)
                {
                    if (DeleteRule == ForeignKey_Action.Cascade)
                    { fkeyDef += "ON DELETE CASCADE" + Environment.NewLine; }
                    else if (DeleteRule == ForeignKey_Action.Set_Null)
                    { fkeyDef += "ON DELETE SET NULL" + Environment.NewLine; }
                    else if (DeleteRule == ForeignKey_Action.Set_Default)
                    { fkeyDef += "ON DELETE SET DEFAULT" + Environment.NewLine; }
                    else
                    { throw new InvalidOperationException("Unsupported Foreign Key action encountered."); }
                }

                if (UpdateRule != Default_ForeignKey_Action)
                {
                    if (UpdateRule == ForeignKey_Action.Cascade)
                    { fkeyDef += "ON DELETE CASCADE" + Environment.NewLine; }
                    else if (UpdateRule == ForeignKey_Action.Set_Null)
                    { fkeyDef += "ON UPDATE SET NULL" + Environment.NewLine; }
                    else if (UpdateRule == ForeignKey_Action.Set_Default)
                    { fkeyDef += "ON UPDATE SET DEFAULT" + Environment.NewLine; }
                    else
                    { throw new InvalidOperationException("Unsupported Foreign Key action encountered."); }
                }
            }

            fkeyDef += "GO" + Environment.NewLine + Environment.NewLine;
            fkeyDef += string.Format("ALTER TABLE [dbo].[{0}] {1} CONSTRAINT [{2}]", localTableName, checkTextValue, constraintName) + Environment.NewLine;
            fkeyDef += "GO" + Environment.NewLine;

            fkeyDef += GenericDatabaseUtils.ScriptSpacer_Minor;
            return fkeyDef;
        }

        private string ExportAsTriggers()
        {
            var localTableName = ParentTable.Name;
            var localColumnNames = string.Empty;
            var foreignTableName = ForeignTableData.TableName;
            var foreignColumnNames = string.Empty;
            var localTableAlias = "locl";
            var foreignTableAlias = "forgn";
            var joinOnText = string.Empty;
            var firstJoinVarText = string.Empty;

            var lastLocalColumnId = LocalColumnIds.Last();
            foreach (var localColumnId in LocalColumnIds)
            {
                var localColumnName = ParentTable.GetColumn(localColumnId).Name;
                var foreignColumnName = GetCorrespondingForeignColumnData(localColumnId).ColumnName;
                var isLast = (localColumnId == lastLocalColumnId);
                var separator = (isLast ? "" : ",");

                localColumnNames += localColumnName + separator;
                foreignColumnNames += foreignColumnName + separator;

                var andText = (isLast ? "" : " AND ");
                var joinPartFormat = ("{0}.[{1}] = {2}.[{3}]" + andText);

                joinOnText += string.Format(joinPartFormat, localTableAlias, localColumnName, foreignTableAlias, foreignColumnName);

                if (string.IsNullOrWhiteSpace(firstJoinVarText))
                { firstJoinVarText = string.Format("{0}.[{1}]", foreignTableAlias, foreignColumnName); }
            }


            string triggerDef_Local_InsertAndUpdate = string.Empty, triggerDef_Foreign_UpdateAndDelete = string.Empty;
            string triggerName_Local_InsertAndUpdate = string.Empty, triggerName_Foreign_UpdateAndDelete = string.Empty;
            string namePrefix = NamePrefix + "_" + ConstraintSource.GetConstraint_NamePrefix();

            if (!DimensionNumber.HasValue)
            {
                triggerName_Local_InsertAndUpdate = string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}" + NamePartSpacer + "Local_InsertOrUpdate", namePrefix, ParentTable.Name, foreignTableName);
                triggerName_Foreign_UpdateAndDelete = string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}" + NamePartSpacer + "Foreign_UpdateOrDelete", namePrefix, ParentTable.Name, foreignTableName);
            }
            else
            {
                triggerName_Local_InsertAndUpdate = string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}" + NamePartSpacer + "{3}" + NamePartSpacer + "Local_InsertOrUpdate", namePrefix, ParentTable.Name, foreignTableName, DimensionNumber_NonNull);
                triggerName_Foreign_UpdateAndDelete = string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}" + NamePartSpacer + "{3}" + NamePartSpacer + "Foreign_UpdateOrDelete", namePrefix, ParentTable.Name, foreignTableName, DimensionNumber_NonNull);
            }

            triggerDef_Local_InsertAndUpdate = string.Format("CREATE TRIGGER [dbo].[{0}] ON [dbo].[{1}]", triggerName_Local_InsertAndUpdate, localTableName) + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += "AFTER INSERT, UPDATE" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += "AS" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += "BEGIN" + Environment.NewLine;

            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L1 + "DECLARE @invalidCount INT;" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L1 + string.Format("SET @invalidCount = (SELECT COUNT(*) FROM [dbo].[{0}] {1} LEFT OUTER JOIN [dbo].[{2}] {3} ON {4} WHERE {5} IS NULL);", localTableName, localTableAlias, foreignTableName, foreignTableAlias, joinOnText, firstJoinVarText) + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += Environment.NewLine;

            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L1 + "IF (@invalidCount > 0)" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L2 + "ROLLBACK TRANSACTION;" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L2 + string.Format("RAISERROR ('Cannot create rows that reference non-existent keys for \"{0}\"!', 16, 1);", localTableName) + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;

            triggerDef_Local_InsertAndUpdate += "END;" + Environment.NewLine;
            triggerDef_Local_InsertAndUpdate += "GO" + Environment.NewLine;

            triggerDef_Foreign_UpdateAndDelete = string.Format("CREATE TRIGGER [dbo].[{0}] ON [dbo].[{1}]", triggerName_Foreign_UpdateAndDelete, foreignTableName) + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += "AFTER UPDATE, DELETE" + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += "AS" + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += "BEGIN" + Environment.NewLine;

            triggerDef_Foreign_UpdateAndDelete += GenericDatabaseUtils.Indent_L1 + string.Format("DELETE {0}", localTableAlias) + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += GenericDatabaseUtils.Indent_L2 + string.Format("FROM [dbo].[{0}] {1} LEFT OUTER JOIN [dbo].[{2}] {3} ON {4}", localTableName, localTableAlias, foreignTableName, foreignTableAlias, joinOnText) + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += GenericDatabaseUtils.Indent_L2 + string.Format("WHERE {0} IS NULL;", firstJoinVarText) + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += Environment.NewLine;

            triggerDef_Foreign_UpdateAndDelete += "END;" + Environment.NewLine;
            triggerDef_Foreign_UpdateAndDelete += "GO" + Environment.NewLine;

            var trgsDef = (triggerDef_Local_InsertAndUpdate + GenericDatabaseUtils.ScriptSpacer_Minor + triggerDef_Foreign_UpdateAndDelete);
            return trgsDef;
        }

        #endregion
    }
}