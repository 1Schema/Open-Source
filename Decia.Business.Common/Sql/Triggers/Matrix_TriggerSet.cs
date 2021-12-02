using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Constraints;

namespace Decia.Business.Common.Sql.Triggers
{
    public class Matrix_TriggerSet
    {
        public const string NamePartSpacer = GenericDatabaseUtils.NamePartSpacer;
        public const string NamePrefix = "MATRIX";
        public const string NameFormat = (NamePrefix + "_{0}");

        #region Members

        private GenericTable m_ParentTable;
        private Guid m_Id;
        private string m_Name;
        private List<Guid> m_ConstrainedColumnIds;

        #endregion

        #region Constructor

        public Matrix_TriggerSet(GenericTable parentTable, IEnumerable<Guid> constrainedColumnIds)
        {
            m_ParentTable = parentTable;
            m_Id = Guid.NewGuid();
            m_Name = string.Format(NameFormat, ParentTable.Name);
            m_ConstrainedColumnIds = new List<Guid>(constrainedColumnIds);

            foreach (var constrainedColumnId in ConstrainedColumnIds)
            {
                var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(constrainedColumnId);
                foreignKey.ConstraintSource = ForeignKey_Source.Matrix;
            }
        }

        #endregion

        #region Properties

        public GenericTable ParentTable { get { return m_ParentTable; } }
        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public ICollection<Guid> ConstrainedColumnIds
        {
            get
            {
                var columnIds = new ReadOnlyList<Guid>(m_ConstrainedColumnIds);
                columnIds.IsReadOnly = true;
                return columnIds;
            }
        }
        public ICollection<GenericColumn> ConstrainedColumns
        {
            get
            {
                var columns = new ReadOnlyList<GenericColumn>(m_ConstrainedColumnIds.Select(x => ParentTable.GetColumn(x)));
                columns.IsReadOnly = true;
                return columns;
            }
        }

        public string GetInsert_TriggerName { get { return string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "Insert", NamePrefix, ParentTable.Name); } }
        public string GetUpdate_TriggerName { get { return string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "Update", NamePrefix, ParentTable.Name); } }
        public string GetDelete_TriggerName { get { return string.Format("{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "Delete", NamePrefix, ParentTable.Name); } }

        #endregion

        #region Export Methods

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                foreach (var constrainedColumnId in ConstrainedColumnIds)
                {
                    var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(constrainedColumnId);

                    if (foreignKey.ConstraintSource != ForeignKey_Source.Matrix)
                    { throw new InvalidOperationException("The Foreign Key delete action has been changed to invalid setting."); }
                }

                var allScripts = string.Empty;

                var matrixScripts = CreateMatrixTableConstraints(dbType);
                allScripts += matrixScripts;

                foreach (var columnId in ConstrainedColumnIds)
                {
                    var relatedListScripts = CreateRelatedListTableConstraints(dbType, columnId);
                    allScripts += GenericDatabaseUtils.ScriptSpacer_Minor + relatedListScripts;
                }
                return allScripts;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string CreateMatrixTableConstraints(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                string triggerDef_Insert = string.Empty, triggerDef_Update = string.Empty, triggerDef_Delete = string.Empty;


                var triggerName_Insert = string.Format("[dbo].[{0}]", GetInsert_TriggerName);
                var triggerTableName_Insert = string.Format("[dbo].[{0}]", ParentTable.Name);

                triggerDef_Insert = string.Format("CREATE TRIGGER {0} ON {1}", triggerName_Insert, triggerTableName_Insert) + Environment.NewLine;
                triggerDef_Insert += "AFTER INSERT" + Environment.NewLine;
                triggerDef_Insert += "AS" + Environment.NewLine;
                triggerDef_Insert += "BEGIN" + Environment.NewLine;

                var varNames = new List<string>();
                foreach (var columnId in ConstrainedColumnIds)
                {
                    var column = ParentTable.GetColumn(columnId);
                    var varName = column.Name.ToCoding_VarName();
                    varNames.Add(varName);

                    var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(columnId);

                    triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} INT;", varName) + Environment.NewLine;
                    triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = (SELECT COUNT(*) FROM [dbo].[{1}]);", varName, foreignKey.ForeignTableData.TableName) + Environment.NewLine;
                    triggerDef_Insert += Environment.NewLine;
                }

                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + "DECLARE @expectedCount INT;" + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + "DECLARE @actualCount INT;" + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + string.Format("SET @expectedCount = ({0});", varNames.ToCoding_ArgString(" * ")) + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + string.Format("SET @actualCount = (SELECT COUNT(*) FROM [dbo].[{0}]);", ParentTable.Name) + Environment.NewLine;
                triggerDef_Insert += Environment.NewLine;

                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + "IF (@actualCount > @expectedCount)" + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + "ROLLBACK TRANSACTION;" + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + string.Format("RAISERROR ('Cannot create extra rows in Matrix \"{0}\"!', 16, 1);", ParentTable.Name) + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;

                triggerDef_Insert += "END;" + Environment.NewLine;
                triggerDef_Insert += "GO" + Environment.NewLine;


                var triggerName_Update = string.Format("[dbo].[{0}]", GetUpdate_TriggerName);
                var triggerTableName_Update = string.Format("[dbo].[{0}]", ParentTable.Name);

                triggerDef_Update = string.Format("CREATE TRIGGER {0} ON {1}", triggerName_Update, triggerTableName_Update) + Environment.NewLine;
                triggerDef_Update += "FOR UPDATE" + Environment.NewLine;
                triggerDef_Update += "AS" + Environment.NewLine;
                triggerDef_Update += "BEGIN" + Environment.NewLine;

                var columnIds = ConstrainedColumnIds;
                var lastColumnId = columnIds.Last();

                foreach (var columnId in columnIds)
                {
                    var column = ParentTable.GetColumn(columnId);

                    triggerDef_Update += GenericDatabaseUtils.Indent_L1 + string.Format("IF UPDATE([{0}])", column.Name) + Environment.NewLine;
                    triggerDef_Update += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                    triggerDef_Update += GenericDatabaseUtils.Indent_L2 + string.Format("RAISERROR('Changes to Column \"{0}\" in Matrix \"{1}\" are not allowed', 16, 1);", column.Name, ParentTable.Name) + Environment.NewLine;
                    triggerDef_Update += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
                    triggerDef_Update += ((lastColumnId != columnId) ? Environment.NewLine : string.Empty);
                }

                triggerDef_Update += "END;" + Environment.NewLine;
                triggerDef_Update += "GO" + Environment.NewLine;


                var triggerName_Delete = string.Format("[dbo].[{0}]", GetDelete_TriggerName);
                var triggerTableName_Delete = string.Format("[dbo].[{0}]", ParentTable.Name);

                triggerDef_Delete = string.Format("CREATE TRIGGER {0} ON {1}", triggerName_Delete, triggerTableName_Delete) + Environment.NewLine;
                triggerDef_Delete += "AFTER DELETE" + Environment.NewLine;
                triggerDef_Delete += "AS" + Environment.NewLine;
                triggerDef_Delete += "BEGIN" + Environment.NewLine;

                varNames = new List<string>();
                foreach (var columnId in ConstrainedColumnIds)
                {
                    var column = ParentTable.GetColumn(columnId);
                    var varName = column.Name.ToCoding_VarName();
                    varNames.Add(varName);

                    var foreignKey = ParentTable.GetForeignKeyForConstrainedColumn(columnId);

                    triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} INT;", varName) + Environment.NewLine;
                    triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = (SELECT COUNT(*) FROM [dbo].[{1}]);", varName, foreignKey.ForeignTableData.TableName) + Environment.NewLine;
                    triggerDef_Delete += Environment.NewLine;
                }

                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + "DECLARE @expectedCount INT;" + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + "DECLARE @actualCount INT;" + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + string.Format("SET @expectedCount = ({0});", varNames.ToCoding_ArgString(" * ")) + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + string.Format("SET @actualCount = (SELECT COUNT(*) FROM [dbo].[{0}]);", ParentTable.Name) + Environment.NewLine;
                triggerDef_Delete += Environment.NewLine;

                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + "IF (@actualCount < @expectedCount)" + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L2 + "ROLLBACK TRANSACTION;" + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L2 + string.Format("RAISERROR ('Cannot delete required rows in Matrix \"{0}\"!', 16, 1);", ParentTable.Name) + Environment.NewLine;
                triggerDef_Delete += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;

                triggerDef_Delete += "END;" + Environment.NewLine;
                triggerDef_Delete += "GO" + Environment.NewLine;


                return (triggerDef_Insert + GenericDatabaseUtils.ScriptSpacer_Minor + triggerDef_Update + GenericDatabaseUtils.ScriptSpacer_Minor + triggerDef_Delete);
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private string CreateRelatedListTableConstraints(SqlDb_TargetType dbType, Guid localSourceColumnId)
        {
            var localTableName = ParentTable.Name;
            var allLocalRelatedColumnIds = ConstrainedColumnIds.ToList();
            var localSourceColumn = ParentTable.GetColumn(localSourceColumnId);
            var sourceForeignKey = ParentTable.GetForeignKeyForConstrainedColumn(localSourceColumnId);
            var foreignTableName = sourceForeignKey.ForeignTableData.TableName;
            var sourceForeignColumnData = sourceForeignKey.GetCorrespondingForeignColumnData(localSourceColumnId);

            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                string triggerDef_Insert = string.Empty, triggerDef_Delete = string.Empty;


                var triggerName_Insert = string.Format("[dbo].[{0}" + NamePartSpacer + "{1}" + NamePartSpacer + "{2}" + NamePartSpacer + "Insert" + NamePartSpacer + "{3}]", NamePrefix, localTableName, foreignTableName, localSourceColumn.Name);
                var triggerTableName_Insert = string.Format("[dbo].[{0}]", foreignTableName);

                triggerDef_Insert = string.Format("CREATE TRIGGER {0} ON {1}", triggerName_Insert, triggerTableName_Insert) + Environment.NewLine;
                triggerDef_Insert += "AFTER INSERT" + Environment.NewLine;
                triggerDef_Insert += "AS" + Environment.NewLine;
                triggerDef_Insert += "BEGIN" + Environment.NewLine;

                var tempSetName = "reqs";
                var localSetName = "rel";

                var tempTable_ColumnNames = string.Empty;
                var tempTable_SelectForReqs = string.Empty;
                var tempTable_FromForReqs = string.Empty;
                var tempTable_InsertForCreation_Args = string.Format("[{0}], [{1}], ", ParentTable.Column_ForId.Name, ParentTable.Column_ForName.Name);
                var tempTable_SelectForCreation_Name = string.Empty;
                var tempTable_SelectForCreation_RelatedIds = string.Empty;
                var tempTable_JoinOnForCreation = string.Empty;

                for (int i = 0; i < allLocalRelatedColumnIds.Count; i++)
                {
                    var localRelatedColumnId = allLocalRelatedColumnIds[i];
                    var localRelatedColumn = ParentTable.GetColumn(localRelatedColumnId);
                    var relatedForeignKey = ParentTable.GetForeignKeyForConstrainedColumn(localRelatedColumnId);
                    var relatedForeignTableData = relatedForeignKey.ForeignTableData;
                    var relatedForeignColumnData = relatedForeignKey.GetCorrespondingForeignColumnData(localRelatedColumnId);

                    var tempTable_IdColumnName = relatedForeignColumnData.ColumnName + (i + 1);
                    var tempTable_NameColumnName = relatedForeignTableData.ColumnName_ForName + (i + 1);

                    var isCurrentFocus = (localRelatedColumnId == localSourceColumnId);
                    var isLast = (i == (allLocalRelatedColumnIds.Count - 1));
                    var foreignSetName = "ent_" + (i + 1);

                    if (true)
                    {
                        var idColumnDef = string.Format("{0}_{1} {2}", relatedForeignTableData.TableName, tempTable_IdColumnName, relatedForeignColumnData.DataType.ToNativeDataType(dbType));
                        var nameColumnDef = string.Format("{0}_{1} {2}", relatedForeignTableData.TableName, tempTable_NameColumnName, relatedForeignTableData.DataType_ForName.ToNativeDataType(dbType));
                        tempTable_ColumnNames += (idColumnDef + ", " + nameColumnDef) + (isLast ? "" : ", ");
                    }
                    if (true)
                    {
                        var idColumnDef = string.Format("{0}.[{1}]", foreignSetName, relatedForeignColumnData.ColumnName);
                        var nameColumnDef = string.Format("{0}.[{1}]", foreignSetName, relatedForeignTableData.ColumnName_ForName);
                        tempTable_SelectForReqs += (idColumnDef + ", " + nameColumnDef) + (isLast ? "" : ", ");
                    }
                    if (true)
                    {
                        var tableName = (isCurrentFocus) ? "inserted" : string.Format("[dbo].[{0}]", relatedForeignTableData.TableName);
                        tempTable_FromForReqs += (tableName + " " + foreignSetName) + (isLast ? "" : ", ");
                    }
                    if (true)
                    {
                        var columnName = string.Format("[{0}]", localRelatedColumn.Name);
                        tempTable_InsertForCreation_Args += columnName + (isLast ? "" : ", ");
                    }
                    if (true)
                    {
                        var idColumnDef = string.Format("{0}.[{1}_{2}]", tempSetName, relatedForeignTableData.TableName, tempTable_IdColumnName);
                        var nameColumnDef = string.Format("{0}.[{1}_{2}]", tempSetName, relatedForeignTableData.TableName, tempTable_NameColumnName);
                        tempTable_SelectForCreation_Name += (nameColumnDef) + (isLast ? "" : " + ',' + ");
                        tempTable_SelectForCreation_RelatedIds += idColumnDef + (isLast ? "" : ", ");
                    }
                    if (true)
                    {
                        var tempIdColumnDef = string.Format("{0}.[{1}_{2}]", tempSetName, relatedForeignTableData.TableName, tempTable_IdColumnName);
                        var localIdColumnDef = string.Format("{0}.[{1}]", localSetName, localRelatedColumn.Name);
                        tempTable_JoinOnForCreation += (tempIdColumnDef + " = " + localIdColumnDef) + (isLast ? "" : " AND ");
                    }
                }

                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE @requiredRows TABLE ( {0} );", tempTable_ColumnNames) + Environment.NewLine;
                triggerDef_Insert += Environment.NewLine;

                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + "INSERT INTO @requiredRows" + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + string.Format("SELECT {0}", tempTable_SelectForReqs) + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + string.Format("FROM {0};", tempTable_FromForReqs) + Environment.NewLine;
                triggerDef_Insert += Environment.NewLine;

                triggerDef_Insert += GenericDatabaseUtils.Indent_L1 + string.Format("INSERT INTO [dbo].[{0}] ({1})", ParentTable.Name, tempTable_InsertForCreation_Args) + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + string.Format("SELECT NEWID(), ({0}), {1}", tempTable_SelectForCreation_Name, tempTable_SelectForCreation_RelatedIds) + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + string.Format("FROM @requiredRows {0} LEFT OUTER JOIN [dbo].[{1}] {2} ON {3}", tempSetName, ParentTable.Name, localSetName, tempTable_JoinOnForCreation) + Environment.NewLine;
                triggerDef_Insert += GenericDatabaseUtils.Indent_L2 + string.Format("WHERE {0}.[{1}] IS NULL;", localSetName, ParentTable.Column_ForId.Name) + Environment.NewLine;
                triggerDef_Insert += Environment.NewLine;

                triggerDef_Insert += "END;" + Environment.NewLine;
                triggerDef_Insert += "GO" + Environment.NewLine;


                return (triggerDef_Insert + GenericDatabaseUtils.ScriptSpacer_Minor + triggerDef_Delete);
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}