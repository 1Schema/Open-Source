using System;
using System.Collections.Generic;
using System.Linq;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Sql.Programmatics
{
    public class ComputeVariableTemplateGroup_GenericProcedure : GenericProcedure
    {
        public const string NameFormat = "spDecia_Orchestration_ComputeGroup{0}_{1}";
        public const string InputParameterName_ResultSetId = "@resultSetId";
        public const string OutputParameterName_WasSuccessful = "@wasSuccessful";
        public const string TemporaryVariableName_TD1_TimePeriodId = "@TD1_TimePeriodId";
        public const string TemporaryVariableName_TD1_TimePeriodId_Min = "@TD1_TimePeriodId_Min";
        public const string TemporaryVariableName_TD1_TimePeriodId_Max = "@TD1_TimePeriodId_Max";
        public const string TemporaryVariableName_TD2_TimePeriodId = "@TD2_TimePeriodId";
        public const string TemporaryVariableName_TD2_TimePeriodId_Min = "@TD2_TimePeriodId_Min";
        public const string TemporaryVariableName_TD2_TimePeriodId_Max = "@TD2_TimePeriodId_Max";
        public const string TemporaryVariableName_TD1_TimePeriod_Cursor = "TD1_TimePeriod_Cursor";
        public const string TemporaryVariableName_TD2_TimePeriod_Cursor = "TD2_TimePeriod_Cursor";

        #region Members

        private OrchestrateComputation_GenericProcedure m_ParentProcedure;
        private Guid m_GroupId;
        private string m_GroupName;
        private int m_GroupIndex;
        private List<ModelObjectReference> m_IncludedVariableTemplateRefs;
        private GenericParameter m_InputParameter_ResultSetId;

        private Dictionary<ModelObjectReference, GenericColumn> m_VariableTemplateColumns;
        private Dictionary<ModelObjectReference, TimePeriodType?> m_VariableTemplateTD1TimePeriodTypes;
        private Dictionary<ModelObjectReference, TimePeriodType?> m_VariableTemplateTD2TimePeriodTypes;
        private Dictionary<ModelObjectReference, Func<SqlDb_TargetType, string, string>> m_ComputationMethodsByVariableTemplate;
        private SortedDictionary<int, Func<SqlDb_TargetType, string, string>> m_OrderedComputationMethods;

        #endregion

        #region Constructors

        public ComputeVariableTemplateGroup_GenericProcedure(OrchestrateComputation_GenericProcedure parentProcedure, Guid groupId, string groupName, int groupIndex, ICollection<ModelObjectReference> includedVariableTemplateRefs)
            : base(parentProcedure.ParentDatabase, string.Format(NameFormat, groupIndex.ToString("D4"), groupName))
        {
            m_ParentProcedure = parentProcedure;
            m_GroupId = groupId;
            m_GroupName = groupName;
            m_GroupIndex = groupIndex;
            m_IncludedVariableTemplateRefs = includedVariableTemplateRefs.ToList();

            m_InputParameter_ResultSetId = AddInputParameter(InputParameterName_ResultSetId, DeciaDataType.UniqueID);
            SetOutputParameter(OutputParameterName_WasSuccessful, DeciaDataType.Boolean);
            OutputParameter.DefaultValue = "0";

            m_VariableTemplateColumns = new Dictionary<ModelObjectReference, GenericColumn>();
            m_VariableTemplateTD1TimePeriodTypes = new Dictionary<ModelObjectReference, TimePeriodType?>();
            m_VariableTemplateTD2TimePeriodTypes = new Dictionary<ModelObjectReference, TimePeriodType?>();
            m_ComputationMethodsByVariableTemplate = new Dictionary<ModelObjectReference, Func<SqlDb_TargetType, string, string>>();
            m_OrderedComputationMethods = new SortedDictionary<int, Func<SqlDb_TargetType, string, string>>();
        }

        #endregion

        #region Properties

        public OrchestrateComputation_GenericProcedure ParentProcedure { get { return m_ParentProcedure; } }
        public Guid GroupId { get { return m_GroupId; } }
        public string GroupName { get { return m_GroupName; } }
        public ICollection<ModelObjectReference> IncludedVariableTemplateRefs
        {
            get
            {
                var result = new ReadOnlyList<ModelObjectReference>(m_IncludedVariableTemplateRefs);
                result.IsReadOnly = true;
                return result;
            }
        }
        public GenericParameter InputParameter_ResultSetId { get { return m_InputParameter_ResultSetId; } }

        public bool HasMultipleComputeMethods { get { return (m_ComputationMethodsByVariableTemplate.Count > 1); } }
        public Dictionary<ModelObjectReference, GenericColumn> VariableTemplateColumns { get { return new Dictionary<ModelObjectReference, GenericColumn>(m_VariableTemplateColumns); } }
        public Dictionary<ModelObjectReference, Func<SqlDb_TargetType, string, string>> ComputationMethodsByVariableTemplate { get { return new Dictionary<ModelObjectReference, Func<SqlDb_TargetType, string, string>>(m_ComputationMethodsByVariableTemplate); } }
        public SortedDictionary<int, Func<SqlDb_TargetType, string, string>> OrderedComputationMethods { get { return new SortedDictionary<int, Func<SqlDb_TargetType, string, string>>(m_OrderedComputationMethods); } }

        #endregion

        #region Methods

        public int AddComputeMethod(ModelObjectReference variableTemplateRef, TimePeriodType? td1TimePeriodType, TimePeriodType? td2TimePeriodType, GenericColumn variableTemplateColumn, Func<SqlDb_TargetType, string, string> computeMethod)
        {
            if (computeMethod == null)
            { throw new InvalidOperationException("Compute Method must not be null."); }

            m_VariableTemplateColumns.Add(variableTemplateRef, variableTemplateColumn);
            m_VariableTemplateTD1TimePeriodTypes.Add(variableTemplateRef, td1TimePeriodType);
            m_VariableTemplateTD2TimePeriodTypes.Add(variableTemplateRef, td2TimePeriodType);

            var index = (m_OrderedComputationMethods.Count + 1);
            m_ComputationMethodsByVariableTemplate.Add(variableTemplateRef, computeMethod);
            m_OrderedComputationMethods.Add(index, computeMethod);
            return index;
        }

        #endregion

        #region Export Methods

        public override string ExportProcedure(SqlDb_TargetType dbType)
        {
            var hasTD1Cursor = false;
            var hasTD2Cursor = false;

            if (dbType == SqlDb_TargetType.MsSqlServer)
            {
                var procName_VTG = Name;
                var procParams_VTG = new GenericParameter[] { InputParameter_ResultSetId, OutputParameter };
                var headerText_VTG = GetProcedureHeader(dbType, procName_VTG, procParams_VTG);

                var bodyText_VTG = string.Empty;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE @variableTemplateIds {0};", DeciaDataType.Text.ToNativeDataType(dbType)) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET @variableTemplateIds = '{0}';", m_IncludedVariableTemplateRefs.Select(x => x.ModelObjectId).ConvertToCollectionAsString(", ")) + Environment.NewLine;
                bodyText_VTG += Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} NVARCHAR(40);", TemporaryVariableName_TD1_TimePeriodId) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} NVARCHAR(40);", TemporaryVariableName_TD1_TimePeriodId_Min) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} NVARCHAR(40);", TemporaryVariableName_TD1_TimePeriodId_Max) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} NVARCHAR(40);", TemporaryVariableName_TD2_TimePeriodId) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} NVARCHAR(40);", TemporaryVariableName_TD2_TimePeriodId_Min) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} NVARCHAR(40);", TemporaryVariableName_TD2_TimePeriodId_Max) + Environment.NewLine;
                bodyText_VTG += Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = '%';", TemporaryVariableName_TD1_TimePeriodId) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = '%';", TemporaryVariableName_TD1_TimePeriodId_Min) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = '%';", TemporaryVariableName_TD1_TimePeriodId_Max) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = '%';", TemporaryVariableName_TD2_TimePeriodId) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = '%';", TemporaryVariableName_TD2_TimePeriodId_Min) + Environment.NewLine;
                bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = '%';", TemporaryVariableName_TD2_TimePeriodId_Max) + Environment.NewLine;
                bodyText_VTG += Environment.NewLine;

                var refWithMostGranularTD1 = GetRefForMostGranularTimePeriodType(m_VariableTemplateTD1TimePeriodTypes);
                var refWithMostGranularTD2 = GetRefForMostGranularTimePeriodType(m_VariableTemplateTD2TimePeriodTypes);

                if (refWithMostGranularTD1.HasValue)
                {
                    var mostGranularTD1_TptId = Convert.ToInt32(m_VariableTemplateTD1TimePeriodTypes[refWithMostGranularTD1.Value].Value);
                    var columnWithMostGranularTD1 = m_VariableTemplateColumns[refWithMostGranularTD1.Value];
                    var tableWithMostGranularTD1 = columnWithMostGranularTD1.ParentTable;
                    var td1IdColumn = tableWithMostGranularTD1.PrimaryKey.Columns.Where(x => x.TimeDimensionType == TimeDimensionType.Primary).First();

                    var baseQueryText = string.Empty;
                    baseQueryText += GenericDatabaseUtils.Indent_L2 + string.Format("FROM {0} itp", DeciaBaseUtils.Included_TimePeriod_TableName) + Environment.NewLine;
                    baseQueryText += GenericDatabaseUtils.Indent_L2 + string.Format("WHERE itp.[{0}] = {1}", DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName, mostGranularTD1_TptId) + Environment.NewLine;

                    bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = (SELECT TOP(1) itp.[{1}]", TemporaryVariableName_TD1_TimePeriodId_Min, DeciaBaseUtils.Included_TimePeriod_Id_ColumnName) + Environment.NewLine;
                    bodyText_VTG += baseQueryText;
                    bodyText_VTG += GenericDatabaseUtils.Indent_L2 + string.Format("ORDER BY itp.[{0}] ASC);", DeciaBaseUtils.Included_TimePeriod_OrderIndex_ColumnName) + Environment.NewLine;
                    bodyText_VTG += Environment.NewLine;

                    bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("SET {0} = (SELECT TOP(1) itp.[{1}]", TemporaryVariableName_TD1_TimePeriodId_Max, DeciaBaseUtils.Included_TimePeriod_Id_ColumnName) + Environment.NewLine;
                    bodyText_VTG += baseQueryText;
                    bodyText_VTG += GenericDatabaseUtils.Indent_L2 + string.Format("ORDER BY itp.[{0}] DESC);", DeciaBaseUtils.Included_TimePeriod_OrderIndex_ColumnName) + Environment.NewLine;
                    bodyText_VTG += Environment.NewLine;

                    if (HasMultipleComputeMethods)
                    {
                        hasTD1Cursor = true;

                        bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DECLARE {0} CURSOR FOR", TemporaryVariableName_TD1_TimePeriod_Cursor) + Environment.NewLine;
                        bodyText_VTG += GenericDatabaseUtils.Indent_L2 + string.Format("SELECT itp.[{0}]", DeciaBaseUtils.Included_TimePeriod_Id_ColumnName) + Environment.NewLine;
                        bodyText_VTG += baseQueryText;
                        bodyText_VTG += GenericDatabaseUtils.Indent_L2 + string.Format("ORDER BY itp.[{0}] ASC;", DeciaBaseUtils.Included_TimePeriod_OrderIndex_ColumnName) + Environment.NewLine;
                        bodyText_VTG += Environment.NewLine;

                        bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("OPEN {0};", TemporaryVariableName_TD1_TimePeriod_Cursor) + Environment.NewLine;
                        bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("FETCH NEXT FROM {0} INTO {1};", TemporaryVariableName_TD1_TimePeriod_Cursor, TemporaryVariableName_TD1_TimePeriodId) + Environment.NewLine;
                        bodyText_VTG += Environment.NewLine;

                        bodyText_VTG += "WHILE @@FETCH_STATUS = 0" + Environment.NewLine;
                        bodyText_VTG += "BEGIN" + Environment.NewLine;
                        bodyText_VTG += Environment.NewLine;
                    }
                }

                if (refWithMostGranularTD2.HasValue)
                {
                    var columnWithMostGranularTD2 = m_VariableTemplateColumns[refWithMostGranularTD2.Value];
                    var tableWithMostGranularTD2 = columnWithMostGranularTD2.ParentTable;
                    var td2IdColumn = tableWithMostGranularTD2.PrimaryKey.Columns.Where(x => x.TimeDimensionType == TimeDimensionType.Secondary).First();

                    if (HasMultipleComputeMethods)
                    {
                        hasTD2Cursor = true;
                    }
                }

                var baseIndent = GenericDatabaseUtils.Indent_L2;
                foreach (var indexBucket in OrderedComputationMethods)
                {
                    bodyText_VTG += indexBucket.Value(dbType, baseIndent);
                }

                if (hasTD1Cursor == true)
                {
                    bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("FETCH NEXT FROM {0} INTO {1};", TemporaryVariableName_TD1_TimePeriod_Cursor, TemporaryVariableName_TD1_TimePeriodId) + Environment.NewLine;
                    bodyText_VTG += GenericDatabaseUtils.Indent_L1 + "END;" + Environment.NewLine;
                    bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("CLOSE {0};", TemporaryVariableName_TD1_TimePeriod_Cursor) + Environment.NewLine;
                    bodyText_VTG += GenericDatabaseUtils.Indent_L1 + string.Format("DEALLOCATE {0};", TemporaryVariableName_TD1_TimePeriod_Cursor) + Environment.NewLine;
                    bodyText_VTG += Environment.NewLine;
                }

                var footerText_VTG = GetProcedureFooter(dbType);

                var procDef_VTG = headerText_VTG + bodyText_VTG + footerText_VTG + GenericDatabaseUtils.ScriptSpacer_Minor;
                return procDef_VTG;
            }
            else
            { throw new NotImplementedException("The specified Database Type is not supported yet."); }
        }

        #endregion

        #region Static Methods

        public static ModelObjectReference? GetRefForMostGranularTimePeriodType(IDictionary<ModelObjectReference, TimePeriodType?> timeGranularities)
        {
            ModelObjectReference? variableTemplateRef = null;
            TimePeriodType? timePeriodType = null;

            foreach (var timeGranularity in timeGranularities)
            {
                if (!timeGranularity.Value.HasValue)
                { continue; }

                if (!timePeriodType.HasValue)
                {
                    variableTemplateRef = timeGranularity.Key;
                    timePeriodType = timeGranularity.Value;
                }
                else if (timePeriodType.Value.CompareTimeTo(timeGranularity.Value.Value) == TimeComparisonResult.ThisIsLessGranular)
                {
                    variableTemplateRef = timeGranularity.Key;
                    timePeriodType = timeGranularity.Value;
                }
            }
            return variableTemplateRef;
        }

        #endregion
    }
}