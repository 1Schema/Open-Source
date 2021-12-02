using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Time;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.Models;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.Exports
{
    public class Sql_ExportState
    {
        #region Constants

        public const long InitialChangeCount = 0;
        public static DateTime InitialChangeDate { get { return DateTime.UtcNow; } }
        public const double TimePeriod_MinMultiplier = .85;
        public const double TimePeriod_MaxMultiplier = 1.15;

        #endregion

        #region Members

        private ModelDataProvider m_DataProvider;
        private Nullable<ModelInstanceId> m_ModelInstanceToExport;

        private ITimeDimension m_Timeframe;
        private SortedDictionary<int, IComputationGroup> m_OrderedGroups;
        private Dictionary<ModelObjectReference, IComputationGroup> m_VariableTemplateGroups;

        private DeciaBase_DataSet.Decia_MetadataRow m_MetadataRow;
        private DeciaBase_DataSet.Decia_TimeDimensionSettingRow m_TimeDimensionSettingRow;
        private SortedDictionary<DeciaDataType, DeciaBase_DataSet.Decia_DataTypeRow> m_DataTypeRows;
        private SortedDictionary<ModelObjectType, DeciaBase_DataSet.Decia_ObjectTypeRow> m_ObjectTypeRows;
        private SortedDictionary<TimePeriodType, DeciaBase_DataSet.Decia_TimePeriodTypeRow> m_TimePeriodTypeRows;
        private Dictionary<ModelObjectReference, DeciaBase_DataSet.Decia_StructuralTypeRow> m_StructuralTypeRows;
        private Dictionary<ModelObjectReference, DeciaBase_DataSet.Decia_VariableTemplateRow> m_VariableTemplateRows;
        private SortedDictionary<Guid, DeciaBase_DataSet.Decia_TimePeriodRow> m_TimePeriodRows;
        private SortedDictionary<TimePeriodType, List<TimePeriod>> m_TimePeriodsByType;

        private Dictionary<ModelObjectReference, GenericTable> m_StructuralTypeRootTables;
        private Dictionary<ModelObjectReference, IDictionary<string, GenericTable>> m_StructuralTypeTables;
        private Dictionary<ModelObjectReference, GenericColumn> m_VariableTemplateColumns;

        #endregion

        #region Constructors

        public Sql_ExportState(ModelDataProvider dataProvider, Nullable<ModelInstanceId> modelInstanceToExport)
        {
            m_DataProvider = dataProvider;
            m_ModelInstanceToExport = modelInstanceToExport;

            Database_Name = DataState.ModelTemplate.Name;
            Database_Description = Database_Name;

            var hasProjectMetadata = DataState.DomainModel.HasRepository<RevisionId, ProjectMetadata>();
            if (!hasProjectMetadata)
            { return; }

            var projectMetadataCount = DataState.DomainModel.ProjectMetadatas().Count();
            if (projectMetadataCount < 1)
            { return; }

            var metadata = DataState.DomainModel.ProjectMetadatas().ReadCurrentObject_ForDesiredRevision(RevisionChain);

            Database_Name = metadata.Name;
            Database_Description = metadata.Description;
        }

        #endregion

        #region Properties - Incoming

        public ModelDataProvider DataProvider { get { return m_DataProvider; } }
        public ModelDataState DataState { get { return DataProvider.DataState; } }
        public RevisionChain RevisionChain { get { return DataState.RevisionChain; } }
        public bool HasDataToExport { get { return m_ModelInstanceToExport.HasValue; } }
        public ModelInstanceId ModelInstanceId { get { return m_ModelInstanceToExport.Value; } }

        #endregion

        #region Properties - Outgoing

        public string Database_Name { get; protected set; }
        public string Database_SqlName { get { return Database_Name.ToEscaped_VarName(); } }
        public string Database_Description { get; protected set; }

        public GenericDatabase GenericDb { get; protected set; }
        public DeciaBase_DataSet PrereqData { get; protected set; }
        public DataSet ModelData { get; protected set; }

        public ITimeDimension Timeframe { get { return m_Timeframe; } }
        public DateTime StartDate { get { return m_Timeframe.FirstPeriodStartDate; } }
        public DateTime EndDate { get { return m_Timeframe.LastPeriodEndDate; } }

        #endregion

        #region Methods

        public void InitializeForExport(SqlDb_TargetType dbType, SortedDictionary<int, IComputationGroup> orderedGroups, string alternateDatabaseName)
        {
            Database_Name = (!string.IsNullOrWhiteSpace(alternateDatabaseName)) ? alternateDatabaseName : Database_Name;

            GenericDb = new GenericDatabase(Database_SqlName);
            PrereqData = new DeciaBase_DataSet();
            ModelData = new DataSet();

            m_Timeframe = (HasDataToExport) ? DataState.ModelInstances[ModelInstanceId].Timeframe : DataState.ModelTemplate.DefaultTimeframe;
            m_OrderedGroups = new SortedDictionary<int, IComputationGroup>(orderedGroups);
            m_VariableTemplateGroups = new Dictionary<ModelObjectReference, IComputationGroup>();
            foreach (var group in m_OrderedGroups.Values)
            {
                foreach (var variableTemplateRef in group.TimeOrderedNodes)
                { m_VariableTemplateGroups.Add(variableTemplateRef, group); }
            }

            m_MetadataRow = PrereqData.Decia_Metadata.NewDecia_MetadataRow();
            m_MetadataRow.ProjectId = RevisionChain.ProjectGuid;
            m_MetadataRow.RevisionNumber = RevisionChain.DesiredRevisionNumber;
            m_MetadataRow.ModelTemplateId = DataState.ModelTemplateRef.ModelObjectId;
            m_MetadataRow.Name = Database_Name;
            m_MetadataRow.SqlName = Database_SqlName;
            m_MetadataRow.Description = Database_Description;
            m_MetadataRow.ConciseRevisionNumber = DataState.GetConciseRevisionNumber(RevisionChain);
            m_MetadataRow.Latest_ChangeCount = InitialChangeCount;
            m_MetadataRow.Latest_ChangeDate = InitialChangeDate;
            PrereqData.Decia_Metadata.AddDecia_MetadataRow(m_MetadataRow);

            m_TimeDimensionSettingRow = PrereqData.Decia_TimeDimensionSetting.NewDecia_TimeDimensionSettingRow();
            m_TimeDimensionSettingRow.Id = TimeDimensionTypeUtils.GetTimeDimensionNumberForType(TimeDimensionType.Primary);
            m_TimeDimensionSettingRow.StartDate = StartDate;
            m_TimeDimensionSettingRow.EndDate = EndDate;
            PrereqData.Decia_TimeDimensionSetting.AddDecia_TimeDimensionSettingRow(m_TimeDimensionSettingRow);

            m_DataTypeRows = new SortedDictionary<DeciaDataType, DeciaBase_DataSet.Decia_DataTypeRow>();
            foreach (var dataType in EnumUtils.GetEnumValues<DeciaDataType>())
            {
                var dataTypeAsInt = (int)dataType;
                var dataTypeName = dataType.ToString();
                var dataTypeDescription = EnumUtils.GetDescription<DeciaDataType>(dataType, false);

                var dataTypeRow = PrereqData.Decia_DataType.AddDecia_DataTypeRow(dataTypeAsInt, dataTypeName, dataTypeDescription);
                m_DataTypeRows.Add(dataType, dataTypeRow);
            }

            m_ObjectTypeRows = new SortedDictionary<ModelObjectType, DeciaBase_DataSet.Decia_ObjectTypeRow>();
            foreach (var objectType in EnumUtils.GetEnumValues<ModelObjectType>())
            {
                if (!objectType.IsExportable())
                { continue; }

                var objectTypeAsInt = (int)objectType;
                var objectTypeName = objectType.ToString();
                var objectTypeDescription = EnumUtils.GetDescription<ModelObjectType>(objectType, false);

                var objectTypeRow = PrereqData.Decia_ObjectType.AddDecia_ObjectTypeRow(objectTypeAsInt, objectTypeName, objectTypeDescription);
                m_ObjectTypeRows.Add(objectType, objectTypeRow);
            }

            m_TimePeriodTypeRows = new SortedDictionary<TimePeriodType, DeciaBase_DataSet.Decia_TimePeriodTypeRow>();
            foreach (var timePeriodType in EnumUtils.GetEnumValues<TimePeriodType>())
            {
                var validationTimeframe = new TimeDimension(TimeDimensionType.Primary, TimeValueType.PeriodValue, timePeriodType, StartDate, StartDate.AddDays(1));
                var validationTimePeriods = validationTimeframe.GeneratePeriodsForTimeDimension();
                var validationTimePeriod = validationTimePeriods.First();
                var validationDuration = (validationTimePeriod.EndDate - validationTimePeriod.StartDate);

                var timePeriodTypeAsInt = (int)timePeriodType;
                var timePeriodTypeName = timePeriodType.ToString();
                var timePeriodTypeDescription = EnumUtils.GetDescription<TimePeriodType>(timePeriodType, false);
                var isForever = false;
                var estimateInDays = (timePeriodType != TimePeriodType.Days) ? validationDuration.Days : 1.0;
                var minDays = (timePeriodType != TimePeriodType.Days) ? (TimePeriod_MinMultiplier * estimateInDays) : 1.0;
                var maxDays = (timePeriodType != TimePeriodType.Days) ? (TimePeriod_MaxMultiplier * estimateInDays) : 1.0;
                string datePartValue;
                double datePartMultiplier;
                timePeriodType.GetDatePart(dbType, out datePartValue, out datePartMultiplier);

                var timePeriodTypeRow = PrereqData.Decia_TimePeriodType.AddDecia_TimePeriodTypeRow(timePeriodTypeAsInt, timePeriodTypeName, timePeriodTypeDescription, isForever, estimateInDays, minDays, maxDays, datePartValue, datePartMultiplier);
                m_TimePeriodTypeRows.Add(timePeriodType, timePeriodTypeRow);
            }

            m_StructuralTypeRows = new Dictionary<ModelObjectReference, DeciaBase_DataSet.Decia_StructuralTypeRow>();
            m_VariableTemplateRows = new Dictionary<ModelObjectReference, DeciaBase_DataSet.Decia_VariableTemplateRow>();
            m_TimePeriodRows = new SortedDictionary<Guid, DeciaBase_DataSet.Decia_TimePeriodRow>();
            m_TimePeriodsByType = new SortedDictionary<TimePeriodType, List<TimePeriod>>();

            m_StructuralTypeRootTables = new Dictionary<ModelObjectReference, GenericTable>();
            m_StructuralTypeTables = new Dictionary<ModelObjectReference, IDictionary<string, GenericTable>>();
            m_VariableTemplateColumns = new Dictionary<ModelObjectReference, GenericColumn>();
        }

        public SortedDictionary<int, IComputationGroup> OrderedComputationGroups
        {
            get { return new SortedDictionary<int, IComputationGroup>(m_OrderedGroups); }
        }

        public IComputationGroup GetComputationGroup(ModelObjectReference variableTemplateRef)
        {
            return m_VariableTemplateGroups[variableTemplateRef];
        }

        public DeciaBase_DataSet.Decia_DataTypeRow GetDataTypeRow(DeciaDataType dataType)
        {
            if (!m_DataTypeRows.ContainsKey(dataType))
            { throw new InvalidOperationException("No row exists for Data Type."); }
            return m_DataTypeRows[dataType];
        }

        public DeciaBase_DataSet.Decia_ObjectTypeRow GetObjectTypeRow(ModelObjectType objectType)
        {
            if (!m_ObjectTypeRows.ContainsKey(objectType))
            { throw new InvalidOperationException("No row exists for Object Type."); }
            return m_ObjectTypeRows[objectType];
        }

        public DeciaBase_DataSet.Decia_TimePeriodTypeRow GetTimePeriodTypeRow(TimePeriodType timePeriodType)
        {
            if (!m_TimePeriodTypeRows.ContainsKey(timePeriodType))
            { throw new InvalidOperationException("No row exists for Time Period Type."); }
            return m_TimePeriodTypeRows[timePeriodType];
        }

        public void AddStructuralTypeRow(ModelObjectReference structuralTypeRef, DeciaBase_DataSet.Decia_StructuralTypeRow structuralTypeRow)
        {
            PrereqData.Decia_StructuralType.AddDecia_StructuralTypeRow(structuralTypeRow);
            m_StructuralTypeRows.Add(structuralTypeRef, structuralTypeRow);
        }

        public DeciaBase_DataSet.Decia_StructuralTypeRow GetStructuralTypeRow(ModelObjectReference structuralOrVariableTypeRef)
        {
            if (structuralOrVariableTypeRef.ModelObjectType == ModelObjectType.VariableTemplate)
            { structuralOrVariableTypeRef = DataProvider.DependencyMap.GetStructuralType(structuralOrVariableTypeRef); }

            if (!m_StructuralTypeRows.ContainsKey(structuralOrVariableTypeRef))
            { throw new InvalidOperationException("No row exists for Structural Type."); }

            return m_StructuralTypeRows[structuralOrVariableTypeRef];
        }

        public void AddVariableTemplateRow(ModelObjectReference variableTemplateRef, DeciaBase_DataSet.Decia_VariableTemplateRow variableTemplateRow)
        {
            PrereqData.Decia_VariableTemplate.AddDecia_VariableTemplateRow(variableTemplateRow);
            m_VariableTemplateRows.Add(variableTemplateRef, variableTemplateRow);
        }

        public DeciaBase_DataSet.Decia_VariableTemplateRow GetVariableTemplateRow(ModelObjectReference variableTemplateRef)
        {
            if (!m_VariableTemplateRows.ContainsKey(variableTemplateRef))
            { throw new InvalidOperationException("No row exists for Variable Template."); }
            return m_VariableTemplateRows[variableTemplateRef];
        }

        public void AddTimePeriodRow(TimePeriod timePeriod, TimePeriodType timePeriodType, DeciaBase_DataSet.Decia_TimePeriodRow timePeriodRow)
        {
            PrereqData.Decia_TimePeriod.AddDecia_TimePeriodRow(timePeriodRow);
            m_TimePeriodRows.Add(timePeriod.Id, timePeriodRow);

            if (!m_TimePeriodsByType.ContainsKey(timePeriodType))
            { m_TimePeriodsByType.Add(timePeriodType, new List<TimePeriod>()); }
            m_TimePeriodsByType[timePeriodType].Add(timePeriod);
        }

        public DeciaBase_DataSet.Decia_TimePeriodRow GetTimePeriodRow(Guid timePeriodId)
        {
            if (!m_TimePeriodRows.ContainsKey(timePeriodId))
            { throw new InvalidOperationException("No row exists for Time Period."); }
            return m_TimePeriodRows[timePeriodId];
        }

        public ICollection<TimePeriodType> SupportedTimePeriodTypes
        {
            get
            {
                var timePeriodTypes = new ReadOnlyList<TimePeriodType>(m_TimePeriodsByType.Keys);
                timePeriodTypes.IsReadOnly = true;
                return timePeriodTypes;
            }
        }

        public ICollection<TimePeriod> GetTimePeriodsByType(TimePeriodType timePeriodType)
        {
            if (!m_TimePeriodsByType.ContainsKey(timePeriodType))
            { throw new InvalidOperationException("No Time Periods exists for Time Period Type."); }

            var timePeriods = new ReadOnlyList<TimePeriod>(m_TimePeriodsByType[timePeriodType]);
            timePeriods.IsReadOnly = true;
            return timePeriods;
        }

        public void AddRootStructuralTable(ModelObjectReference structuralTypeRef, GenericTable table)
        {
            var rootTableKey = Sql_ExportUtils.GenerateExportKey(null, null, false);

            if (m_StructuralTypeRootTables.ContainsKey(structuralTypeRef))
            { throw new InvalidOperationException("Cannot add table that has already been added."); }

            if (m_StructuralTypeTables.ContainsKey(structuralTypeRef))
            {
                if (m_StructuralTypeTables[structuralTypeRef].ContainsKey(rootTableKey))
                { throw new InvalidOperationException("Cannot add table that has already been added."); }
            }
            else
            { m_StructuralTypeTables.Add(structuralTypeRef, new Dictionary<string, GenericTable>()); }

            m_StructuralTypeRootTables.Add(structuralTypeRef, table);
            m_StructuralTypeTables[structuralTypeRef].Add(rootTableKey, table);
        }

        public void AddNestedStructuralTable(ModelObjectReference structuralTypeRef, string tableKey, GenericTable table)
        {
            if (m_StructuralTypeTables.ContainsKey(structuralTypeRef))
            {
                if (m_StructuralTypeTables[structuralTypeRef].ContainsKey(tableKey))
                { throw new InvalidOperationException("Cannot add table that has already been added."); }
            }
            else
            { m_StructuralTypeTables.Add(structuralTypeRef, new Dictionary<string, GenericTable>()); }

            m_StructuralTypeTables[structuralTypeRef].Add(tableKey, table);
        }

        public bool HasRootStructuralTable(ModelObjectReference structuralTypeRef)
        {
            return m_StructuralTypeTables.ContainsKey(structuralTypeRef);
        }

        public GenericTable GetRootStructuralTable(ModelObjectReference structuralTypeRef)
        {
            return m_StructuralTypeRootTables[structuralTypeRef];
        }

        public bool HasStructuralTable(ModelObjectReference structuralTypeRef, string tableKey)
        {
            if (!m_StructuralTypeTables.ContainsKey(structuralTypeRef))
            { return false; }
            if (!m_StructuralTypeTables[structuralTypeRef].ContainsKey(tableKey))
            { return false; }
            return true;
        }

        public GenericTable GetStructuralTable(ModelObjectReference structuralTypeRef, string tableKey)
        {
            return m_StructuralTypeTables[structuralTypeRef][tableKey];
        }

        public bool HasVariableColumn(ModelObjectReference variableTemplateRef)
        {
            return m_VariableTemplateColumns.ContainsKey(variableTemplateRef);
        }

        public void AddVariableColumn(ModelObjectReference variableTemplateRef, GenericColumn column)
        {
            if (m_VariableTemplateColumns.ContainsKey(variableTemplateRef))
            { throw new InvalidOperationException("Cannot add column that has already been added."); }

            m_VariableTemplateColumns.Add(variableTemplateRef, column);
        }

        public GenericColumn GetVariableColumn(ModelObjectReference variableTemplateRef)
        {
            return m_VariableTemplateColumns[variableTemplateRef];
        }

        public GenericTable GetVariableTable(ModelObjectReference variableTemplateRef)
        {
            var column = m_VariableTemplateColumns[variableTemplateRef];
            return column.ParentTable;
        }

        #endregion
    }
}