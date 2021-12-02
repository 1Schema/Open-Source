using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql.Base;

namespace Decia.Business.Common.Sql.Programmatics
{
    public class OrchestrateComputation_GenericProcedure : GenericProcedure
    {
        public const string ProcedureNameBase = "spDecia_Orchestration_ComputeResultSet_";
        public const string InputParameterName_Name = "@resultSetName";
        public const string InputParameterName_Description = "@resultSetDescription";
        public const string InputParameterName_StartDate = "@startDate";
        public const string InputParameterName_EndDate = "@endDate";
        public const string InputParameterName_VariableTemplateIds = "@variableTemplateIds";
        public const string OutputParameterName_ResultSetId = "@resultSetId";

        #region Members

        private SortedDictionary<int, ComputeVariableTemplateGroup_GenericProcedure> m_ComputationSubProcedures;
        private Func<SqlDb_TargetType, string> m_ResultsCreationMethod;

        private GenericParameter m_InputParameter_ForName;
        private GenericParameter m_InputParameter_ForDescription;
        private GenericParameter m_InputParameter_ForStartDate;
        private GenericParameter m_InputParameter_ForEndDate;
        private GenericParameter m_InputParameter_ForVariableTemplateIds;
        private Dictionary<ModelObjectReference, GenericParameter> m_InputParameters_ForStructuralInstanceIds;

        #endregion

        #region Constructor

        public OrchestrateComputation_GenericProcedure(GenericDatabase parentDatabase)
            : base(parentDatabase, ProcedureNameBase)
        {
            m_ComputationSubProcedures = new SortedDictionary<int, ComputeVariableTemplateGroup_GenericProcedure>();
            m_ResultsCreationMethod = null;

            m_InputParameter_ForName = this.AddInputParameter(InputParameterName_Name, DeciaDataType.Text);
            m_InputParameter_ForDescription = this.AddInputParameter(InputParameterName_Description, DeciaDataType.Text);
            m_InputParameter_ForStartDate = this.AddInputParameter(InputParameterName_StartDate, DeciaDataType.DateTime);
            m_InputParameter_ForEndDate = this.AddInputParameter(InputParameterName_EndDate, DeciaDataType.DateTime);
            m_InputParameter_ForVariableTemplateIds = this.AddInputParameter(InputParameterName_VariableTemplateIds, DeciaDataType.Text);
            m_InputParameters_ForStructuralInstanceIds = new Dictionary<ModelObjectReference, GenericParameter>();
            SetOutputParameter(OutputParameterName_ResultSetId, DeciaDataType.UniqueID);

            m_InputParameter_ForStartDate.SourceTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_TimePeriod.TableName;
            m_InputParameter_ForStartDate.DefaultValue = DeciaBaseUtils.NullValue;
            m_InputParameter_ForEndDate.SourceTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_TimePeriod.TableName;
            m_InputParameter_ForEndDate.DefaultValue = DeciaBaseUtils.NullValue;
            m_InputParameter_ForVariableTemplateIds.SourceTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_VariableTemplate.TableName;
            m_InputParameter_ForVariableTemplateIds.DefaultValue = DeciaBaseUtils.NullValue;
            OutputParameter.DefaultValue = DeciaBaseUtils.NullValue;
        }

        #endregion

        #region Properties

        public SortedDictionary<int, ComputeVariableTemplateGroup_GenericProcedure> ComputationSubProcedures { get { return new SortedDictionary<int, ComputeVariableTemplateGroup_GenericProcedure>(m_ComputationSubProcedures); } }
        public Func<SqlDb_TargetType, string> ResultsCreationMethod
        {
            get { return m_ResultsCreationMethod; }
            set { m_ResultsCreationMethod = value; }
        }

        public GenericParameter InputParameter_ForName { get { return m_InputParameter_ForName; } }
        public GenericParameter InputParameter_ForDescription { get { return m_InputParameter_ForDescription; } }
        public GenericParameter InputParameter_ForStartDate { get { return m_InputParameter_ForStartDate; } }
        public GenericParameter InputParameter_ForEndDate { get { return m_InputParameter_ForEndDate; } }
        public GenericParameter InputParameter_ForVariableTemplateIds { get { return m_InputParameter_ForVariableTemplateIds; } }
        public IDictionary<ModelObjectReference, GenericParameter> InputParameters_ForStructuralInstanceIds { get { return m_InputParameters_ForStructuralInstanceIds.ToDictionary(x => x.Key, x => x.Value); } }

        #endregion

        #region Methods

        public GenericParameter AddInputParameter_ForStructuralInstanceIds(string name, DeciaDataType dataType, ModelObjectReference structuralTypeRef, string exportedTableName)
        {
            if (m_InputParameters_ForStructuralInstanceIds.ContainsKey(structuralTypeRef))
            { throw new InvalidOperationException("The Input Parameter for Variable Template Ids has already been set."); }

            var inputParameter = AddInputParameter(name, dataType);
            m_InputParameters_ForStructuralInstanceIds.Add(structuralTypeRef, inputParameter);
            inputParameter.SourceTableName = exportedTableName;
            inputParameter.DefaultValue = "NULL";
            return inputParameter;
        }

        public ComputeVariableTemplateGroup_GenericProcedure AddCompute_SubProcedure(Guid id, string name, ICollection<ModelObjectReference> includedVariableTemplateRefs)
        {
            var index = (m_ComputationSubProcedures.Count + 1);
            var subProcedure = new ComputeVariableTemplateGroup_GenericProcedure(this, id, name, index, includedVariableTemplateRefs);
            m_ComputationSubProcedures.Add(index, subProcedure);
            return subProcedure;
        }

        #endregion

        #region Export Methods

        public override string ExportProcedure(SqlDb_TargetType dbType)
        {
            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var procParams_Required = new GenericParameter[] { InputParameter_ForName, InputParameter_ForDescription, InputParameter_ForStartDate, InputParameter_ForEndDate };

                var procParams_Full = new List<GenericParameter>(procParams_Required);
                procParams_Full.Add(OutputParameter);

                var procParams_Partial = new List<GenericParameter>(procParams_Required);
                procParams_Partial.Add(InputParameter_ForVariableTemplateIds);
                foreach (var param in InputParameters_ForStructuralInstanceIds.Values)
                { procParams_Partial.Add(param); }
                procParams_Partial.Add(OutputParameter);

                var procName_Partial = Name + "Partial";
                var headerText_Partial = GetProcedureHeader(dbType, procName_Partial, procParams_Partial);
                var bodyText_Partial = string.Empty;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "IF (object_id('tempdb..#DeciaResults_DeleteLock') IS NULL)" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "CREATE TABLE #DeciaResults_DeleteLock (IsLocked BIT);" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_ResultsLock_UnlockDeletion];" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + string.Format("CREATE TABLE {0} (Id UNIQUEIDENTIFIER, Name NVARCHAR(MAX), SortingIndex INT, StructuralTypeId UNIQUEIDENTIFIER);", DeciaBaseUtils.Included_StructuralInstance_TableName) + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + string.Format("CREATE TABLE {0} (TimePeriodId UNIQUEIDENTIFIER, TimePeriodTypeId INT, StartDate DATETIME, EndDate DATETIME, OrderIndex INT);", DeciaBaseUtils.Included_TimePeriod_TableName) + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + string.Format("CREATE TABLE {0} (VariableTemplateId UNIQUEIDENTIFIER, VariableTemplateGroupId UNIQUEIDENTIFIER, OrderIndex INT);", DeciaBaseUtils.Included_VariableTemplate_TableName) + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + string.Format("CREATE TABLE {0} (VariableTemplateGroupId UNIQUEIDENTIFIER, OrderIndex INT);", DeciaBaseUtils.Included_VariableTemplateGroup_TableName) + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "DECLARE @changeCount bigint;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "DECLARE @changeDate datetime;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_ChangeState_GetLatest] @changeCount OUTPUT, @changeDate OUTPUT;" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "SET @resultSetId = NEWID();" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "INSERT INTO [dbo].[Decia_ResultSet] ([Id], [Name], [Description], [Metadata_ChangeCount], [Metadata_ChangeDate])" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "VALUES (@resultSetId, @resultSetName, @resultSetDescription, @changeCount, @changeDate);" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "DECLARE @actualStartDate datetime;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "DECLARE @actualEndDate datetime;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "SET @actualStartDate = @startDate;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "SET @actualEndDate = @endDate;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "IF (@actualStartDate IS NULL)" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "SET @actualStartDate = (SELECT [StartDate] FROM [dbo].[Decia_TimeDimensionSetting]);" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "IF (@actualEndDate IS NULL)" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "SET @actualEndDate = (SELECT [EndDate] FROM [dbo].[Decia_TimeDimensionSetting]);" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "INSERT INTO [dbo].[Decia_ResultSetTimeDimensionSetting]" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "VALUES (@resultSetId, @actualStartDate, @actualEndDate);" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                foreach (var structuralParameter in InputParameters_ForStructuralInstanceIds)
                {
                    var paramName = structuralParameter.Value.Name;
                    var tableName = structuralParameter.Value.SourceTableName;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L1 + string.Format("EXEC [dbo].[spDecia_Common_GatherIncludedStructuralInstances] '{0}', {1};", tableName, paramName) + Environment.NewLine;
                }
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_Common_GatherIncludedTimePeriods] @actualStartDate, @actualEndDate;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "EXEC [dbo].[spDecia_Common_GatherIncludedVariableTemplateGroups] @variableTemplateIds;" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "INSERT INTO [dbo].[Decia_ResultSetProcessingMember] ([ResultSetId], [VariableTemplateGroupId], [OrderIndex])" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "SELECT @resultSetId, ivtg.[VariableTemplateGroupId], ivtg.[OrderIndex]" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + string.Format("FROM {0} ivtg;", DeciaBaseUtils.Included_VariableTemplateGroup_TableName) + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += (ResultsCreationMethod != null) ? ResultsCreationMethod(dbType) : string.Empty;
                bodyText_Partial += Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "DECLARE @computeResult BIT;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "DECLARE @vtgComputeResult BIT;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "SET @computeResult = 0;" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "SET @vtgComputeResult = 0;" + Environment.NewLine;
                bodyText_Partial += Environment.NewLine;
                foreach (var subProcBucket in m_ComputationSubProcedures)
                {
                    var subProc = subProcBucket.Value;
                    var groupId = subProc.GroupId;
                    var subProcName = subProc.Name;

                    bodyText_Partial += GenericDatabaseUtils.Indent_L1 + string.Format("IF (EXISTS (SELECT ivtg.[VariableTemplateGroupId] FROM {0} ivtg WHERE ((ivtg.[VariableTemplateGroupId] = '{1}'))))", DeciaBaseUtils.Included_VariableTemplateGroup_TableName, groupId) + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "BEGIN" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "UPDATE [dbo].[Decia_ResultSetProcessingMember]" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L3 + "SET [Computation_StartDate] = GETDATE()" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L3 + string.Format("WHERE [VariableTemplateGroupId] = '{0}';", groupId) + Environment.NewLine;
                    bodyText_Partial += Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "SET @vtgComputeResult = 0;" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L2 + string.Format("EXEC [dbo].[{0}] @resultSetId, @vtgComputeResult OUTPUT;", subProcName) + Environment.NewLine;
                    bodyText_Partial += Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "UPDATE [dbo].[Decia_ResultSetProcessingMember]" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L3 + "SET [Computation_EndDate] = GETDATE(), [Computation_Succeeded] = @vtgComputeResult" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L3 + string.Format("WHERE [VariableTemplateGroupId] = '{0}';", groupId) + Environment.NewLine;
                    bodyText_Partial += Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "SET @computeResult = (@computeResult & @vtgComputeResult);" + Environment.NewLine;
                    bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
                    bodyText_Partial += Environment.NewLine;
                }
                bodyText_Partial += GenericDatabaseUtils.Indent_L1 + "UPDATE [dbo].[Decia_ResultSet]" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "SET [Computation_EndDate] = GETDATE(), [Computation_Succeeded] = @computeResult" + Environment.NewLine;
                bodyText_Partial += GenericDatabaseUtils.Indent_L2 + "WHERE [Id] = @resultSetId;" + Environment.NewLine;
                var footerText_Partial = GetProcedureFooter(dbType);

                var procName_Full = Name + "Full";
                var headerText_Full = GetProcedureHeader(dbType, procName_Full, procParams_Full);
                var bodyText_Full = GenericDatabaseUtils.Indent_L1 + string.Format("EXEC [dbo].[{0}] {1};", procName_Partial, GetParameterText(dbType, procParams_Partial, false, procParams_Partial.Where(x => !procParams_Full.Contains(x)))) + Environment.NewLine;
                var footerText_Full = GetProcedureFooter(dbType);

                var procDef_Partial = headerText_Partial + bodyText_Partial + footerText_Partial + GenericDatabaseUtils.ScriptSpacer_Minor;
                var procDef_Full = headerText_Full + bodyText_Full + footerText_Full + GenericDatabaseUtils.ScriptSpacer_Minor;
                var procDef_VTGs = string.Empty;

                foreach (var subProcBucket in m_ComputationSubProcedures)
                {
                    var subProc = subProcBucket.Value;
                    procDef_VTGs += subProc.ExportProcedure(dbType);
                    procDef_VTGs += GenericDatabaseUtils.ScriptSpacer_Minor;
                }

                return procDef_VTGs + procDef_Partial + procDef_Full;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion
    }
}