using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Sql.Programmatics;

namespace Decia.Business.Common.Sql
{
    public delegate void FillDataDelegate(string connectionString);

    public class GenericDatabase : IDatabaseMember
    {
        public const string DataSet_Ref_Name = "Ref";

        #region Members

        private Guid m_Id;
        private string m_Name;
        private Dictionary<Guid, GenericTable> m_Tables;
        private Dictionary<Guid, GenericProcedure> m_Procedures;

        private SortedDictionary<string, GenericTable> m_TablesByName;
        private SortedDictionary<string, GenericProcedure> m_ProceduresByName;

        private SortedDictionary<int, GenericTable> m_TablesByOrder;
        private SortedDictionary<int, GenericProcedure> m_ProceduresByOrder;

        #endregion

        #region Constructors

        public GenericDatabase(string databaseName)
        {
            m_Id = Guid.NewGuid();
            m_Name = databaseName;

            m_Tables = new Dictionary<Guid, GenericTable>();
            m_Procedures = new Dictionary<Guid, GenericProcedure>();

            m_TablesByOrder = new SortedDictionary<int, GenericTable>();
            m_ProceduresByOrder = new SortedDictionary<int, GenericProcedure>();

            m_TablesByName = new SortedDictionary<string, GenericTable>();
            m_ProceduresByName = new SortedDictionary<string, GenericProcedure>();
        }

        #endregion

        #region Properties

        public Guid Id { get { return m_Id; } }
        public string Name { get { return m_Name; } }
        public ICollection<GenericTable> Tables
        {
            get
            {
                var tables = new ReadOnlyList<GenericTable>(m_Tables.Values);
                tables.IsReadOnly = true;
                return tables;
            }
        }

        #endregion

        #region Methods

        public GenericTable CreateTable(string tableName)
        {
            var table = CreateTable(tableName, GenericTable.Default_IsForInputs);
            return table;
        }

        public GenericTable CreateTable(string tableName, bool isForInputs)
        {
            var table = CreateTable(tableName, isForInputs, GenericTable.Default_IsDimensionalSource);
            return table;
        }

        public GenericTable CreateTable(string tableName, bool isForInputs, bool isDimensionalSource)
        {
            var table = new GenericTable(this, tableName, isForInputs, isDimensionalSource);
            m_Tables.Add(table.Id, table);
            m_TablesByName.Add(table.Name, table);
            m_TablesByOrder.Add(m_TablesByOrder.Count, table);
            return table;
        }

        public GenericTable GetTable(Guid tableId)
        {
            if (!m_Tables.ContainsKey(tableId))
            { return null; }
            return m_Tables[tableId];
        }

        public void AddProcedure(GenericProcedure procedure)
        {
            if (procedure.ParentDatabase != this)
            { throw new InvalidOperationException("The Parent Database does not match."); }
            if (m_Procedures.Values.Contains(procedure))
            { throw new InvalidOperationException("The Procedure has already been added."); }

            var index = m_Procedures.Count;

            m_Procedures.Add(procedure.Id, procedure);
            m_ProceduresByName.Add(procedure.Name, procedure);
            m_ProceduresByOrder.Add(index, procedure);
        }

        public DataSet GenerateDataSet()
        {
            Dictionary<GenericTable, DataTable> tableMappings;
            Dictionary<GenericColumn, DataColumn> columnMappings;
            return GenerateDataSet(out tableMappings, out columnMappings);
        }

        public DataSet GenerateDataSet(out Dictionary<GenericTable, DataTable> tableMappings, out Dictionary<GenericColumn, DataColumn> columnMappings)
        {
            tableMappings = new Dictionary<GenericTable, DataTable>();
            columnMappings = new Dictionary<GenericColumn, DataColumn>();
            var dataSet = new DataSet(Name);

            foreach (var table in m_TablesByOrder.Values)
            {
                var dataTable = dataSet.Tables.Add();
                tableMappings.Add(table, dataTable);

                dataTable.TableName = table.Name;
                dataTable.ExtendedProperties.Add(DataSet_Ref_Name, table.StructuralTypeRef.Value);

                foreach (var column in table.Columns.Where(x => !x.UsesSqlFormula).ToList())
                {
                    var dataColumn = dataTable.Columns.Add();
                    columnMappings.Add(column, dataColumn);

                    dataColumn.ColumnName = column.Name;
                    dataColumn.ExtendedProperties.Add(DataSet_Ref_Name, column.VariableTemplateRef);
                    dataColumn.AllowDBNull = true;
                    dataColumn.AutoIncrement = false;
                    dataColumn.DataType = column.DataType.GetSystemTypeForDataType();
                }
            }
            return dataSet;
        }

        #endregion

        #region Export Methods

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var dbDef = string.Empty;
                dbDef += ExportSchema(dbType);
                dbDef += ExportProgrammatics(dbType);
                return dbDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public string ExportToServer(SqlDb_TargetType dbType, string connectionString_ToMasterDb)
        {
            var script = this.ExportSchema(dbType);
            return GenericDatabaseUtils.ExportScriptToServer(this.Name, script, connectionString_ToMasterDb);
        }

        public string ExportSchema(SqlDb_TargetType dbType)
        {
            var placeholderValues = GetPlaceholderValues();

            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var dbDef = string.Empty;

                dbDef += DeciaBase_Resources.Config_Copyright_Format.InsertPlaceholderValues(placeholderValues) + Environment.NewLine + Environment.NewLine + Environment.NewLine;

                dbDef += "USE [master]" + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("IF (EXISTS (SELECT sdb.[name] FROM master.dbo.sysdatabases sdb WHERE ((name = '{0}'))))", Name) + Environment.NewLine;
                dbDef += "BEGIN" + Environment.NewLine;
                dbDef += GenericDatabaseUtils.Indent_L1 + string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", Name) + Environment.NewLine;
                dbDef += GenericDatabaseUtils.Indent_L1 + string.Format("DROP DATABASE [{0}];", Name) + Environment.NewLine;
                dbDef += "END;" + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("CREATE DATABASE [{0}]", Name) + Environment.NewLine;
                dbDef += "CONTAINMENT = NONE" + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET COMPATIBILITY_LEVEL = 120", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET ANSI_NULLS OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET ANSI_PADDING OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET ANSI_WARNINGS OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET ARITHABORT OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET AUTO_CLOSE OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET AUTO_SHRINK OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS ON", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET CURSOR_CLOSE_ON_COMMIT OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET CURSOR_DEFAULT  GLOBAL", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET CONCAT_NULL_YIELDS_NULL OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET NUMERIC_ROUNDABORT OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET QUOTED_IDENTIFIER OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET RECURSIVE_TRIGGERS OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET  DISABLE_BROKER", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET DATE_CORRELATION_OPTIMIZATION OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET TRUSTWORTHY OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET PARAMETERIZATION SIMPLE", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET READ_COMMITTED_SNAPSHOT OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET HONOR_BROKER_PRIORITY OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET RECOVERY SIMPLE", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET  MULTI_USER", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET PAGE_VERIFY CHECKSUM", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET DB_CHAINING OFF", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET TARGET_RECOVERY_TIME = 0 SECONDS", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET DELAYED_DURABILITY = DISABLED", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("ALTER DATABASE [{0}] SET  READ_WRITE", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += string.Format("USE [{0}]", Name) + Environment.NewLine;
                dbDef += "GO" + Environment.NewLine + Environment.NewLine;

                dbDef += DeciaBase_Resources.Decia_Metadata + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_TimeDimensionSetting + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_TimePeriodType + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_DataType + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ObjectType + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_TimePeriod + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_StructuralType + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplate + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplateDependency + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplateGroup + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplateGroupMember + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ResultSet + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ResultSetTimeDimensionSetting + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ResultSetProcessingMember + Environment.NewLine + Environment.NewLine;
                dbDef += GenericDatabaseUtils.ScriptSpacer_Major;

                foreach (var table in m_TablesByOrder.Values)
                {
                    dbDef += table.ExportSchema(dbType) + GenericDatabaseUtils.ScriptSpacer_Major;
                }

                return dbDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public string ExportProgrammatics(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var dbDef = string.Empty;

                dbDef += DeciaBase_Resources.spDecia_ChangeState_GetLatest + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_ChangeState_IncrementLatest + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_ChangeState_ResetLatest + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Common_GetTempTableCreationText + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Common_GatherIncludedStructuralInstances + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Common_GatherIncludedTimePeriods + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Common_GatherIncludedVariableTemplateGroups + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Time_GetNextTimePeriodStartDate + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Time_GetTimePeriodDatesForId + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Time_GetTimePeriodIdForDates + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Time_SetTimeDimensionBounds + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_ResultsLock_AssertIsNotReadOnly + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_ResultsLock_AssertIsReadOnly + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_ResultsLock_LockDeletion + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_ResultsLock_UnlockDeletion + Environment.NewLine + Environment.NewLine;
                dbDef += GenericDatabaseUtils.ScriptSpacer_Major;

                dbDef += DeciaBase_Resources.Decia_Metadata_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_TimeDimensionSetting_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_TimePeriodType_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_DataType_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ObjectType_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_TimePeriod_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_StructuralType_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplate_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplateDependency_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplateGroup_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_VariableTemplateGroupMember_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ResultSet_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ResultSetTimeDimensionSetting_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.Decia_ResultSetProcessingMember_Triggers + Environment.NewLine + Environment.NewLine;
                dbDef += GenericDatabaseUtils.ScriptSpacer_Major;

                foreach (var table in m_TablesByOrder.Values)
                {
                    var tableProgDef = table.ExportProgrammatics(dbType);

                    if (string.IsNullOrWhiteSpace(tableProgDef))
                    { continue; }

                    dbDef += table.ExportProgrammatics(dbType) + Environment.NewLine;
                }

                dbDef += DeciaBase_Resources.spDecia_Cleanup_DeleteAllResults + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Cleanup_DeleteAllOutdatedResults + Environment.NewLine + Environment.NewLine;
                dbDef += DeciaBase_Resources.spDecia_Cleanup_DeleteDesiredResults + Environment.NewLine + Environment.NewLine;

                foreach (var procedureBucket in m_ProceduresByOrder)
                {
                    var procedure = procedureBucket.Value;
                    dbDef += procedure.ExportProcedure(dbType) + Environment.NewLine + Environment.NewLine;
                    dbDef += GenericDatabaseUtils.ScriptSpacer_Major;
                }

                return dbDef;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        public string ExportCleanup(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                return "EXEC [dbo].[spDecia_ChangeState_ResetLatest] NULL, NULL;";
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        private Dictionary<string, string> GetPlaceholderValues()
        {
            var now = DateTime.UtcNow;
            var dateAsText = now.ToShortDateString() + ", " + now.ToShortTimeString() + " (UTC)";

            var placeholderValues = new Dictionary<string, string>();
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_MyDbName, this.Name);
            placeholderValues.Add(DeciaBaseUtils.Fnctn_Decia_Format_Placeholder_DateOfExportName, dateAsText);
            return placeholderValues;
        }

        #endregion
    }
}