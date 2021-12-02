using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Models;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.Exports
{
    public class NoSql_Exporter
    {
        #region Constants

        public const string ChangeCount_ResetCommand = "fnDecia_ChangeState_ResetLatest();";
        public const long InitialChangeCount = Sql_Exporter.InitialChangeCount;
        public static DateTime InitialChangeDate { get { return Sql_Exporter.InitialChangeDate; } }
        public const string TimePeriodId_DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public const double TimePeriod_MinMultiplier = Sql_Exporter.TimePeriod_MinMultiplier;
        public const double TimePeriod_MaxMultiplier = Sql_Exporter.TimePeriod_MaxMultiplier;

        public static string GetTimePeriodId(TimePeriod timePeriod)
        { return GetTimePeriodId(timePeriod.StartDate, timePeriod.EndDate); }

        public static string GetTimePeriodId(DateTime startDate, DateTime endDate)
        { return (startDate.ToString(TimePeriodId_DateFormat) + "_" + endDate.ToString(TimePeriodId_DateFormat)); }

        #endregion

        #region Members

        private NoSql_ExportState m_ExportState;

        #endregion

        #region Constructors

        public NoSql_Exporter(ModelDataProvider dataProvider, Nullable<ModelInstanceId> modelInstanceToExport)
        {
            m_ExportState = new NoSql_ExportState(dataProvider, modelInstanceToExport);
        }

        #endregion

        #region Properties

        public NoSql_ExportState ExportState { get { return m_ExportState; } }
        public ModelDataProvider DataProvider { get { return ExportState.DataProvider; } }
        public ModelDataState DataState { get { return DataProvider.DataState; } }
        public RevisionChain RevisionChain { get { return DataState.RevisionChain; } }
        public bool HasDataToExport { get { return ExportState.HasDataToExport; } }
        public ModelInstanceId ModelInstanceId { get { return ExportState.ModelInstanceId; } }

        public GenericDatabase GenericDb { get { return ExportState.GenericDb; } }
        public DeciaBase_JsonSet PrereqData { get { return ExportState.PrereqData; } }
        public JsonSet ModelData { get { return ExportState.ModelData; } }

        #endregion

        #region Methods

        public string ExportToScript(NoSqlDb_TargetType dbType)
        {
            var script = ExportToScript(dbType, null);
            return script;
        }

        public string ExportToScript(NoSqlDb_TargetType dbType, string alternateDatabaseName)
        {
            var engine = new FormulaProcessingEngine(DataProvider);
            engine.SetInitializationInputs(DataState.ModelTemplateRef);
            var initializationResult = engine.Initialize();

            if (!initializationResult.IsValid)
            { throw new InvalidOperationException("Initialization failed."); }
            if (engine.ComputationGroupsWithRealCycles.Count > 0)
            { throw new InvalidOperationException("The Model provided has Cycles."); }

            engine.SetValidationInputs(DataState.GetModelTemplateState());
            var validationResult = engine.Validate();

            if (!validationResult.IsValid)
            { throw new InvalidOperationException("Validation failed."); }

            var orderedGroups = new SortedDictionary<int, IComputationGroup>(engine.OrderedComputationGroups);
            ExportState.InitializeForExport(dbType, orderedGroups, alternateDatabaseName);

            var usedTimePeriodTypes = new HashSet<TimePeriodType>();
            var remainingVariableTemplatesByCollection = new Dictionary<GenericCollection, List<VariableTemplate>>();


            if (true)
            {
                var globalType = DataState.GlobalType;
                var globalTypeName = globalType.Name.ToEscaped_VarName();
                var globalTypeRef = globalType.ModelObjectRef;
                var globalType_VariableTemplates = DataState.GetVariableTemplates(globalTypeRef, null).Values.OrderBy(x => x.OrderNumber).ToList();

                AddTimePeriodTypesUsed(usedTimePeriodTypes, globalType_VariableTemplates);

                var idVariableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(globalTypeRef);
                var idVariableTemplate = DataState.VariableTemplatesByRef[idVariableTemplateRef];
                var nameVariableTemplateRef = DataProvider.DependencyMap.GetNameVariableTemplate(globalTypeRef);
                var nameVariableTemplate = DataState.VariableTemplatesByRef[nameVariableTemplateRef];
                var sortingVariableTemplateRef = DataProvider.DependencyMap.GetOrderVariableTemplate(globalTypeRef);
                var sortingVariableTemplate = DataState.VariableTemplatesByRef[sortingVariableTemplateRef];

                var globalTypeObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument>();
                globalTypeObject[globalTypeObject.SchemaDoc.Id.Name] = globalType.ModelObjectId;
                globalTypeObject[globalTypeObject.SchemaDoc.Name.Name] = globalType.Name;
                globalTypeObject[globalTypeObject.SchemaDoc.MongoName.Name] = globalTypeName;
                globalTypeObject[globalTypeObject.SchemaDoc.ObjectTypeId.Name] = (int)globalType.ModelObjectType;
                globalTypeObject[globalTypeObject.SchemaDoc.TreeLevel_Basic.Name] = DataProvider.StructuralMap.GetDistanceFromGlobal(globalTypeRef, false);
                globalTypeObject[globalTypeObject.SchemaDoc.TreeLevel_Extended.Name] = DataProvider.StructuralMap.GetDistanceFromGlobal(globalTypeRef, true);
                globalTypeObject[globalTypeObject.SchemaDoc.Parent_StructuralTypeId.Name] = null;
                globalTypeObject[globalTypeObject.SchemaDoc.Parent_IsNullable.Name] = false;
                globalTypeObject[globalTypeObject.SchemaDoc.Parent_Default_InstanceId.Name] = null;
                globalTypeObject[globalTypeObject.SchemaDoc.Instance_Collection_Name.Name] = globalTypeName.ToPluralized(false);
                globalTypeObject[globalTypeObject.SchemaDoc.Instance_Id_VariableTemplateId.Name] = idVariableTemplateRef.ModelObjectId;
                globalTypeObject[globalTypeObject.SchemaDoc.Instance_Name_VariableTemplateId.Name] = nameVariableTemplateRef.ModelObjectId;
                globalTypeObject[globalTypeObject.SchemaDoc.Instance_Sorting_VariableTemplateId.Name] = sortingVariableTemplateRef.ModelObjectId;
                globalTypeObject[globalTypeObject.SchemaDoc.Instance_ParentId_VariableTemplateId.Name] = null;

                ExportState.AddStructuralTypeObject(globalTypeRef, globalTypeObject);
                FillWithVariableTemplates(DataProvider, ExportState, globalTypeRef, globalType_VariableTemplates);


                var global_Clctn = GenericDb.CreateCollection(globalTypeName, globalTypeRef);
                ExportState.AddRootStructuralCollection(globalTypeRef, global_Clctn);

                var global_IdObject = ExportState.GetVariableTemplateObject(idVariableTemplateRef);
                var global_IdName = global_IdObject.GetValue(global_IdObject.SchemaDoc.MongoName.Name).ToString();
                var global_IdField = global_Clctn.CreateLocalField(global_IdName, idVariableTemplate.DataType, idVariableTemplate.GetTimeDimensionality(), idVariableTemplate.PredefinedVariableTemplateOption);
                global_IdField.SetVariableInfo(idVariableTemplateRef, globalTypeRef, true);
                global_Clctn.SetLocalField_ForId(global_IdField.Id);
                globalType_VariableTemplates.Remove(idVariableTemplate);
                ExportState.AddVariableField(idVariableTemplateRef, global_IdField);

                var global_NameObject = ExportState.GetVariableTemplateObject(nameVariableTemplateRef);
                var global_NameName = global_NameObject.GetValue(global_NameObject.SchemaDoc.MongoName.Name).ToString();
                var global_NameField = global_Clctn.CreateLocalField(global_NameName, nameVariableTemplate.DataType, nameVariableTemplate.GetTimeDimensionality(), nameVariableTemplate.PredefinedVariableTemplateOption);
                global_NameField.SetVariableInfo(nameVariableTemplateRef);
                global_Clctn.SetLocalField_ForName(global_NameField.Id);
                globalType_VariableTemplates.Remove(nameVariableTemplate);
                ExportState.AddVariableField(nameVariableTemplateRef, global_NameField);

                var global_SortingObject = ExportState.GetVariableTemplateObject(sortingVariableTemplateRef);
                var global_SortingName = global_SortingObject.GetValue(global_SortingObject.SchemaDoc.MongoName.Name).ToString();
                var global_SortingField = global_Clctn.CreateLocalField(global_SortingName, sortingVariableTemplate.DataType, sortingVariableTemplate.GetTimeDimensionality(), sortingVariableTemplate.PredefinedVariableTemplateOption);
                global_SortingField.SetVariableInfo(sortingVariableTemplateRef);
                global_Clctn.SetLocalField_ForSorting(global_SortingField.Id);
                globalType_VariableTemplates.Remove(sortingVariableTemplate);
                ExportState.AddVariableField(sortingVariableTemplateRef, global_SortingField);

                remainingVariableTemplatesByCollection.Add(global_Clctn, globalType_VariableTemplates);
            }

            var orderedEntityTypeRefs = DataProvider.StructuralMap.EntityTypeExtendedNetwork.GetDependencyOrderedTraversalFromRoot();
            foreach (var entityTypeRef in orderedEntityTypeRefs)
            {
                var entityType = DataState.EntityTypesByRef[entityTypeRef];
                var entityTypeName = entityType.Name.ToEscaped_VarName();
                var entityType_VariableTemplates = DataState.GetVariableTemplates(entityTypeRef, null).Values.OrderBy(x => x.OrderNumber).ToList();

                AddTimePeriodTypesUsed(usedTimePeriodTypes, entityType_VariableTemplates);

                var idVariableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(entityTypeRef);
                var idVariableTemplate = DataState.VariableTemplatesByRef[idVariableTemplateRef];
                var nameVariableTemplateRef = DataProvider.DependencyMap.GetNameVariableTemplate(entityTypeRef);
                var nameVariableTemplate = DataState.VariableTemplatesByRef[nameVariableTemplateRef];
                var sortingVariableTemplateRef = DataProvider.DependencyMap.GetOrderVariableTemplate(entityTypeRef);
                var sortingVariableTemplate = DataState.VariableTemplatesByRef[sortingVariableTemplateRef];
                var parentIdVariableTemplate = entityType_VariableTemplates.Where(x => (x.PredefinedVariableTemplateOption == PredefinedVariableTemplateOption.Id_Parent)).FirstOrDefault();
                var hasParentEntityType = (parentIdVariableTemplate != null);
                var parentStructuralTypeRef = (hasParentEntityType) ? entityType.Parent_EntityTypeId.Value.ModelObjectRef : (ModelObjectReference?)null;
                var parentIdVariableTemplateRef = (hasParentEntityType) ? parentIdVariableTemplate.ModelObjectRef : (ModelObjectReference?)null;

                if (hasParentEntityType != entityType.Parent_EntityTypeNumber.HasValue)
                { throw new InvalidOperationException("The Entity Type definition does not match its contained Variable Templates."); }

                var entityTypeObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument>();
                entityTypeObject[entityTypeObject.SchemaDoc.Id.Name] = entityType.ModelObjectId;
                entityTypeObject[entityTypeObject.SchemaDoc.Name.Name] = entityType.Name;
                entityTypeObject[entityTypeObject.SchemaDoc.MongoName.Name] = entityTypeName;
                entityTypeObject[entityTypeObject.SchemaDoc.ObjectTypeId.Name] = (int)entityType.ModelObjectType;
                entityTypeObject[entityTypeObject.SchemaDoc.TreeLevel_Basic.Name] = DataProvider.StructuralMap.GetDistanceFromGlobal(entityTypeRef, false);
                entityTypeObject[entityTypeObject.SchemaDoc.TreeLevel_Extended.Name] = DataProvider.StructuralMap.GetDistanceFromGlobal(entityTypeRef, true);
                entityTypeObject[entityTypeObject.SchemaDoc.Parent_StructuralTypeId.Name] = null;
                entityTypeObject[entityTypeObject.SchemaDoc.Parent_IsNullable.Name] = false;
                entityTypeObject[entityTypeObject.SchemaDoc.Parent_Default_InstanceId.Name] = null;
                entityTypeObject[entityTypeObject.SchemaDoc.Instance_Collection_Name.Name] = entityTypeName.ToPluralized(false);
                entityTypeObject[entityTypeObject.SchemaDoc.Instance_Id_VariableTemplateId.Name] = idVariableTemplateRef.ModelObjectId;
                entityTypeObject[entityTypeObject.SchemaDoc.Instance_Name_VariableTemplateId.Name] = nameVariableTemplateRef.ModelObjectId;
                entityTypeObject[entityTypeObject.SchemaDoc.Instance_Sorting_VariableTemplateId.Name] = sortingVariableTemplateRef.ModelObjectId;
                entityTypeObject[entityTypeObject.SchemaDoc.Instance_ParentId_VariableTemplateId.Name] = null;

                var parent_Clctn = (GenericCollection)null;
                if (hasParentEntityType)
                {
                    parent_Clctn = ExportState.GetRootStructuralCollection(parentStructuralTypeRef.Value);

                    entityTypeObject[entityTypeObject.SchemaDoc.Parent_StructuralTypeId.Name] = parentStructuralTypeRef.Value.ModelObjectId;
                    entityTypeObject[entityTypeObject.SchemaDoc.Parent_IsNullable.Name] = entityType.Parent_IsNullable;
                    entityTypeObject[entityTypeObject.SchemaDoc.Instance_ParentId_VariableTemplateId.Name] = parentIdVariableTemplateRef.Value.ModelObjectId;
                }

                ExportState.AddStructuralTypeObject(entityTypeRef, entityTypeObject);
                FillWithVariableTemplates(DataProvider, ExportState, entityTypeRef, entityType_VariableTemplates);


                var entity_Clctn = GenericDb.CreateCollection(entityTypeName, entityTypeRef, parent_Clctn);
                ExportState.AddRootStructuralCollection(entityTypeRef, entity_Clctn);

                var entity_IdObject = ExportState.GetVariableTemplateObject(idVariableTemplateRef);
                var entity_IdName = entity_IdObject.GetValue(entity_IdObject.SchemaDoc.MongoName.Name).ToString();
                var entity_IdField = entity_Clctn.CreateLocalField(entity_IdName, idVariableTemplate.DataType, idVariableTemplate.GetTimeDimensionality(), idVariableTemplate.PredefinedVariableTemplateOption);
                entity_IdField.SetVariableInfo(idVariableTemplateRef, entityTypeRef, true);
                entity_Clctn.SetLocalField_ForId(entity_IdField.Id);
                entityType_VariableTemplates.Remove(idVariableTemplate);
                ExportState.AddVariableField(idVariableTemplateRef, entity_IdField);

                var entity_NameObject = ExportState.GetVariableTemplateObject(nameVariableTemplateRef);
                var entity_NameName = entity_NameObject.GetValue(entity_NameObject.SchemaDoc.MongoName.Name).ToString();
                var entity_NameField = entity_Clctn.CreateLocalField(entity_NameName, nameVariableTemplate.DataType, nameVariableTemplate.GetTimeDimensionality(), nameVariableTemplate.PredefinedVariableTemplateOption);
                entity_NameField.SetVariableInfo(nameVariableTemplateRef);
                entity_Clctn.SetLocalField_ForName(entity_NameField.Id);
                entityType_VariableTemplates.Remove(nameVariableTemplate);
                ExportState.AddVariableField(nameVariableTemplateRef, entity_NameField);

                var entity_SortingObject = ExportState.GetVariableTemplateObject(sortingVariableTemplateRef);
                var entity_SortingName = entity_SortingObject.GetValue(entity_SortingObject.SchemaDoc.MongoName.Name).ToString();
                var entity_SortingField = entity_Clctn.CreateLocalField(entity_SortingName, sortingVariableTemplate.DataType, sortingVariableTemplate.GetTimeDimensionality(), sortingVariableTemplate.PredefinedVariableTemplateOption);
                entity_SortingField.SetVariableInfo(sortingVariableTemplateRef);
                entity_Clctn.SetLocalField_ForSorting(entity_SortingField.Id);
                entityType_VariableTemplates.Remove(sortingVariableTemplate);
                ExportState.AddVariableField(sortingVariableTemplateRef, entity_SortingField);

                remainingVariableTemplatesByCollection.Add(entity_Clctn, entityType_VariableTemplates);
            }

            var orderedRelationTypes = DataState.RelationTypes.Values.OrderBy(x => x.OrderNumber);
            foreach (var relationType in orderedRelationTypes)
            {
                var relationTypeRef = relationType.ModelObjectRef;
                var relationTypeName = relationType.Name.ToEscaped_VarName();
                var relationType_VariableTemplates = DataState.GetVariableTemplates(relationTypeRef, null).Values.OrderBy(x => x.OrderNumber).ToList();

                AddTimePeriodTypesUsed(usedTimePeriodTypes, relationType_VariableTemplates);

                var idVariableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(relationTypeRef);
                var idVariableTemplate = DataState.VariableTemplatesByRef[idVariableTemplateRef];
                var nameVariableTemplateRef = DataProvider.DependencyMap.GetNameVariableTemplate(relationTypeRef);
                var nameVariableTemplate = DataState.VariableTemplatesByRef[nameVariableTemplateRef];
                var sortingVariableTemplateRef = DataProvider.DependencyMap.GetOrderVariableTemplate(relationTypeRef);
                var sortingVariableTemplate = DataState.VariableTemplatesByRef[sortingVariableTemplateRef];

                var relationTypeObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_StructuralType_JsonDocument>();
                relationTypeObject[relationTypeObject.SchemaDoc.Id.Name] = relationType.ModelObjectId;
                relationTypeObject[relationTypeObject.SchemaDoc.Name.Name] = relationType.Name;
                relationTypeObject[relationTypeObject.SchemaDoc.MongoName.Name] = relationTypeName;
                relationTypeObject[relationTypeObject.SchemaDoc.ObjectTypeId.Name] = (int)relationType.ModelObjectType;
                relationTypeObject[relationTypeObject.SchemaDoc.TreeLevel_Basic.Name] = DataProvider.StructuralMap.GetDistanceFromGlobal(relationTypeRef, false);
                relationTypeObject[relationTypeObject.SchemaDoc.TreeLevel_Extended.Name] = DataProvider.StructuralMap.GetDistanceFromGlobal(relationTypeRef, true);
                relationTypeObject[relationTypeObject.SchemaDoc.Parent_StructuralTypeId.Name] = null;
                relationTypeObject[relationTypeObject.SchemaDoc.Parent_IsNullable.Name] = false;
                relationTypeObject[relationTypeObject.SchemaDoc.Parent_Default_InstanceId.Name] = null;
                relationTypeObject[relationTypeObject.SchemaDoc.Instance_Collection_Name.Name] = relationTypeName.ToPluralized(true);
                relationTypeObject[relationTypeObject.SchemaDoc.Instance_Id_VariableTemplateId.Name] = idVariableTemplateRef.ModelObjectId;
                relationTypeObject[relationTypeObject.SchemaDoc.Instance_Name_VariableTemplateId.Name] = nameVariableTemplateRef.ModelObjectId;
                relationTypeObject[relationTypeObject.SchemaDoc.Instance_Sorting_VariableTemplateId.Name] = sortingVariableTemplateRef.ModelObjectId;
                relationTypeObject[relationTypeObject.SchemaDoc.Instance_ParentId_VariableTemplateId.Name] = null;

                ExportState.AddStructuralTypeObject(relationTypeRef, relationTypeObject);
                FillWithVariableTemplates(DataProvider, ExportState, relationTypeRef, relationType_VariableTemplates);


                var relation_Clctn = GenericDb.CreateCollection(relationTypeName, relationTypeRef);
                ExportState.AddRootStructuralCollection(relationTypeRef, relation_Clctn);

                var relation_IdObject = ExportState.GetVariableTemplateObject(idVariableTemplateRef);
                var relation_IdName = relation_IdObject.GetValue(relation_IdObject.SchemaDoc.MongoName.Name).ToString();
                var relation_IdField = relation_Clctn.CreateLocalField(relation_IdName, idVariableTemplate.DataType, idVariableTemplate.GetTimeDimensionality(), idVariableTemplate.PredefinedVariableTemplateOption);
                relation_IdField.SetVariableInfo(idVariableTemplateRef, relationTypeRef, true);
                relation_Clctn.SetLocalField_ForId(relation_IdField.Id);
                relationType_VariableTemplates.Remove(idVariableTemplate);
                ExportState.AddVariableField(idVariableTemplateRef, relation_IdField);

                var relation_NameObject = ExportState.GetVariableTemplateObject(nameVariableTemplateRef);
                var relation_NameName = relation_NameObject.GetValue(relation_NameObject.SchemaDoc.MongoName.Name).ToString();
                var relation_NameField = relation_Clctn.CreateLocalField(relation_NameName, nameVariableTemplate.DataType, nameVariableTemplate.GetTimeDimensionality(), nameVariableTemplate.PredefinedVariableTemplateOption);
                relation_NameField.SetVariableInfo(nameVariableTemplateRef);
                relation_Clctn.SetLocalField_ForName(relation_NameField.Id);
                relationType_VariableTemplates.Remove(nameVariableTemplate);
                ExportState.AddVariableField(nameVariableTemplateRef, relation_NameField);

                var relation_SortingObject = ExportState.GetVariableTemplateObject(sortingVariableTemplateRef);
                var relation_SortingName = relation_SortingObject.GetValue(relation_SortingObject.SchemaDoc.MongoName.Name).ToString();
                var relation_SortingField = relation_Clctn.CreateLocalField(relation_SortingName, sortingVariableTemplate.DataType, sortingVariableTemplate.GetTimeDimensionality(), sortingVariableTemplate.PredefinedVariableTemplateOption);
                relation_SortingField.SetVariableInfo(sortingVariableTemplateRef);
                relation_Clctn.SetLocalField_ForSorting(relation_SortingField.Id);
                relationType_VariableTemplates.Remove(sortingVariableTemplate);
                ExportState.AddVariableField(sortingVariableTemplateRef, relation_SortingField);

                remainingVariableTemplatesByCollection.Add(relation_Clctn, relationType_VariableTemplates);
            }

            foreach (var collectionBuckets in remainingVariableTemplatesByCollection)
            {
                CreateFieldsForVariableTemplates(DataProvider, ExportState, collectionBuckets.Key, collectionBuckets.Value);
            }


            foreach (var groupBucket in orderedGroups)
            {
                var groupIndex = groupBucket.Key;
                var group = groupBucket.Value;

                var groupMembers = new JArray();
                var groupObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_VariableTemplateGroup_JsonDocument>();
                groupObject[groupObject.SchemaDoc.Id.Name] = group.Id;
                groupObject[groupObject.SchemaDoc.ProcessingIndex.Name] = groupIndex;
                groupObject[groupObject.SchemaDoc.HasCycles.Name] = group.HasCycle;
                groupObject[groupObject.SchemaDoc.HasUnresolvableCycles.Name] = (group.HasCycle && !group.HasStrictTimeOrdering);
                groupObject[groupObject.SchemaDoc.Members_Array.Name] = groupMembers;

                int priority = 1;
                foreach (var groupedVariableTemplateRef in group.TimeOrderedNodes)
                {
                    var groupMember = new JObject();
                    groupMembers.Add(groupMember);

                    groupMember[groupObject.SchemaDoc.Member_Object_VariableTemplateId.Name] = groupedVariableTemplateRef.ModelObjectId;
                    groupMember[groupObject.SchemaDoc.Member_Object_Priority.Name] = priority;

                    priority++;
                }
            }

            foreach (var variableTemplate in DataState.VariableTemplates.Values)
            {
                if (variableTemplate.VariableType == VariableType.Input)
                { continue; }

                var group = ExportState.GetComputationGroup(variableTemplate.ModelObjectRef);
                var formula = DataState.FormulasByRef[variableTemplate.FormulaId.ModelObjectRef];
                var dependencyRefs = formula.Arguments.Values.Where(x => x.ArgumentType == ArgumentType.ReferencedId).Select(x => x.ReferencedModelObject).ToList();
                var uniqueDependencyRefs = dependencyRefs.Distinct(ModelObjectReference.DimensionalComparer).ToList();

                var dependencies = new JArray();
                var variableTemplateObject = ExportState.GetVariableTemplateObject(variableTemplate.ModelObjectRef);
                variableTemplateObject[variableTemplateObject.SchemaDoc.Dependencies_Array.Name] = dependencies;

                foreach (var dependencyRef in uniqueDependencyRefs)
                {
                    var dependency = new JObject();
                    dependencies.Add(dependency);

                    dependency[variableTemplateObject.SchemaDoc.Dependency_Object_VariableTemplateId.Name] = dependencyRef.ModelObjectId;
                    dependency[variableTemplateObject.SchemaDoc.Dependency_Object_StructuralDimensionNumber.Name] = dependencyRef.NonNullAlternateDimensionNumber;
                    dependency[variableTemplateObject.SchemaDoc.Dependency_Object_IsStrict.Name] = (!group.NodesIncluded.Contains(dependencyRef));
                }
            }

            foreach (var timePeriodType in usedTimePeriodTypes)
            {
                var timePeriodTypeObject = ExportState.GetTimePeriodTypeObject(timePeriodType);
                var timeframe = new TimeDimension(TimeDimensionType.Primary, TimeValueType.PeriodValue, timePeriodType, ExportState.StartDate, ExportState.EndDate);
                var timePeriods = timeframe.GeneratePeriodsForTimeDimension();
                var isForever = false;

                foreach (var timePeriod in timePeriods)
                {
                    var timePeriodId = GetTimePeriodId(timePeriod);

                    var timePeriodObject = PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_TimePeriod_JsonDocument>();
                    timePeriodObject[timePeriodObject.SchemaDoc.Id.Name] = timePeriodId;
                    timePeriodObject[timePeriodObject.SchemaDoc.TimePeriodTypeId.Name] = (int)timePeriodType;
                    timePeriodObject[timePeriodObject.SchemaDoc.StartDate.Name] = timePeriod.StartDate;
                    timePeriodObject[timePeriodObject.SchemaDoc.EndDate.Name] = timePeriod.EndDate;
                    timePeriodObject[timePeriodObject.SchemaDoc.IsForever.Name] = isForever;
                    ExportState.AddTimePeriodObject(timePeriod, timePeriodType, timePeriodObject);
                }
            }


            var script = GenericDb.ExportToScript(dbType).UpdateDeciaPrefix();
            script += PrereqData.Metadata_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.TimeDimensionSetting_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.TimePeriodType_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.DataType_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.ObjectType_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.TimePeriod_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.StructuralType_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.VariableTemplate_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.VariableTemplateGroup_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();
            script += PrereqData.ResultSet_Document.GetAllInserts_AsJsText().UpdateDeciaPrefix();

            var jsonState = GenericDb.Exported_JsonSchema;
            var jsonSchema = jsonState.Schema;
            var collectionMappings = jsonState.CollectionMappings;
            var fieldMappings = jsonState.FieldMappings;
            var rootDocumentMappings = collectionMappings.Where(x => (x.Value.Key.Root == x.Value.Value)).ToDictionary(x => x.Value.Key, x => x.Key);

            if (!HasDataToExport)
            {
                var globalCollection = ExportState.GetRootStructuralCollection(DataState.GlobalTypeRef);
                var globalDocumentType = collectionMappings[globalCollection].Key;
                var globalIdField = globalCollection.Field_ForId;
                var globalNameField = globalCollection.Field_ForName;

                var dataObject = new JObject();
                dataObject[globalIdField.Name] = ModelObjectReference.GlobalInstanceGuid;
                dataObject[globalNameField.Name] = GlobalInstance.DefaultGlobalInstanceName;
                globalDocumentType.AddInstance(dataObject);

                script += globalDocumentType.GetAllInserts_AsJsText(globalCollection);

                script += Environment.NewLine + Environment.NewLine + DeciaBaseUtils.Config_OpLogListener_Start.UpdateDeciaPrefix(); ;

                return script;
            }

            var modelInstanceRef = ModelInstanceId.ModelObjectRef;

            foreach (var collection in GenericDb.RootCollections_Ordered)
            {
                var structuralTypeRef = collection.StructuralTypeRef;
                var structuralInstanceIds = DataState.GetStructuralInstanceIds(modelInstanceRef, structuralTypeRef);

                foreach (var structuralInstanceId in structuralInstanceIds)
                {
                    var structuralInstance = DataState.GetStructuralInstance(structuralInstanceId);
                    ExportInstanceData(collectionMappings, fieldMappings, structuralInstance, collection, null);
                }
            }

            foreach (JsonDocument documentType in jsonSchema.DocumentsById.Values)
            {
                var rootCollection = rootDocumentMappings[documentType];
                script += documentType.GetAllInserts_AsJsText(rootCollection);
            }

            script += Environment.NewLine + Environment.NewLine + DeciaBaseUtils.Config_OpLogListener_Start.UpdateDeciaPrefix(); ;

            return script;
        }

        #endregion

        #region Helper Methods for Required Collections

        private static void AddTimePeriodTypesUsed(HashSet<TimePeriodType> usedTimePeriodTypes, List<VariableTemplate> entityType_VariableTemplates)
        {
            usedTimePeriodTypes.AddRange(entityType_VariableTemplates.Select(x => x.HasSpecific_TimePeriodType_Primary ? x.Specific_TimePeriodType_Primary : TimePeriodType.Years));
            usedTimePeriodTypes.AddRange(entityType_VariableTemplates.Select(x => x.HasSpecific_TimePeriodType_Secondary ? x.Specific_TimePeriodType_Secondary : TimePeriodType.Years));
        }

        private static void FillWithVariableTemplates(IFormulaDataProvider dataProvider, NoSql_ExportState exportState, ModelObjectReference structuralTypeRef, IEnumerable<VariableTemplate> relevantVariableTemplates)
        {
            foreach (var variableTemplate in relevantVariableTemplates)
            {
                var variableTemplateRef = variableTemplate.ModelObjectRef;
                var variableTemplate_DataType = dataProvider.GetValidatedDataType(variableTemplateRef).Value;
                var variableTemplate_TimeDimensionality = dataProvider.GetValidatedTimeDimensions(variableTemplateRef);
                var relatedEntityTypeRef = variableTemplate.RelatedEntityTypeRef;

                var variableTemplateRow = exportState.PrereqData.CreateJsonObject<DeciaBase_JsonSet.Decia_VariableTemplate_JsonDocument>();
                variableTemplateRow[variableTemplateRow.SchemaDoc.Id.Name] = variableTemplate.ModelObjectId;
                variableTemplateRow[variableTemplateRow.SchemaDoc.Name.Name] = variableTemplate.Name;
                variableTemplateRow[variableTemplateRow.SchemaDoc.MongoName.Name] = variableTemplate.Name.ToEscaped_VarName();
                variableTemplateRow[variableTemplateRow.SchemaDoc.StructuralTypeId.Name] = variableTemplate.Containing_StructuralTypeId.ModelObjectId;
                variableTemplateRow[variableTemplateRow.SchemaDoc.Related_StructuralTypeId.Name] = null;
                variableTemplateRow[variableTemplateRow.SchemaDoc.Related_StructuralDimensionNumber.Name] = null;
                variableTemplateRow[variableTemplateRow.SchemaDoc.IsComputed.Name] = variableTemplate.VariableType.IsComputed();
                variableTemplateRow[variableTemplateRow.SchemaDoc.TimeDimensionCount.Name] = variableTemplate_TimeDimensionality.UsedDimensionCount;
                variableTemplateRow[variableTemplateRow.SchemaDoc.PrimaryTimePeriodTypeId.Name] = null;
                variableTemplateRow[variableTemplateRow.SchemaDoc.SecondaryTimePeriodTypeId.Name] = null;
                variableTemplateRow[variableTemplateRow.SchemaDoc.DataTypeId.Name] = (int)variableTemplate_DataType;
                variableTemplateRow[variableTemplateRow.SchemaDoc.Instance_Field_Name.Name] = variableTemplateRow.GetValue(variableTemplateRow.SchemaDoc.MongoName.Name);
                variableTemplateRow[variableTemplateRow.SchemaDoc.Instance_Field_DefaultValue.Name] = null;

                if (relatedEntityTypeRef.HasValue)
                {
                    variableTemplateRow[variableTemplateRow.SchemaDoc.Related_StructuralTypeId.Name] = relatedEntityTypeRef.Value.ModelObjectId;
                    variableTemplateRow[variableTemplateRow.SchemaDoc.Related_StructuralDimensionNumber.Name] = relatedEntityTypeRef.Value.NonNullAlternateDimensionNumber;
                }

                if (variableTemplate_TimeDimensionality.PrimaryTimeDimension.HasTimeValue)
                {
                    if (!variableTemplate_TimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType.HasValue)
                    { throw new InvalidOperationException("Primary Time Period Type is not set."); }

                    variableTemplateRow[variableTemplateRow.SchemaDoc.PrimaryTimePeriodTypeId.Name] = (int)variableTemplate_TimeDimensionality.PrimaryTimeDimension.TimePeriodType;
                }
                if (variableTemplate_TimeDimensionality.SecondaryTimeDimension.HasTimeValue)
                {
                    if (!variableTemplate_TimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType.HasValue)
                    { throw new InvalidOperationException("Secondary Time Period Type is not set."); }

                    variableTemplateRow[variableTemplateRow.SchemaDoc.SecondaryTimePeriodTypeId.Name] = (int)variableTemplate_TimeDimensionality.SecondaryTimeDimension.TimePeriodType;
                }
                if (variableTemplate.DefaultValue != ((object)null))
                {
                    if (!variableTemplate.DefaultValue.IsNull && variableTemplate.DefaultValue.IsValid)
                    { variableTemplateRow[variableTemplateRow.SchemaDoc.Instance_Field_DefaultValue.Name] = variableTemplate.DefaultValue.GetValue().ToString(); }
                }

                exportState.AddVariableTemplateObject(variableTemplateRef, variableTemplateRow);
            }
        }

        private static void CreateFieldsForVariableTemplates(IFormulaDataProvider dataProvider, NoSql_ExportState exportState, GenericCollection currentClctn, ICollection<VariableTemplate> remainingVariableTemplatesToAdd)
        {
            var currentIdField = currentClctn.GetLocalField(currentClctn.FieldId_ForId.Value);
            var structuralTypeRef = currentClctn.StructuralTypeRef;

            var clctnNameBase = structuralTypeRef.HexBasedName;
            currentClctn.Alias = clctnNameBase;

            foreach (var variableTemplate in remainingVariableTemplatesToAdd.GetOrderedList())
            {
                var variableTemplateRef = variableTemplate.ModelObjectRef;
                var variableTemplate_DataType = dataProvider.GetValidatedDataType(variableTemplateRef).Value;
                var variableTemplate_TimeDimensionality = dataProvider.GetValidatedTimeDimensions(variableTemplateRef);

                var variableTemplateObject = exportState.GetVariableTemplateObject(variableTemplateRef);
                var timePeriodType_Primary = variableTemplate_TimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType;
                var timePeriodType_Secondary = variableTemplate_TimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType;

                var isArray = (timePeriodType_Primary.HasValue || timePeriodType_Secondary.HasValue);
                var isComputed = variableTemplate.VariableType.IsComputed();

                if (isComputed)
                { continue; }

                var variableTemplateName = variableTemplateObject.GetValue(variableTemplateObject.SchemaDoc.MongoName.Name).ToString();
                var field = (GenericField)null;

                if (variableTemplate.PredefinedVariableTemplateOption == PredefinedVariableTemplateOption.Id_Parent)
                {
                    var parentEntityTypeRef = variableTemplate.RelatedEntityTypeRef.Value;
                    var parentClctn = exportState.GetRootStructuralCollection(parentEntityTypeRef);

                    field = currentClctn.CreateLocalField(variableTemplateName, variableTemplate.DataType, variableTemplate_TimeDimensionality.GetTimeDimensionality(), variableTemplate.PredefinedVariableTemplateOption);
                    field.SetVariableInfo(variableTemplateRef, variableTemplate.RelatedEntityTypeRef.Value, true);
                }
                else if (variableTemplate.PredefinedVariableTemplateOption == PredefinedVariableTemplateOption.Id_Related)
                {
                    var relatedEntityTypeRef = variableTemplate.RelatedEntityTypeRef.Value;
                    var relatedClctn = exportState.GetRootStructuralCollection(relatedEntityTypeRef);

                    field = currentClctn.CreateLocalField(variableTemplateName, variableTemplate_TimeDimensionality.GetTimeDimensionality(), relatedClctn, variableTemplate.PredefinedVariableTemplateOption, relatedEntityTypeRef.AlternateDimensionNumber);
                    field.SetVariableInfo(variableTemplateRef, relatedEntityTypeRef, true);
                    field.SetCaching(variableTemplate.LocalCacheMode, variableTemplate.ForeignCacheMode, variableTemplate.MaxCachingDepth, variableTemplate.ForeignInvalidationMode, variableTemplate.ForeignInvalidationValue);
                }
                else if (variableTemplate.IsStructuralVariable)
                {
                    var relatedEntityTypeRef = variableTemplate.RelatedEntityTypeRef.Value;
                    var relatedClctn = exportState.GetRootStructuralCollection(relatedEntityTypeRef);
                    var dimensionNumber = (variableTemplate.IsNavigationVariable) ? relatedEntityTypeRef.NonNullAlternateDimensionNumber : (int?)null;

                    field = currentClctn.CreateLocalField(variableTemplateName, variableTemplate_TimeDimensionality.GetTimeDimensionality(), relatedClctn, variableTemplate.PredefinedVariableTemplateOption, dimensionNumber);
                    field.SetVariableInfo(variableTemplateRef, relatedEntityTypeRef, dimensionNumber.HasValue);
                    field.SetCaching(variableTemplate.LocalCacheMode, variableTemplate.ForeignCacheMode, variableTemplate.MaxCachingDepth, variableTemplate.ForeignInvalidationMode, variableTemplate.ForeignInvalidationValue);
                }
                else
                {
                    field = currentClctn.CreateLocalField(variableTemplateName, variableTemplate_DataType, variableTemplate_TimeDimensionality.GetTimeDimensionality(), variableTemplate.PredefinedVariableTemplateOption);
                    field.SetVariableInfo(variableTemplateRef);
                }

                field.DefaultValue = (!variableTemplate.DefaultValue.IsNull) ? variableTemplate.DefaultValue.GetValue().ToString() : null;

                exportState.AddVariableField(variableTemplateRef, field);
            }
        }

        private void ExportInstanceData(Dictionary<GenericCollection, KeyValuePair<JsonDocument, JsonProperty>> collectionMappings, Dictionary<GenericField, JsonProperty> fieldMappings, IStructuralMember_Orderable currentStructuralInstance, GenericCollection currentCollection, JObject currentObject)
        {
            if (currentObject == null)
            {
                var documentType = collectionMappings[currentCollection].Key;

                currentObject = new JObject();
                documentType.AddInstance(currentObject);
            }

            var modelInstanceRef = currentStructuralInstance.GetModelMemberId().ModelObjectRef;
            var currentStructuralInstanceRef = currentStructuralInstance.ModelObjectRef;
            var currentStructuralTypeRef = currentStructuralInstance.GetStructuralId_ForType().ModelObjectRef;

            var variableTemplates = ExportState.DataState.GetVariableTemplates(currentStructuralTypeRef, null);
            var inputVariableTemplate = variableTemplates.Where(x => x.Value.VariableType == VariableType.Input).ToDictionary(x => x.Key, x => x.Value);
            var replicationFields = currentCollection.LocalFields.Where(x => x.FieldMode == FieldMode.ReplicatedList).ToList();

            foreach (var inputTemplate in inputVariableTemplate.Values.GetOrderedList())
            {
                var inputTemplateRef = inputTemplate.ModelObjectRef;
                var predefinedType = inputTemplate.PredefinedVariableTemplateOption;
                var field = ExportState.GetVariableField(inputTemplateRef);
                var propertyType = fieldMappings[field];

                if (predefinedType.HasValue)
                {
                    if (predefinedType == PredefinedVariableTemplateOption.Id)
                    { currentObject[propertyType.Name] = currentStructuralInstance.StructuralInstanceGuid; }
                    else if (predefinedType == PredefinedVariableTemplateOption.Name)
                    { currentObject[propertyType.Name] = currentStructuralInstance.GetStructuralMember_Name(); }
                    else if (predefinedType == PredefinedVariableTemplateOption.Order)
                    { currentObject[propertyType.Name] = currentStructuralInstance.GetStructuralMember_OrderNumber(); }
                    else if (predefinedType == PredefinedVariableTemplateOption.Id_Parent)
                    { currentObject[propertyType.Name] = (currentStructuralInstance as EntityInstance).Parent_EntityInstanceGuid; }
                    else if (predefinedType == PredefinedVariableTemplateOption.Id_Related)
                    {
                        var relationInstance = (RelationInstance)currentStructuralInstance;
                        var entityInstanceId = relationInstance.RelatedEntityInstanceGuids[inputTemplate.RelatedEntityTypeRef.Value];

                        var idObject = new JObject();
                        currentObject[propertyType.Name] = idObject;

                        idObject[field.XmlMapping.DocIdElement.Name] = entityInstanceId;
                        idObject[GenericDatabaseUtils.MaxCachingDepth_Name] = field.MaxCachingDepth;
                    }
                    else
                    { throw new InvalidOperationException("Unexpected PredefinedVariableTemplateOption encountered."); }
                }
                else
                {
                    var variableInstance = DataState.GetVariableInstance(modelInstanceRef, currentStructuralInstanceRef, inputTemplateRef);
                    var timeMatrix = DataProvider.GetAssessedTimeMatrix(modelInstanceRef, variableInstance.ModelObjectRef);

                    var hasTimeValue = (timeMatrix.TimeDimensionSet.UsedDimensionCount > 0);
                    var isReference = inputTemplate.IsStructuralVariable;

                    if (!hasTimeValue && !isReference)
                    {
                        var dynamicValue = timeMatrix.GetValue(MultiTimePeriodKey.DimensionlessTimeKey);
                        var value = dynamicValue.GetValue();

                        currentObject[propertyType.Name] = (value != null) ? JToken.FromObject(value) : null;
                    }
                    else if (!hasTimeValue && isReference)
                    {
                        var refObject = new JObject();
                        currentObject[propertyType.Name] = refObject;

                        var dynamicValue = timeMatrix.GetValue(MultiTimePeriodKey.DimensionlessTimeKey);
                        var value = dynamicValue.GetValue();

                        refObject[field.XmlMapping.DocIdElement.Name] = (value != null) ? JToken.FromObject(value) : null;
                        refObject[GenericDatabaseUtils.MaxCachingDepth_Name] = field.MaxCachingDepth;
                    }
                    else
                    {
                        var timeKeysArray = new JArray();
                        currentObject[propertyType.Name] = timeKeysArray;

                        foreach (var timeKey in timeMatrix.TimeKeys)
                        {
                            var timeKeyObject = new JObject();
                            timeKeysArray.Add(timeKeyObject);

                            if (timeMatrix.TimeDimensionSet.UsedDimensionCount >= 1)
                            {
                                var tp1 = timeKey.PrimaryTimePeriod;
                                var tp1Id = GetTimePeriodId(tp1);

                                var tp1KeyElement = field.XmlMapping.KeyElements[new ModelObjectReference(TimeDimensionType.Primary)];
                                timeKeyObject[tp1KeyElement.Name] = tp1Id;
                            }

                            if (timeMatrix.TimeDimensionSet.UsedDimensionCount >= 2)
                            {
                                var tp2 = timeKey.SecondaryTimePeriod;
                                var tp2Id = GetTimePeriodId(tp2);

                                var tp2KeyElement = field.XmlMapping.KeyElements[new ModelObjectReference(TimeDimensionType.Secondary)];
                                timeKeyObject[tp2KeyElement.Name] = tp2Id;
                            }

                            var dynamicValue = timeMatrix.GetValue(timeKey);
                            var value = dynamicValue.GetValue();

                            timeKeyObject[field.XmlMapping.ValueElement.Name] = (value != null) ? JToken.FromObject(value) : null;
                            if (isReference)
                            { timeKeyObject[GenericDatabaseUtils.MaxCachingDepth_Name] = field.MaxCachingDepth; }
                        }
                    }
                }
            }

            foreach (var replicationField in replicationFields)
            {
                var arrayName = replicationField.XmlMapping.RootFieldElement.Name;
                var arrayObject = new JArray();

                currentObject[arrayName] = arrayObject;
            }

            if (currentStructuralTypeRef.ModelObjectType != ModelObjectType.EntityType)
            { return; }

            var nestedCollectionFields = currentCollection.LocalFields.Where(x => x.FieldMode == FieldMode.NestedCollection).ToList();
            var entityInstanceRefTree = DataProvider.StructuralMap.GetEntityInstanceTree(modelInstanceRef);

            foreach (var nestedCollectionField in nestedCollectionFields)
            {
                var nestedCollection = nestedCollectionField.NestedCollection;
                var nestedStructuralTypeRef = nestedCollection.StructuralTypeRef;
                var nestedStructuralInstanceRefs = entityInstanceRefTree.GetChildren(currentStructuralInstanceRef);

                var propertyType = fieldMappings[nestedCollectionField];
                var nestedObjectArray = new JArray();

                currentObject[propertyType.Name] = nestedObjectArray;

                foreach (var nestedStructuralInstanceRef in nestedStructuralInstanceRefs)
                {
                    var nestedStructuralInstanceId = new StructuralMemberId(DataState.RevisionChain.ProjectGuid, DataState.RevisionChain.DesiredRevisionNumber, DataState.ModelTemplateNumber, modelInstanceRef.ModelObjectId, StructuralTypeOption.EntityType, nestedStructuralTypeRef.ModelObjectIdAsInt, nestedStructuralInstanceRef.ModelObjectId);
                    var nestedStructuralInstance = DataState.GetStructuralInstance(nestedStructuralInstanceId);

                    var nestedObject = new JObject();
                    nestedObjectArray.Add(nestedObject);

                    ExportInstanceData(collectionMappings, fieldMappings, nestedStructuralInstance, nestedCollection, nestedObject);
                }
            }
        }

        #endregion
    }
}