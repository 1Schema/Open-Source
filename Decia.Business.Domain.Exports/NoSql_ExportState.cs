using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Exports;
using Decia.Business.Common.JsonSets;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.NoSql;
using Decia.Business.Common.NoSql.Base;
using Decia.Business.Common.Time;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.Models;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.Exports
{
    public class NoSql_ExportState
    {
        #region Constants

        public const long InitialChangeCount = Sql_ExportState.InitialChangeCount;
        public static DateTime InitialChangeDate { get { return Sql_ExportState.InitialChangeDate; } }
        public const double TimePeriod_MinMultiplier = Sql_ExportState.TimePeriod_MinMultiplier;
        public const double TimePeriod_MaxMultiplier = Sql_ExportState.TimePeriod_MaxMultiplier;

        #endregion

        #region Members

        private ModelDataProvider m_DataProvider;
        private Nullable<ModelInstanceId> m_ModelInstanceToExport;

        private ITimeDimension m_Timeframe;
        private SortedDictionary<int, IComputationGroup> m_OrderedGroups;
        private Dictionary<ModelObjectReference, IComputationGroup> m_VariableTemplateGroups;

        private DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_Metadata_JsonDocument> m_MetadataObject;
        private DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimeDimensionSetting_JsonDocument> m_TimeDimensionSettingObject;
        private SortedDictionary<DeciaDataType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_DataType_JsonDocument>> m_DataTypeObjects;
        private SortedDictionary<ModelObjectType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_ObjectType_JsonDocument>> m_ObjectTypeObjects;
        private SortedDictionary<TimePeriodType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriodType_JsonDocument>> m_TimePeriodTypeObjects;
        private Dictionary<ModelObjectReference, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument>> m_StructuralTypeObjects;
        private Dictionary<ModelObjectReference, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_VariableTemplate_JsonDocument>> m_VariableTemplateObjects;
        private SortedDictionary<Guid, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriod_JsonDocument>> m_TimePeriodObjects;
        private SortedDictionary<TimePeriodType, List<TimePeriod>> m_TimePeriodsByType;

        private Dictionary<ModelObjectReference, GenericCollection> m_StructuralTypeRootCollections;
        private Dictionary<ModelObjectReference, IDictionary<string, GenericCollection>> m_StructuralTypeCollections;
        private Dictionary<ModelObjectReference, GenericField> m_VariableTemplateFields;

        #endregion

        #region Constructors

        public NoSql_ExportState(ModelDataProvider dataProvider, Nullable<ModelInstanceId> modelInstanceToExport)
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
        public DeciaBase_JsonSet PrereqData { get; protected set; }
        public JsonSet ModelData { get; protected set; }

        public ITimeDimension Timeframe { get { return m_Timeframe; } }
        public DateTime StartDate { get { return m_Timeframe.FirstPeriodStartDate; } }
        public DateTime EndDate { get { return m_Timeframe.LastPeriodEndDate; } }

        #endregion

        #region Methods

        public void InitializeForExport(NoSqlDb_TargetType dbType, SortedDictionary<int, IComputationGroup> orderedGroups, string alternateDatabaseName)
        {
            Database_Name = (!string.IsNullOrWhiteSpace(alternateDatabaseName)) ? alternateDatabaseName : Database_Name;

            GenericDb = new GenericDatabase(Database_SqlName, true);
            PrereqData = new DeciaBase_JsonSet();
            ModelData = new JsonSet();

            m_Timeframe = (HasDataToExport) ? DataState.ModelInstances[ModelInstanceId].Timeframe : DataState.ModelTemplate.DefaultTimeframe;
            m_OrderedGroups = new SortedDictionary<int, IComputationGroup>(orderedGroups);
            m_VariableTemplateGroups = new Dictionary<ModelObjectReference, IComputationGroup>();
            foreach (var group in m_OrderedGroups.Values)
            {
                foreach (var variableTemplateRef in group.TimeOrderedNodes)
                { m_VariableTemplateGroups.Add(variableTemplateRef, group); }
            }

            m_MetadataObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_Metadata_JsonDocument>();
            m_MetadataObject[m_MetadataObject.SchemaDoc.ProjectId.Name] = RevisionChain.ProjectGuid;
            m_MetadataObject[m_MetadataObject.SchemaDoc.RevisionNumber.Name] = RevisionChain.DesiredRevisionNumber;
            m_MetadataObject[m_MetadataObject.SchemaDoc.ModelTemplateId.Name] = DataState.ModelTemplateRef.ModelObjectId;
            m_MetadataObject[m_MetadataObject.SchemaDoc.Name.Name] = Database_Name;
            m_MetadataObject[m_MetadataObject.SchemaDoc.MongoName.Name] = Database_SqlName;
            m_MetadataObject[m_MetadataObject.SchemaDoc.Description.Name] = Database_Description;
            m_MetadataObject[m_MetadataObject.SchemaDoc.ConciseRevisionNumber.Name] = DataState.GetConciseRevisionNumber(RevisionChain);
            m_MetadataObject[m_MetadataObject.SchemaDoc.Latest_ChangeCount.Name] = InitialChangeCount;
            m_MetadataObject[m_MetadataObject.SchemaDoc.Latest_ChangeDate.Name] = InitialChangeDate;

            m_TimeDimensionSettingObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_TimeDimensionSetting_JsonDocument>();
            m_TimeDimensionSettingObject[m_TimeDimensionSettingObject.SchemaDoc.Id.Name] = TimeDimensionTypeUtils.GetTimeDimensionNumberForType(TimeDimensionType.Primary);
            m_TimeDimensionSettingObject[m_TimeDimensionSettingObject.SchemaDoc.StartDate.Name] = StartDate;
            m_TimeDimensionSettingObject[m_TimeDimensionSettingObject.SchemaDoc.EndDate.Name] = EndDate;

            m_DataTypeObjects = new SortedDictionary<DeciaDataType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_DataType_JsonDocument>>();
            foreach (var dataType in EnumUtils.GetEnumValues<DeciaDataType>())
            {
                var dataTypeAsInt = (int)dataType;
                var dataTypeName = dataType.ToString();
                var dataTypeDescription = EnumUtils.GetDescription<DeciaDataType>(dataType, false);

                var dataTypeObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_DataType_JsonDocument>();
                dataTypeObject[dataTypeObject.SchemaDoc.Id.Name] = dataTypeAsInt;
                dataTypeObject[dataTypeObject.SchemaDoc.Name.Name] = dataTypeName;
                dataTypeObject[dataTypeObject.SchemaDoc.Description.Name] = dataTypeDescription;

                m_DataTypeObjects.Add(dataType, dataTypeObject);
            }

            m_ObjectTypeObjects = new SortedDictionary<ModelObjectType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_ObjectType_JsonDocument>>();
            foreach (var objectType in EnumUtils.GetEnumValues<ModelObjectType>())
            {
                if (!objectType.IsExportable())
                { continue; }

                var objectTypeAsInt = (int)objectType;
                var objectTypeName = objectType.ToString();
                var objectTypeDescription = EnumUtils.GetDescription<ModelObjectType>(objectType, false);

                var objectTypeObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_ObjectType_JsonDocument>();
                objectTypeObject[objectTypeObject.SchemaDoc.Id.Name] = objectTypeAsInt;
                objectTypeObject[objectTypeObject.SchemaDoc.Name.Name] = objectTypeName;
                objectTypeObject[objectTypeObject.SchemaDoc.Description.Name] = objectTypeDescription;

                m_ObjectTypeObjects.Add(objectType, objectTypeObject);
            }

            m_TimePeriodTypeObjects = new SortedDictionary<TimePeriodType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriodType_JsonDocument>>();
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

                var timePeriodTypeObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_TimePeriodType_JsonDocument>();
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.Id.Name] = timePeriodTypeAsInt;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.Name.Name] = timePeriodTypeName;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.Description.Name] = timePeriodTypeDescription;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.IsForever.Name] = isForever;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.EstimateInDays.Name] = estimateInDays;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.MinValidDays.Name] = minDays;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.MaxValidDays.Name] = maxDays;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.DatePart_Value.Name] = datePartValue;
                timePeriodTypeObject[timePeriodTypeObject.SchemaDoc.DatePart_Multiplier.Name] = datePartMultiplier;

                m_TimePeriodTypeObjects.Add(timePeriodType, timePeriodTypeObject);
            }

            m_StructuralTypeObjects = new Dictionary<ModelObjectReference, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument>>();
            m_VariableTemplateObjects = new Dictionary<ModelObjectReference, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_VariableTemplate_JsonDocument>>();
            m_TimePeriodObjects = new SortedDictionary<Guid, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriod_JsonDocument>>();
            m_TimePeriodsByType = new SortedDictionary<TimePeriodType, List<TimePeriod>>();

            m_StructuralTypeRootCollections = new Dictionary<ModelObjectReference, GenericCollection>();
            m_StructuralTypeCollections = new Dictionary<ModelObjectReference, IDictionary<string, GenericCollection>>();
            m_VariableTemplateFields = new Dictionary<ModelObjectReference, GenericField>();
        }

        public SortedDictionary<int, IComputationGroup> OrderedComputationGroups
        {
            get { return new SortedDictionary<int, IComputationGroup>(m_OrderedGroups); }
        }

        public IComputationGroup GetComputationGroup(ModelObjectReference variableTemplateRef)
        {
            return m_VariableTemplateGroups[variableTemplateRef];
        }

        public DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_DataType_JsonDocument> GetDataTypeObject(DeciaDataType dataType)
        {
            if (!m_DataTypeObjects.ContainsKey(dataType))
            { throw new InvalidOperationException("No object exists for Data Type."); }

            return m_DataTypeObjects[dataType];
        }

        public DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_ObjectType_JsonDocument> GetObjectTypeObject(ModelObjectType objectType)
        {
            if (!m_ObjectTypeObjects.ContainsKey(objectType))
            { throw new InvalidOperationException("No object exists for Object Type."); }

            return m_ObjectTypeObjects[objectType];
        }

        public DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriodType_JsonDocument> GetTimePeriodTypeObject(TimePeriodType timePeriodType)
        {
            if (!m_TimePeriodTypeObjects.ContainsKey(timePeriodType))
            { throw new InvalidOperationException("No object exists for Time Period Type."); }

            return m_TimePeriodTypeObjects[timePeriodType];
        }

        public void AddStructuralTypeObject(ModelObjectReference structuralTypeRef, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument> structuralTypeObject)
        {
            if (!PrereqData.StructuralType_Document.Instances.Contains(structuralTypeObject))
            { PrereqData.StructuralType_Document.AddInstance(structuralTypeObject); }

            m_StructuralTypeObjects.Add(structuralTypeRef, structuralTypeObject);
        }

        public DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument> GetStructuralTypeObject(ModelObjectReference structuralOrVariableTypeRef)
        {
            if (structuralOrVariableTypeRef.ModelObjectType == ModelObjectType.VariableTemplate)
            { structuralOrVariableTypeRef = DataProvider.DependencyMap.GetStructuralType(structuralOrVariableTypeRef); }

            if (!m_StructuralTypeObjects.ContainsKey(structuralOrVariableTypeRef))
            { throw new InvalidOperationException("No object exists for Structural Type."); }

            return m_StructuralTypeObjects[structuralOrVariableTypeRef];
        }

        public void AddVariableTemplateObject(ModelObjectReference variableTemplateRef, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_VariableTemplate_JsonDocument> variableTemplateObject)
        {
            if (!PrereqData.VariableTemplate_Document.Instances.Contains(variableTemplateObject))
            { PrereqData.VariableTemplate_Document.AddInstance(variableTemplateObject); }

            m_VariableTemplateObjects.Add(variableTemplateRef, variableTemplateObject);
        }

        public DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_VariableTemplate_JsonDocument> GetVariableTemplateObject(ModelObjectReference variableTemplateRef)
        {
            if (!m_VariableTemplateObjects.ContainsKey(variableTemplateRef))
            { throw new InvalidOperationException("No object exists for Variable Template."); }

            return m_VariableTemplateObjects[variableTemplateRef];
        }

        public void AddTimePeriodObject(TimePeriod timePeriod, TimePeriodType timePeriodType, DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriod_JsonDocument> timePeriodObject)
        {
            if (!PrereqData.TimePeriod_Document.Instances.Contains(timePeriodObject))
            { PrereqData.TimePeriod_Document.AddInstance(timePeriodObject); }

            m_TimePeriodObjects.Add(timePeriod.Id, timePeriodObject);

            if (!m_TimePeriodsByType.ContainsKey(timePeriodType))
            { m_TimePeriodsByType.Add(timePeriodType, new List<TimePeriod>()); }
            m_TimePeriodsByType[timePeriodType].Add(timePeriod);
        }

        public DeciaBase_JsonSet.Decia_JsonObject<DeciaBase_JsonSet.Decia_TimePeriod_JsonDocument> GetTimePeriodObject(Guid timePeriodId)
        {
            if (!m_TimePeriodObjects.ContainsKey(timePeriodId))
            { throw new InvalidOperationException("No object exists for Time Period."); }

            return m_TimePeriodObjects[timePeriodId];
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

        public void AddRootStructuralCollection(ModelObjectReference structuralTypeRef, GenericCollection collection)
        {
            var rootCollectionKey = Sql_ExportUtils.GenerateExportKey(null, null, false);

            if (m_StructuralTypeRootCollections.ContainsKey(structuralTypeRef))
            { throw new InvalidOperationException("Cannot add collection that has already been added."); }

            if (m_StructuralTypeCollections.ContainsKey(structuralTypeRef))
            {
                if (m_StructuralTypeCollections[structuralTypeRef].ContainsKey(rootCollectionKey))
                { throw new InvalidOperationException("Cannot add collection that has already been added."); }
            }
            else
            { m_StructuralTypeCollections.Add(structuralTypeRef, new Dictionary<string, GenericCollection>()); }

            m_StructuralTypeRootCollections.Add(structuralTypeRef, collection);
            m_StructuralTypeCollections[structuralTypeRef].Add(rootCollectionKey, collection);
        }

        public void AddNestedStructuralCollection(ModelObjectReference structuralTypeRef, string collectionKey, GenericCollection collection)
        {
            if (m_StructuralTypeCollections.ContainsKey(structuralTypeRef))
            {
                if (m_StructuralTypeCollections[structuralTypeRef].ContainsKey(collectionKey))
                { throw new InvalidOperationException("Cannot add collection that has already been added."); }
            }
            else
            { m_StructuralTypeCollections.Add(structuralTypeRef, new Dictionary<string, GenericCollection>()); }

            m_StructuralTypeCollections[structuralTypeRef].Add(collectionKey, collection);
        }

        public bool HasRootStructuralCollection(ModelObjectReference structuralTypeRef)
        {
            return m_StructuralTypeCollections.ContainsKey(structuralTypeRef);
        }

        public GenericCollection GetRootStructuralCollection(ModelObjectReference structuralTypeRef)
        {
            return m_StructuralTypeRootCollections[structuralTypeRef];
        }

        public bool HasStructuralCollection(ModelObjectReference structuralTypeRef, string collectionKey)
        {
            if (!m_StructuralTypeCollections.ContainsKey(structuralTypeRef))
            { return false; }
            if (!m_StructuralTypeCollections[structuralTypeRef].ContainsKey(collectionKey))
            { return false; }
            return true;
        }

        public GenericCollection GetStructuralCollection(ModelObjectReference structuralTypeRef, string collectionKey)
        {
            return m_StructuralTypeCollections[structuralTypeRef][collectionKey];
        }

        public bool HasVariableField(ModelObjectReference variableTemplateRef)
        {
            return m_VariableTemplateFields.ContainsKey(variableTemplateRef);
        }

        public void AddVariableField(ModelObjectReference variableTemplateRef, GenericField field)
        {
            if (m_VariableTemplateFields.ContainsKey(variableTemplateRef))
            { throw new InvalidOperationException("Cannot add field that has already been added."); }

            m_VariableTemplateFields.Add(variableTemplateRef, field);
        }

        public GenericField GetVariableField(ModelObjectReference variableTemplateRef)
        {
            return m_VariableTemplateFields[variableTemplateRef];
        }

        public GenericCollection GetVariableCollection(ModelObjectReference variableTemplateRef)
        {
            var field = m_VariableTemplateFields[variableTemplateRef];
            return field.ParentCollection;
        }

        #endregion
    }
}