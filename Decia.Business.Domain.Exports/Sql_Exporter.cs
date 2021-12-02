using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Exports;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Sql;
using Decia.Business.Common.Sql.Base;
using Decia.Business.Common.Sql.Constraints;
using Decia.Business.Common.Sql.Programmatics;
using Decia.Business.Common.Sql.Triggers;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Formulas.Exports;
using Decia.Business.Domain.Formulas.Operations.Queries;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.Models;
using Decia.Business.Domain.Exports.SqlDbs;

namespace Decia.Business.Domain.Exports
{
    public class Sql_Exporter
    {
        #region Constants

        public static readonly string ChangeCount_ResetCommand = string.Format("EXEC [dbo].[spDecia_ChangeState_ResetLatest] {0}, {1};", DeciaBaseUtils.NullValue, DeciaBaseUtils.NullValue);
        public const long InitialChangeCount = 0;
        public static DateTime InitialChangeDate { get { return DateTime.UtcNow; } }
        public const double TimePeriod_MinMultiplier = .85;
        public const double TimePeriod_MaxMultiplier = 1.15;

        #endregion

        #region Members

        private Sql_ExportState m_ExportState;

        #endregion

        #region Constructors

        public Sql_Exporter(ModelDataProvider dataProvider, Nullable<ModelInstanceId> modelInstanceToExport)
        {
            m_ExportState = new Sql_ExportState(dataProvider, modelInstanceToExport);
        }

        #endregion

        #region Properties

        public Sql_ExportState ExportState { get { return m_ExportState; } }
        public ModelDataProvider DataProvider { get { return ExportState.DataProvider; } }
        public ModelDataState DataState { get { return DataProvider.DataState; } }
        public RevisionChain RevisionChain { get { return DataState.RevisionChain; } }
        public bool HasDataToExport { get { return ExportState.HasDataToExport; } }
        public ModelInstanceId ModelInstanceId { get { return ExportState.ModelInstanceId; } }

        public GenericDatabase GenericDb { get { return ExportState.GenericDb; } }
        public DeciaBase_DataSet PrereqData { get { return ExportState.PrereqData; } }
        public DataSet ModelData { get { return ExportState.ModelData; } }

        #endregion

        #region Methods

        public string ExportToDatabase(SqlDb_TargetType dbType, string connectionString_ToMasterDb)
        {
            var resultingConnectionString = ExportToDatabase(dbType, connectionString_ToMasterDb, AdoNetUtils.Default_InitialCatalog);
            return resultingConnectionString;
        }

        public string ExportToDatabase(SqlDb_TargetType dbType, string connectionString_ToMasterDb, string alternateDatabaseName)
        {
            var script = ExportToScript(dbType, alternateDatabaseName);
            var dbName = (!string.IsNullOrWhiteSpace(alternateDatabaseName)) ? alternateDatabaseName : GenericDb.Name;
            var resultingConnectionString = GenericDatabaseUtils.ExportScriptToServer(dbName, script, connectionString_ToMasterDb);
            return resultingConnectionString;
        }

        public string ExportToScript(SqlDb_TargetType dbType)
        {
            var script = ExportToScript(dbType, AdoNetUtils.Default_InitialCatalog);
            return script;
        }

        public string ExportToScript(SqlDb_TargetType dbType, string alternateDatabaseName)
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


            if (true)
            {
                var globalType = DataState.GlobalType;
                var globalTypeRef = globalType.ModelObjectRef;
                var globalType_VariableTemplates = DataState.GetVariableTemplates(globalTypeRef, null).Values.OrderBy(x => x.OrderNumber).ToList();

                AddTimePeriodTypesUsed(usedTimePeriodTypes, globalType_VariableTemplates);

                var idVariableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(globalTypeRef);
                var idVariableTemplate = DataState.VariableTemplatesByRef[idVariableTemplateRef];
                var nameVariableTemplateRef = DataProvider.DependencyMap.GetNameVariableTemplate(globalTypeRef);
                var nameVariableTemplate = DataState.VariableTemplatesByRef[nameVariableTemplateRef];
                var sortingVariableTemplateRef = DataProvider.DependencyMap.GetOrderVariableTemplate(globalTypeRef);
                var sortingVariableTemplate = DataState.VariableTemplatesByRef[sortingVariableTemplateRef];

                var globalTypeRow = PrereqData.Decia_StructuralType.NewDecia_StructuralTypeRow();
                globalTypeRow.Id = globalType.ModelObjectId;
                globalTypeRow.Name = globalType.Name;
                globalTypeRow.SqlName = globalType.Name.ToEscaped_VarName();
                globalTypeRow.ObjectTypeId = (int)globalType.ModelObjectType;
                globalTypeRow.TreeLevel_Basic = DataProvider.StructuralMap.GetDistanceFromGlobal(globalTypeRef, false);
                globalTypeRow.TreeLevel_Extended = DataProvider.StructuralMap.GetDistanceFromGlobal(globalTypeRef, true);
                globalTypeRow.SetParent_StructuralTypeIdNull();
                globalTypeRow.Parent_IsNullable = false;
                globalTypeRow.SetParent_Default_InstanceIdNull();
                globalTypeRow.Instance_Table_Name = globalTypeRow.SqlName;
                globalTypeRow.Instance_Id_VariableTemplateId = idVariableTemplateRef.ModelObjectId;
                globalTypeRow.Instance_Name_VariableTemplateId = nameVariableTemplateRef.ModelObjectId;
                globalTypeRow.Instance_Sorting_VariableTemplateId = sortingVariableTemplateRef.ModelObjectId;
                globalTypeRow.SetInstance_ParentId_VariableTemplateIdNull();

                ExportState.AddStructuralTypeRow(globalTypeRef, globalTypeRow);
                FillWithVariableTemplates(DataProvider, ExportState, globalTypeRef, globalType_VariableTemplates);


                var global_GenericTbl_TD0 = GenericDb.CreateTable(globalTypeRow.SqlName, true, false);
                global_GenericTbl_TD0.SetStructuralTypeRef(globalTypeRef);
                ExportState.AddRootStructuralTable(globalTypeRef, global_GenericTbl_TD0);

                var global_IdRow = ExportState.GetVariableTemplateRow(idVariableTemplateRef);
                var global_IdColumn = global_GenericTbl_TD0.CreateColumn(global_IdRow.SqlName, idVariableTemplate.DataType, false, true);
                global_IdColumn.SetVariableTemplateRef(idVariableTemplateRef);
                global_GenericTbl_TD0.SetPrimaryKey(global_IdColumn.Id);
                global_GenericTbl_TD0.SetColumn_ForId(global_IdColumn.Id);
                global_GenericTbl_TD0.SetSingletonTrigger();
                globalType_VariableTemplates.Remove(idVariableTemplate);
                ExportState.AddVariableColumn(idVariableTemplateRef, global_IdColumn);

                var global_NameRow = ExportState.GetVariableTemplateRow(nameVariableTemplateRef);
                var global_NameColumn = global_GenericTbl_TD0.CreateColumn(global_NameRow.SqlName, nameVariableTemplate.DataType, false, false);
                global_NameColumn.SetVariableTemplateRef(nameVariableTemplateRef);
                global_GenericTbl_TD0.SetColumn_ForName(global_NameColumn.Id);
                globalType_VariableTemplates.Remove(nameVariableTemplate);
                ExportState.AddVariableColumn(nameVariableTemplateRef, global_NameColumn);

                var global_SortingRow = ExportState.GetVariableTemplateRow(sortingVariableTemplateRef);
                var global_SortingColumn = global_GenericTbl_TD0.CreateColumn(global_SortingRow.SqlName, sortingVariableTemplate.DataType, true, false);
                global_SortingColumn.SetVariableTemplateRef(sortingVariableTemplateRef);
                global_GenericTbl_TD0.SetColumn_ForSorting(global_SortingColumn.Id);
                globalType_VariableTemplates.Remove(sortingVariableTemplate);
                ExportState.AddVariableColumn(sortingVariableTemplateRef, global_SortingColumn);

                CreateTablesAndColumnsForVariableTemplates(DataProvider, ExportState, global_GenericTbl_TD0, global_IdColumn, globalType_VariableTemplates);
            }

            var orderedEntityTypeRefs = DataProvider.StructuralMap.EntityTypeExtendedNetwork.GetDependencyOrderedTraversalFromRoot();
            foreach (var entityTypeRef in orderedEntityTypeRefs)
            {
                var entityType = DataState.EntityTypesByRef[entityTypeRef];
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

                var entityTypeRow = PrereqData.Decia_StructuralType.NewDecia_StructuralTypeRow();
                entityTypeRow.Id = entityType.ModelObjectId;
                entityTypeRow.Name = entityType.Name;
                entityTypeRow.SqlName = entityType.Name.ToEscaped_VarName();
                entityTypeRow.ObjectTypeId = (int)entityType.ModelObjectType;
                entityTypeRow.TreeLevel_Basic = DataProvider.StructuralMap.GetDistanceFromGlobal(entityTypeRef, false);
                entityTypeRow.TreeLevel_Extended = DataProvider.StructuralMap.GetDistanceFromGlobal(entityTypeRef, true);
                entityTypeRow.SetParent_StructuralTypeIdNull();
                entityTypeRow.Parent_IsNullable = false;
                entityTypeRow.SetParent_Default_InstanceIdNull();
                entityTypeRow.Instance_Table_Name = entityTypeRow.SqlName;
                entityTypeRow.Instance_Id_VariableTemplateId = idVariableTemplateRef.ModelObjectId;
                entityTypeRow.Instance_Name_VariableTemplateId = nameVariableTemplateRef.ModelObjectId;
                entityTypeRow.Instance_Sorting_VariableTemplateId = sortingVariableTemplateRef.ModelObjectId;
                entityTypeRow.SetInstance_ParentId_VariableTemplateIdNull();

                if (hasParentEntityType)
                {
                    entityTypeRow.Parent_StructuralTypeId = parentStructuralTypeRef.Value.ModelObjectId;
                    entityTypeRow.Parent_IsNullable = entityType.Parent_IsNullable;
                    entityTypeRow.Instance_ParentId_VariableTemplateId = parentIdVariableTemplateRef.Value.ModelObjectId;
                }

                ExportState.AddStructuralTypeRow(entityTypeRef, entityTypeRow);
                FillWithVariableTemplates(DataProvider, ExportState, entityTypeRef, entityType_VariableTemplates);


                var entity_GenericTbl_TD0 = GenericDb.CreateTable(entityTypeRow.SqlName, true, true);
                entity_GenericTbl_TD0.SetStructuralTypeRef(entityTypeRef);
                ExportState.AddRootStructuralTable(entityTypeRef, entity_GenericTbl_TD0);

                var entity_IdRow = ExportState.GetVariableTemplateRow(idVariableTemplateRef);
                var entity_IdColumn = entity_GenericTbl_TD0.CreateColumn(entity_IdRow.SqlName, idVariableTemplate.DataType, false, true);
                entity_IdColumn.SetVariableTemplateRef(idVariableTemplateRef);
                entity_GenericTbl_TD0.SetPrimaryKey(entity_IdColumn.Id);
                entity_GenericTbl_TD0.SetColumn_ForId(entity_IdColumn.Id);
                entityType_VariableTemplates.Remove(idVariableTemplate);
                ExportState.AddVariableColumn(idVariableTemplateRef, entity_IdColumn);

                var entity_NameRow = ExportState.GetVariableTemplateRow(nameVariableTemplateRef);
                var entity_NameColumn = entity_GenericTbl_TD0.CreateColumn(entity_NameRow.SqlName, nameVariableTemplate.DataType, false, false);
                entity_NameColumn.SetVariableTemplateRef(nameVariableTemplateRef);
                entity_GenericTbl_TD0.SetColumn_ForName(entity_NameColumn.Id);
                entityType_VariableTemplates.Remove(nameVariableTemplate);
                ExportState.AddVariableColumn(nameVariableTemplateRef, entity_NameColumn);

                var entity_SortingRow = ExportState.GetVariableTemplateRow(sortingVariableTemplateRef);
                var entity_SortingColumn = entity_GenericTbl_TD0.CreateColumn(entity_SortingRow.SqlName, sortingVariableTemplate.DataType, true, false);
                entity_SortingColumn.SetVariableTemplateRef(sortingVariableTemplateRef);
                entity_GenericTbl_TD0.SetColumn_ForSorting(entity_SortingColumn.Id);
                entityType_VariableTemplates.Remove(sortingVariableTemplate);
                ExportState.AddVariableColumn(sortingVariableTemplateRef, entity_SortingColumn);

                if (hasParentEntityType)
                {
                    var parentTable = ExportState.GetRootStructuralTable(parentStructuralTypeRef.Value);

                    var entity_ParentIdRow = ExportState.GetVariableTemplateRow(parentIdVariableTemplateRef.Value);
                    var entity_ParentIdColumn = entity_GenericTbl_TD0.CreateColumn(entity_ParentIdRow.SqlName, parentIdVariableTemplate.DataType, false, false);
                    entity_ParentIdColumn.SetVariableTemplateRef(parentIdVariableTemplateRef.Value);
                    entity_GenericTbl_TD0.AddDynamicForeignKey(parentTable.Id, entity_ParentIdColumn.Id, parentTable.ColumnId_ForId.Value);
                    entity_GenericTbl_TD0.SetNestedListTrigger(entity_ParentIdColumn.Id);
                    entityType_VariableTemplates.Remove(parentIdVariableTemplate);
                    ExportState.AddVariableColumn(parentIdVariableTemplateRef.Value, entity_ParentIdColumn);
                }

                CreateTablesAndColumnsForVariableTemplates(DataProvider, ExportState, entity_GenericTbl_TD0, entity_IdColumn, entityType_VariableTemplates);

                if (entityType.AutoUpdate_InstanceNames)
                {
                    Func<int, string> columnNameGetter = delegate(int variableTemplateNumber)
                    {
                        var variableTemplateRef = new ModelObjectReference(ModelObjectType.VariableTemplate, variableTemplateNumber);
                        var variableColumn = ExportState.GetVariableColumn(variableTemplateRef);
                        return variableColumn.Name;
                    };

                    var nameChunks = EntityType.RenderAsNameChunks_AutoNameFormat(entityType, columnNameGetter);
                    var sqlFormula = "''";

                    foreach (var nameChunk in nameChunks)
                    {
                        var chunkSql = (nameChunk.IsRef) ? string.Format("CONVERT(nvarchar(MAX), {0})", nameChunk.ChunkText) : string.Format("'{0}'", nameChunk.ChunkText);
                        sqlFormula += " + " + chunkSql;
                    }
                    entity_NameColumn.SqlFormula = sqlFormula;
                }
            }

            var orderedRelationTypes = DataState.RelationTypes.Values.OrderBy(x => x.OrderNumber);
            foreach (var relationType in orderedRelationTypes)
            {
                var relationTypeRef = relationType.ModelObjectRef;
                var relationType_VariableTemplates = DataState.GetVariableTemplates(relationTypeRef, null).Values.OrderBy(x => x.OrderNumber).ToList();

                AddTimePeriodTypesUsed(usedTimePeriodTypes, relationType_VariableTemplates);

                var idVariableTemplateRef = DataProvider.DependencyMap.GetIdVariableTemplate(relationTypeRef);
                var idVariableTemplate = DataState.VariableTemplatesByRef[idVariableTemplateRef];
                var nameVariableTemplateRef = DataProvider.DependencyMap.GetNameVariableTemplate(relationTypeRef);
                var nameVariableTemplate = DataState.VariableTemplatesByRef[nameVariableTemplateRef];
                var sortingVariableTemplateRef = DataProvider.DependencyMap.GetOrderVariableTemplate(relationTypeRef);
                var sortingVariableTemplate = DataState.VariableTemplatesByRef[sortingVariableTemplateRef];

                var relationTypeRow = PrereqData.Decia_StructuralType.NewDecia_StructuralTypeRow();
                relationTypeRow.Id = relationType.ModelObjectId;
                relationTypeRow.Name = relationType.Name;
                relationTypeRow.SqlName = relationType.Name.ToEscaped_VarName();
                relationTypeRow.ObjectTypeId = (int)relationType.ModelObjectType;
                relationTypeRow.TreeLevel_Basic = DataProvider.StructuralMap.GetDistanceFromGlobal(relationTypeRef, false);
                relationTypeRow.TreeLevel_Extended = DataProvider.StructuralMap.GetDistanceFromGlobal(relationTypeRef, true);
                relationTypeRow.SetParent_StructuralTypeIdNull();
                relationTypeRow.Parent_IsNullable = false;
                relationTypeRow.SetParent_Default_InstanceIdNull();
                relationTypeRow.Instance_Table_Name = relationTypeRow.SqlName;
                relationTypeRow.Instance_Id_VariableTemplateId = idVariableTemplateRef.ModelObjectId;
                relationTypeRow.Instance_Name_VariableTemplateId = nameVariableTemplateRef.ModelObjectId;
                relationTypeRow.Instance_Sorting_VariableTemplateId = sortingVariableTemplateRef.ModelObjectId;
                relationTypeRow.SetInstance_ParentId_VariableTemplateIdNull();

                ExportState.AddStructuralTypeRow(relationTypeRef, relationTypeRow);
                FillWithVariableTemplates(DataProvider, ExportState, relationTypeRef, relationType_VariableTemplates);


                var relation_GenericTbl_TD0 = GenericDb.CreateTable(relationTypeRow.SqlName, true, false);
                relation_GenericTbl_TD0.SetStructuralTypeRef(relationTypeRef);
                ExportState.AddRootStructuralTable(relationTypeRef, relation_GenericTbl_TD0);

                var relation_IdRow = ExportState.GetVariableTemplateRow(idVariableTemplateRef);
                var relation_IdColumn = relation_GenericTbl_TD0.CreateColumn(relation_IdRow.SqlName, idVariableTemplate.DataType, false, true);
                relation_IdColumn.SetVariableTemplateRef(idVariableTemplateRef);
                relation_GenericTbl_TD0.SetPrimaryKey(relation_IdColumn.Id);
                relation_GenericTbl_TD0.SetColumn_ForId(relation_IdColumn.Id);
                relationType_VariableTemplates.Remove(idVariableTemplate);
                ExportState.AddVariableColumn(idVariableTemplateRef, relation_IdColumn);

                var relation_NameRow = ExportState.GetVariableTemplateRow(nameVariableTemplateRef);
                var relation_NameColumn = relation_GenericTbl_TD0.CreateColumn(relation_NameRow.SqlName, nameVariableTemplate.DataType, false, false);
                relation_NameColumn.SetVariableTemplateRef(nameVariableTemplateRef);
                relation_GenericTbl_TD0.SetColumn_ForName(relation_NameColumn.Id);
                relationType_VariableTemplates.Remove(nameVariableTemplate);
                ExportState.AddVariableColumn(nameVariableTemplateRef, relation_NameColumn);

                var relation_SortingRow = ExportState.GetVariableTemplateRow(sortingVariableTemplateRef);
                var relation_SortingColumn = relation_GenericTbl_TD0.CreateColumn(relation_SortingRow.SqlName, sortingVariableTemplate.DataType, true, false);
                relation_SortingColumn.SetVariableTemplateRef(sortingVariableTemplateRef);
                relation_GenericTbl_TD0.SetColumn_ForSorting(relation_SortingColumn.Id);
                relationType_VariableTemplates.Remove(sortingVariableTemplate);
                ExportState.AddVariableColumn(sortingVariableTemplateRef, relation_SortingColumn);

                var relatedIdVariableTemplates = relationType_VariableTemplates.Where(x => x.PredefinedVariableTemplateOption == PredefinedVariableTemplateOption.Id_Related).ToList();
                var relation_RelatedIdColumns = new Dictionary<Guid, GenericColumn>();
                var relation_UsedDimensions = relatedIdVariableTemplates.Select(x => x.RelatedEntityTypeRef.Value).GetStructuralTypeCounts();
                foreach (var relatedIdVariableTemplate in relatedIdVariableTemplates)
                {
                    var relatedIdVariableTemplateRef = relatedIdVariableTemplate.ModelObjectRef;
                    var relatedStructuralTypeRef = relatedIdVariableTemplate.RelatedEntityTypeRef.Value;
                    var parentTable = ExportState.GetRootStructuralTable(relatedStructuralTypeRef);
                    var dimensionNumber = (relation_UsedDimensions[relatedStructuralTypeRef] > ModelObjectReference.MinimumAlternateDimensionNumber) ? relatedStructuralTypeRef.NonNullAlternateDimensionNumber : (int?)null;

                    var relation_RelatedIdRow = ExportState.GetVariableTemplateRow(relatedIdVariableTemplateRef);
                    var relation_RelatedIdColumn = relation_GenericTbl_TD0.CreateColumn(relation_RelatedIdRow.SqlName, relatedIdVariableTemplate.DataType, false, false);
                    relation_RelatedIdColumn.SetVariableTemplateRef(relatedIdVariableTemplateRef);
                    relation_GenericTbl_TD0.AddDynamicForeignKey(parentTable.Id, relation_RelatedIdColumn.Id, parentTable.ColumnId_ForId.Value, dimensionNumber);
                    relation_RelatedIdColumns.Add(relation_RelatedIdColumn.Id, relation_RelatedIdColumn);
                    relationType_VariableTemplates.Remove(relatedIdVariableTemplate);
                    ExportState.AddVariableColumn(relatedIdVariableTemplateRef, relation_RelatedIdColumn);
                }
                relation_GenericTbl_TD0.AddUniqueKey(relation_RelatedIdColumns.Keys);
                relation_GenericTbl_TD0.SetMatrixTrigger(relation_RelatedIdColumns.Keys);

                CreateTablesAndColumnsForVariableTemplates(DataProvider, ExportState, relation_GenericTbl_TD0, relation_IdColumn, relationType_VariableTemplates);
            }

            foreach (var groupBucket in orderedGroups)
            {
                var groupIndex = groupBucket.Key;
                var group = groupBucket.Value;

                var groupRow = PrereqData.Decia_VariableTemplateGroup.NewDecia_VariableTemplateGroupRow();
                groupRow.Id = group.Id;
                groupRow.ProcessingIndex = groupIndex;
                groupRow.HasCycles = group.HasCycle;
                groupRow.HasUnresolvableCycles = (group.HasCycle && !group.HasStrictTimeOrdering);
                PrereqData.Decia_VariableTemplateGroup.AddDecia_VariableTemplateGroupRow(groupRow);

                int priority = 1;
                foreach (var groupedVariableTemplateRef in group.TimeOrderedNodes)
                {
                    var groupMemberRow = PrereqData.Decia_VariableTemplateGroupMember.NewDecia_VariableTemplateGroupMemberRow();
                    groupMemberRow.VariableTemplateGroupId = group.Id;
                    groupMemberRow.VariableTemplateId = groupedVariableTemplateRef.ModelObjectId;
                    groupMemberRow.Priority = priority;
                    PrereqData.Decia_VariableTemplateGroupMember.AddDecia_VariableTemplateGroupMemberRow(groupMemberRow);
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

                foreach (var dependencyRef in uniqueDependencyRefs)
                {
                    var dependencyRow = PrereqData.Decia_VariableTemplateDependency.NewDecia_VariableTemplateDependencyRow();
                    dependencyRow.Result_VariableTemplateId = variableTemplate.ModelObjectId;
                    dependencyRow.Dependency_VariableTemplateId = dependencyRef.ModelObjectId;
                    dependencyRow.Dependency_StructuralDimensionNumber = dependencyRef.NonNullAlternateDimensionNumber;
                    dependencyRow.IsStrict = (!group.NodesIncluded.Contains(dependencyRef));
                    PrereqData.Decia_VariableTemplateDependency.AddDecia_VariableTemplateDependencyRow(dependencyRow);
                }
            }

            foreach (var timePeriodType in usedTimePeriodTypes)
            {
                var timePeriodTypeRow = ExportState.GetTimePeriodTypeRow(timePeriodType);
                var timeframe = new TimeDimension(TimeDimensionType.Primary, TimeValueType.PeriodValue, timePeriodType, ExportState.StartDate, ExportState.EndDate);
                var timePeriods = timeframe.GeneratePeriodsForTimeDimension();
                var isForever = false;

                foreach (var timePeriod in timePeriods)
                {
                    var timePeriodRow = PrereqData.Decia_TimePeriod.NewDecia_TimePeriodRow();
                    timePeriodRow.Id = timePeriod.Id;
                    timePeriodRow.TimePeriodTypeId = timePeriodTypeRow.Id;
                    timePeriodRow.StartDate = timePeriod.StartDate;
                    timePeriodRow.EndDate = timePeriod.EndDate;
                    timePeriodRow.IsForever = isForever;
                    ExportState.AddTimePeriodRow(timePeriod, timePeriodType, timePeriodRow);
                }
            }

            var orchestrateProc = new OrchestrateComputation_GenericProcedure(GenericDb);
            orchestrateProc.ResultsCreationMethod = ExportInsertStatements;
            GenericDb.AddProcedure(orchestrateProc);

            foreach (var entityType in DataState.EntityTypes.Values.OrderBy(x => x.OrderValue))
            {
                var varName = entityType.Name.ToEscaped_VarName();
                varName = "@" + varName.Substring(0, 1).ToLower() + varName.Substring(1) + "Ids";
                var entityTypeRef = entityType.ModelObjectRef;
                var rootTableName = ExportState.GetRootStructuralTable(entityTypeRef).Name;

                orchestrateProc.AddInputParameter_ForStructuralInstanceIds(varName, DeciaDataType.Text, entityTypeRef, rootTableName);
            }
            foreach (var groupBucket in orderedGroups)
            {
                var group = groupBucket.Value;
                var groupId = group.Id;

                var variableTypes = group.TimeOrderedNodes.Select(x => DataProvider.GetVariableType(x)).Distinct().ToList();
                if ((variableTypes.Count <= 1) && (variableTypes.Contains(VariableType.Input)))
                { continue; }

                var variableTemplateRefs = group.TimeOrderedNodes.ToList();
                var firstVariableTemplateRef = variableTemplateRefs.First();
                var firstVariableTemplateRow = ExportState.GetVariableTemplateRow(firstVariableTemplateRef);
                var procNameBase = firstVariableTemplateRow.SqlName;

                var computeSubProc = orchestrateProc.AddCompute_SubProcedure(groupId, procNameBase, variableTemplateRefs);

                foreach (var mainVariableTemplateRef in variableTemplateRefs)
                {
                    var mainVariableTemplate = DataState.VariableTemplatesByRef[mainVariableTemplateRef];
                    var mainVariableTemplateIsStructuralAggr = (mainVariableTemplate.VariableType == VariableType.StructuralAggregationFormula);
                    var mainVariableTemplateIsTimeAggr = (mainVariableTemplate.VariableType == VariableType.TimeAggregationFormula);

                    var mainStructuralType = DataState.GetStructuralType(mainVariableTemplate.Containing_StructuralTypeId.ModelObjectRef);
                    var mainStructuralTypeRef = mainStructuralType.ModelObjectRef;
                    var navStructuralTypeRef = mainVariableTemplate.IsNavigationVariable ? mainVariableTemplate.RelatedEntityTypeRef.Value : (ModelObjectReference?)null;
                    var mainTimeDimensionality = DataProvider.GetValidatedTimeDimensions(mainVariableTemplateRef);

                    var mainColumn = ExportState.GetVariableColumn(mainVariableTemplateRef);
                    var mainTable = mainColumn.ParentTable;

                    Func<SqlDb_TargetType, string, string> computeMethod = delegate(SqlDb_TargetType sqlDbType, string baseIndent)
                    {
                        var formula = DataState.Formulas[mainVariableTemplate.FormulaId];
                        var formulaHasQuery = formula.Expressions.Values.Where(x => x.Operation is QueryOperation).HasContents();
                        var formulaInfo = new SqlFormulaInfo(sqlDbType, mainVariableTemplateRef, GenericDb);
                        formulaInfo.QueryType = formula.RootExpression.Operation.QueryType_ForSqlExport;

                        var requiredVariableTemplateRefs = formula.Arguments.Values.Where(x => x.ArgumentType == ArgumentType.ReferencedId).Select(x => x.ReferencedModelObject).ToHashSet(ModelObjectReference.DimensionalComparer);
                        var impliedNavVariableTemplateRefs = DataProvider.DependencyMap.GetImpliedNavigationVariableReferences(DataProvider.StructuralMap, mainVariableTemplateRef, true);
                        foreach (var navRef in impliedNavVariableTemplateRefs)
                        { requiredVariableTemplateRefs.Add(navRef); }

                        var requiredVariableTemplateStructuralTypeRefs = requiredVariableTemplateRefs.ToDictionary(x => x, x => DataProvider.DependencyMap.GetStructuralType(x), ModelObjectReference.DimensionalComparer);
                        var requiredStructuralTypeRefs = requiredVariableTemplateStructuralTypeRefs.Values.ToHashSet(ModelObjectReference.DimensionalComparer);

                        var joinPath = DataProvider.StructuralMap.GetJoinPath(mainStructuralTypeRef, requiredStructuralTypeRefs);
                        var joinSteps = joinPath.GetOrderedJoinSteps();

                        var orderedStructuralTypeRefs = joinSteps.Select(x => x.StructuralTypeRef).ToList();
                        var distinctStructuralTypeRefs = orderedStructuralTypeRefs.ToHashSet(ModelObjectReference.DimensionalComparer);

                        var orderedVariableTemplateRefs = new List<ModelObjectReference>();
                        var structuralTypesWithOrderedVariableTemplates = new Dictionary<ModelObjectReference, List<ModelObjectReference>>(ModelObjectReference.DimensionalComparer);
                        var structuralTypesWithDistinctVariableTemplates = new Dictionary<ModelObjectReference, HashSet<ModelObjectReference>>(ModelObjectReference.DimensionalComparer);

                        foreach (var orderedStructuralTypeRef in orderedStructuralTypeRefs)
                        {
                            structuralTypesWithOrderedVariableTemplates.Add(orderedStructuralTypeRef, new List<ModelObjectReference>());
                            structuralTypesWithDistinctVariableTemplates.Add(orderedStructuralTypeRef, new HashSet<ModelObjectReference>(ModelObjectReference.DimensionalComparer));

                            if (ModelObjectReference.DimensionalComparer.Equals(mainStructuralTypeRef, orderedStructuralTypeRef))
                            {
                                orderedVariableTemplateRefs.Add(mainVariableTemplateRef);
                                structuralTypesWithOrderedVariableTemplates[orderedStructuralTypeRef].Add(mainVariableTemplateRef);
                                structuralTypesWithDistinctVariableTemplates[orderedStructuralTypeRef].Add(mainVariableTemplateRef);
                            }

                            foreach (var requiredBucket in requiredVariableTemplateStructuralTypeRefs)
                            {
                                var requiredVariableTemplateRef = requiredBucket.Key;
                                var requiredStructuralTypeRef = requiredBucket.Value;

                                if (!ModelObjectReference.DimensionalComparer.Equals(orderedStructuralTypeRef, requiredStructuralTypeRef))
                                { continue; }
                                if (structuralTypesWithDistinctVariableTemplates[orderedStructuralTypeRef].Contains(requiredVariableTemplateRef))
                                { continue; }

                                orderedVariableTemplateRefs.Add(requiredVariableTemplateRef);
                                structuralTypesWithOrderedVariableTemplates[orderedStructuralTypeRef].Add(requiredVariableTemplateRef);
                                structuralTypesWithDistinctVariableTemplates[orderedStructuralTypeRef].Add(requiredVariableTemplateRef);
                            }
                        }

                        var allUsedTables = new HashSet<GenericTable>();
                        var orderedStructuralTypeRefsAndNames = orderedStructuralTypeRefs.ToDictionary(x => x, x => DataProvider.GetObjectName(x), ModelObjectReference.DimensionalComparer);
                        var orderedVariableTemplateRefsAndNames = orderedVariableTemplateRefs.ToDictionary(x => x, x => DataProvider.GetObjectName(x), ModelObjectReference.DimensionalComparer);
                        var orderedStructuralTableHashSets = new Dictionary<ModelObjectReference, HashSet<GenericTable>>(ModelObjectReference.DimensionalComparer);
                        var orderedStructuralTableLists = new Dictionary<ModelObjectReference, List<GenericTable>>(ModelObjectReference.DimensionalComparer);
                        var requiredTD1_TimePeriodTypesAndTableAliases = new SortedDictionary<TimePeriodType, KeyValuePair<string, string>>();
                        var requiredTD2_TimePeriodTypesAndTableAliases = new SortedDictionary<TimePeriodType, KeyValuePair<string, string>>();

                        foreach (var requiredVariableTemplateRef in orderedVariableTemplateRefs)
                        {
                            var requiredVariableTemplate = DataState.VariableTemplatesByRef[requiredVariableTemplateRef];
                            var requiredStructuralType = DataState.GetStructuralType(requiredVariableTemplate.Containing_StructuralTypeId.ModelObjectRef);
                            var requiredStructuralTypeRef = requiredStructuralType.ModelObjectRef;

                            if (!orderedStructuralTableHashSets.ContainsKey(requiredStructuralTypeRef))
                            { orderedStructuralTableHashSets.Add(requiredStructuralTypeRef, new HashSet<GenericTable>()); }

                            var requiredColumn = ExportState.GetVariableColumn(requiredVariableTemplateRef);
                            var requiredTable = requiredColumn.ParentTable;
                            var rootStructuralTable = ExportState.GetRootStructuralTable(requiredStructuralTypeRef);

                            allUsedTables.Add(requiredTable);
                            allUsedTables.Add(rootStructuralTable);
                            orderedStructuralTableHashSets[requiredStructuralTypeRef].Add(requiredTable);
                            orderedStructuralTableHashSets[requiredStructuralTypeRef].Add(rootStructuralTable);

                            var argInfo = new SqlArgumentInfo(requiredVariableTemplateRef, requiredStructuralTypeRef);
                            argInfo.VariableRef_Table = requiredTable;
                            argInfo.VariableRef_ColumnName = requiredColumn.Name;
                            argInfo.StructuralRef_Table = rootStructuralTable;
                            argInfo.StructuralRef_Id_ColumnName = rootStructuralTable.Column_ForId.Name;
                            argInfo.StructuralRef_Name_ColumnName = rootStructuralTable.Column_ForName.Name;
                            argInfo.StructuralRef_Order_ColumnName = rootStructuralTable.Column_ForSorting.Name;
                            argInfo.DataType_TableAlias = requiredVariableTemplateRef.HexBasedName + "_dt";
                            argInfo.ObjectType_TableAlias = requiredVariableTemplateRef.HexBasedName + "_ot";
                            argInfo.VariableTemplate_TableAlias = requiredVariableTemplateRef.HexBasedName + "_vt";
                            argInfo.StructuralType_TableAlias = requiredVariableTemplateRef.HexBasedName + "_st";

                            if (requiredTable.TD1_TimePeriodType.HasValue)
                            {
                                var tpt = requiredTable.TD1_TimePeriodType.Value;
                                var tpName = mainTable.Alias + string.Format("_tp1_{0}", tpt);
                                var tptName = mainTable.Alias + string.Format("_tpt1_{0}", tpt);

                                if (!requiredTD1_TimePeriodTypesAndTableAliases.ContainsKey(tpt))
                                { requiredTD1_TimePeriodTypesAndTableAliases.Add(tpt, new KeyValuePair<string, string>(tpName, tptName)); }

                                argInfo.TD1_TimePeriod_IsUsed = true;
                                argInfo.TD1_TimePeriod_TableAlias = tpName;

                                argInfo.TD1_TimePeriodType = tpt;
                                argInfo.TD1_TimePeriodType_TableAlias = tptName;
                            }

                            if (requiredTable.TD2_TimePeriodType.HasValue)
                            {
                                var tpt = requiredTable.TD2_TimePeriodType.Value;
                                var tpName = mainTable.Alias + string.Format("_tp2_{0}", tpt);
                                var tptName = mainTable.Alias + string.Format("_tpt2_{0}", tpt);

                                if (!requiredTD2_TimePeriodTypesAndTableAliases.ContainsKey(tpt))
                                { requiredTD2_TimePeriodTypesAndTableAliases.Add(tpt, new KeyValuePair<string, string>(tpName, tptName)); }

                                argInfo.TD2_TimePeriod_IsUsed = true;
                                argInfo.TD2_TimePeriod_TableAlias = tpName;

                                argInfo.TD2_TimePeriodType = tpt;
                                argInfo.TD2_TimePeriodType_TableAlias = tptName;
                            }

                            formulaInfo.ArgumentInfos.Add(requiredVariableTemplateRef, argInfo);
                        }

                        var orderedTD1_TimePeriodTypes = requiredTD1_TimePeriodTypesAndTableAliases.Keys.OrderBy(x => x).ToList();
                        var orderedTD2_TimePeriodTypes = requiredTD2_TimePeriodTypesAndTableAliases.Keys.OrderBy(x => x).ToList();

                        foreach (var bucket in orderedStructuralTableHashSets)
                        {
                            var orderedTables = bucket.Value.OrderBy(x => (x.OrderingValue)).ToList();
                            orderedStructuralTableLists.Add(bucket.Key, orderedTables);
                        }

                        var hasRowNumber = false;
                        var rowNumberColumnName = "RowNumber";
                        var valueColumnName = "Value";
                        var srcTableAlias = "src_table";
                        var aggrTableAlias = "aggr_table";

                        var mainArgInfo = formulaInfo.ArgumentInfos[mainVariableTemplateRef];
                        var currentState = DataState.GetVariableTemplateState(mainVariableTemplateRef);
                        var selectText = formula.RenderAsSqlSelect(DataProvider, currentState, formulaInfo);

                        var selectIdsText = string.Empty;
                        var partitionIdsText = string.Empty;
                        var joinIdsText = string.Empty;
                        var whereIdsText = string.Empty;
                        var isFirstId = true;

                        var mainStructuralColumn = (GenericColumn)null;
                        var mainTimeDim1Column = (GenericColumn)null;
                        var mainTimeDim2Column = (GenericColumn)null;
                        var mainResultSetColumn = (GenericColumn)null;

                        foreach (var column in mainTable.PrimaryKey.Columns)
                        {
                            if (isFirstId)
                            { isFirstId = false; }
                            else
                            {
                                selectIdsText += ", ";
                                partitionIdsText += ", ";
                                joinIdsText += " AND ";

                                if (!string.IsNullOrWhiteSpace(whereIdsText))
                                { whereIdsText += " AND "; }
                            }

                            selectIdsText += string.Format("{0}.[{1}]", mainTable.Alias, column.Name);
                            partitionIdsText += string.Format("{0}.[{1}]", mainTable.Alias, column.Name);
                            joinIdsText += string.Format("({0}.[{1}] = {2}.[{1}])", srcTableAlias, column.Name, aggrTableAlias, column.Name);

                            if ((column == mainTable.Column_ForId) || (column == mainTable.Column_ForStructure))
                            {
                                mainStructuralColumn = column;
                                whereIdsText += string.Empty;
                            }
                            else if (column == mainTable.Column_ForTimeD1)
                            {
                                mainTimeDim1Column = column;
                                whereIdsText += string.Format("({0}.[{1}] LIKE {2})", mainTable.Alias, column.Name, ComputeVariableTemplateGroup_GenericProcedure.TemporaryVariableName_TD1_TimePeriodId);
                            }
                            else if (column == mainTable.Column_ForTimeD2)
                            {
                                mainTimeDim2Column = column;
                                whereIdsText += string.Format("({0}.[{1}] LIKE {2})", mainTable.Alias, column.Name, ComputeVariableTemplateGroup_GenericProcedure.TemporaryVariableName_TD2_TimePeriodId);
                            }
                            else if (column == mainTable.Column_ForResultSet)
                            {
                                mainResultSetColumn = column;
                                whereIdsText += string.Format("({0}.[{1}] LIKE {2})", mainTable.Alias, column.Name, ComputeVariableTemplateGroup_GenericProcedure.InputParameterName_ResultSetId);
                            }
                            else
                            { throw new InvalidOperationException("Unexpected Primary Key Column encountered."); }
                        }

                        var innerQueryDef = string.Empty;
                        var innerQuery_SelectText = string.Empty;
                        var innerQuery_MainTps_JoinText = string.Empty;
                        var innerQuery_ShiftedTps_JoinText = string.Empty;
                        var innerQuery_Metadata_JoinText = string.Empty;
                        var innerQuery_Structural_JoinText = string.Empty;
                        var innerQuery_ShiftedTps_WhereText = string.Empty;
                        var innerQuery_GroupByText = string.Empty;

                        if (formulaInfo.QueryType == SqlQueryType.SimpleSelect)
                        {
                            innerQuery_SelectText += string.Format("SELECT {0}, {1} AS [{2}]", selectIdsText, selectText, valueColumnName);
                        }
                        else if (formulaInfo.QueryType == SqlQueryType.SelectAggregation)
                        {
                            innerQuery_SelectText += string.Format("SELECT {0}, {1} AS [{2}]", selectIdsText, selectText, valueColumnName);
                            innerQuery_GroupByText += string.Format("GROUP BY {0}", selectIdsText);
                        }
                        else if (formulaInfo.QueryType == SqlQueryType.SelectOneFromMany)
                        {
                            hasRowNumber = true;
                            var partitionText = string.Format("ROW_NUMBER() OVER (PARTITION BY {0} {1}) AS [{2}]", partitionIdsText, formulaInfo.RowLimiting_OrderByText, rowNumberColumnName);
                            innerQuery_SelectText += string.Format("SELECT {0}, {1}, {2} AS [{3}]", selectIdsText, partitionText, selectText, valueColumnName);
                        }
                        else
                        { throw new InvalidOperationException("Invalid SqlQueryType encountered."); }

                        foreach (var td1TimePeriodType in orderedTD1_TimePeriodTypes)
                        {
                            var currentTd1TableAliases = requiredTD1_TimePeriodTypesAndTableAliases[td1TimePeriodType];
                            var currentTpTableAlias = currentTd1TableAliases.Key;
                            var currentTptTableAlias = currentTd1TableAliases.Value;

                            var tpJoinText = baseIndent + GenericDatabaseUtils.Indent_L6;
                            var tptJoinText = baseIndent + GenericDatabaseUtils.Indent_L6;

                            if (mainTimeDim1Column == null)
                            {
                                tpJoinText += string.Format("INNER JOIN {0} {1} ON ({2} = {1}.[{3}])", DeciaBaseUtils.Included_TimePeriod_TableName, currentTpTableAlias, Convert.ToInt32(td1TimePeriodType), DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName) + Environment.NewLine;
                            }
                            else
                            {
                                if (td1TimePeriodType == mainTable.TD1_TimePeriodType)
                                {
                                    tpJoinText += string.Format("INNER JOIN {0} {1} ON ({2}.[{3}] = {1}.[{4}])", DeciaBaseUtils.Included_TimePeriod_TableName, currentTpTableAlias, mainTable.Alias, mainTimeDim1Column.Name, DeciaBaseUtils.Included_TimePeriod_Id_ColumnName) + Environment.NewLine;
                                }
                                else
                                {
                                    var mainTpTableAlias = mainTable.Alias + string.Format("_tp1_{0}", mainTable.TD1_TimePeriodType);
                                    tpJoinText += string.Format("INNER JOIN {0} {1} ON ({2} = {1}.[{3}]) AND ({4}.[{5}] <= {1}.[{5}]) AND ({4}.[{6}] <= {1}.[{6}])", DeciaBaseUtils.Included_TimePeriod_TableName, currentTpTableAlias, Convert.ToInt32(td1TimePeriodType), DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName, mainTpTableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName) + Environment.NewLine;
                                }
                            }
                            tptJoinText += string.Format("INNER JOIN [dbo].[{0}] {1} ON ({2}.[{3}] = {1}.[{4}])", DeciaBaseUtils.DeciaBase_Schema.Decia_TimePeriodType.TableName, currentTptTableAlias, currentTpTableAlias, DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName, DeciaBaseUtils.DeciaBase_Schema.Decia_TimePeriodType.IdColumn.ColumnName) + Environment.NewLine;

                            innerQuery_MainTps_JoinText += tpJoinText + tptJoinText;
                        }

                        foreach (var td2TimePeriodType in orderedTD2_TimePeriodTypes)
                        {
                            var currentTd2TableAliases = requiredTD2_TimePeriodTypesAndTableAliases[td2TimePeriodType];
                            var currentTpTableAlias = currentTd2TableAliases.Key;
                            var currentTptTableAlias = currentTd2TableAliases.Value;

                            var tpJoinText = baseIndent + GenericDatabaseUtils.Indent_L6;
                            var tptJoinText = baseIndent + GenericDatabaseUtils.Indent_L6;

                            if (mainTimeDim2Column == null)
                            {
                                tpJoinText += string.Format("INNER JOIN {0} {1} ON ({2} = {1}.[{3}])", DeciaBaseUtils.Included_TimePeriod_TableName, currentTpTableAlias, Convert.ToInt32(td2TimePeriodType), DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName) + Environment.NewLine;
                            }
                            else
                            {
                                if (td2TimePeriodType == mainTable.TD2_TimePeriodType)
                                {
                                    tpJoinText += string.Format("INNER JOIN {0} {1} ON ({2}.[{3}] = {1}.[{4}])", DeciaBaseUtils.Included_TimePeriod_TableName, currentTpTableAlias, mainTable.Alias, mainTimeDim2Column.Name, DeciaBaseUtils.Included_TimePeriod_Id_ColumnName) + Environment.NewLine;
                                }
                                else
                                {
                                    var mainTpTableAlias = mainTable.Alias + string.Format("_tp2_{0}", mainTable.TD2_TimePeriodType);
                                    tpJoinText += string.Format("INNER JOIN {0} {1} ON ({2} = {1}.[{3}]) AND ({4}.[{5}] <= {1}.[{5}]) AND ({4}.[{6}] <= {1}.[{6}])", DeciaBaseUtils.Included_TimePeriod_TableName, currentTpTableAlias, Convert.ToInt32(td2TimePeriodType), DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName, mainTpTableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName) + Environment.NewLine;
                                }
                            }
                            tptJoinText += string.Format("INNER JOIN [dbo].[{0}] {1} ON ({2}.[{3}] = {1}.[{4}])", DeciaBaseUtils.DeciaBase_Schema.Decia_TimePeriodType.TableName, currentTptTableAlias, currentTpTableAlias, DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName, DeciaBaseUtils.DeciaBase_Schema.Decia_TimePeriodType.IdColumn.ColumnName) + Environment.NewLine;

                            innerQuery_MainTps_JoinText += tpJoinText + tptJoinText;
                        }

                        foreach (var overrideBucket in formulaInfo.ArgumentOverrideInfos)
                        {
                            if (overrideBucket.Value.TD1_HasShiftExpression)
                            {
                                innerQuery_ShiftedTps_JoinText += baseIndent + GenericDatabaseUtils.Indent_L6;

                                var tableName = DeciaBaseUtils.Included_TimePeriod_TableName;
                                var tableAlias = overrideBucket.Value.TD1_TimePeriod_TableAlias;
                                var timePeriodType = overrideBucket.Value.TD1_TimePeriodType.Value;
                                innerQuery_ShiftedTps_JoinText += string.Format("INNER JOIN {0} {1} ON ({2} = {1}.[{3}])", tableName, tableAlias, Convert.ToInt32(timePeriodType), DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName);

                                if (!overrideBucket.Value.TD1_ShiftExpression_WorksAsJoinOnText)
                                {
                                    if (string.IsNullOrWhiteSpace(innerQuery_ShiftedTps_WhereText))
                                    { innerQuery_ShiftedTps_WhereText += overrideBucket.Value.TD1_ShiftExpression_WhereText; }
                                    else
                                    { innerQuery_ShiftedTps_WhereText += string.Format(" AND {0}", overrideBucket.Value.TD1_ShiftExpression_WhereText); }
                                }
                                else
                                { innerQuery_ShiftedTps_JoinText += string.Format(" AND {0}", overrideBucket.Value.TD1_ShiftExpression_WhereText); }

                                innerQuery_ShiftedTps_JoinText += Environment.NewLine;
                            }

                            if (overrideBucket.Value.TD2_HasShiftExpression)
                            {
                                innerQuery_ShiftedTps_JoinText += baseIndent + GenericDatabaseUtils.Indent_L6;

                                var tableName = DeciaBaseUtils.Included_TimePeriod_TableName;
                                var tableAlias = overrideBucket.Value.TD2_TimePeriod_TableAlias;
                                var timePeriodType = overrideBucket.Value.TD2_TimePeriodType.Value;
                                innerQuery_ShiftedTps_JoinText += string.Format("INNER JOIN {0} {1} ON ({2} = {1}.[{3}])", tableName, tableAlias, Convert.ToInt32(timePeriodType), DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName);

                                if (!overrideBucket.Value.TD2_ShiftExpression_WorksAsJoinOnText)
                                {
                                    if (string.IsNullOrWhiteSpace(innerQuery_ShiftedTps_WhereText))
                                    { innerQuery_ShiftedTps_WhereText += overrideBucket.Value.TD2_ShiftExpression_WhereText; }
                                    else
                                    { innerQuery_ShiftedTps_WhereText += string.Format(" AND {0}", overrideBucket.Value.TD2_ShiftExpression_WhereText); }
                                }
                                else
                                { innerQuery_ShiftedTps_JoinText += string.Format(" AND {0}", overrideBucket.Value.TD2_ShiftExpression_WhereText); }

                                innerQuery_ShiftedTps_JoinText += Environment.NewLine;
                            }
                        }

                        var argInfosByStructuralTypeRef = new Dictionary<ModelObjectReference, List<SqlArgumentInfo>>(ModelObjectReference.DimensionalComparer);
                        var tableGroups = new Dictionary<ModelObjectReference, StructuralTableGroup>(ModelObjectReference.DimensionalComparer);

                        foreach (var argInfo in formulaInfo.ArgumentInfos.Values)
                        {
                            var structuralTypeRef = argInfo.StructuralRef;

                            if (!argInfosByStructuralTypeRef.ContainsKey(structuralTypeRef))
                            { argInfosByStructuralTypeRef.Add(structuralTypeRef, new List<SqlArgumentInfo>()); }

                            argInfosByStructuralTypeRef[structuralTypeRef].Add(argInfo);
                        }
                        foreach (var argInfo in formulaInfo.ArgumentOverrideInfos.Values)
                        {
                            var structuralTypeRef = argInfo.StructuralRef;

                            if (!argInfosByStructuralTypeRef.ContainsKey(structuralTypeRef))
                            { argInfosByStructuralTypeRef.Add(structuralTypeRef, new List<SqlArgumentInfo>()); }

                            argInfosByStructuralTypeRef[structuralTypeRef].Add(argInfo);
                        }

                        foreach (var structuralTypeRef in orderedStructuralTypeRefs)
                        {
                            var isMainGroup = ModelObjectReference.DimensionalComparer.Equals(mainStructuralTypeRef, structuralTypeRef);
                            var rootTable = ExportState.GetRootStructuralTable(structuralTypeRef);

                            var tableGroup = new StructuralTableGroup(isMainGroup, structuralTypeRef, rootTable);
                            tableGroups.Add(structuralTypeRef, tableGroup);

                            if (!argInfosByStructuralTypeRef.ContainsKey(structuralTypeRef))
                            { continue; }

                            foreach (var argInfo in argInfosByStructuralTypeRef[structuralTypeRef])
                            {
                                if (argInfo.VariableRef_Table == rootTable)
                                { continue; }

                                var tableAlias = argInfo.VariableRef_TableAlias;
                                var tableValue = (StructuralTableValue)null;

                                if (tableGroup.SubTableValuesByAlias.ContainsKey(tableAlias))
                                {
                                    tableValue = tableGroup.SubTableValuesByAlias[tableAlias];
                                    tableValue.ArgInfos.Add(argInfo);
                                    continue;
                                }

                                tableValue = new StructuralTableValue(tableAlias, argInfo.VariableRef_Table);
                                tableValue.ArgInfos.Add(argInfo);
                                tableGroup.SubTableValuesByAlias.Add(tableAlias, tableValue);
                            }
                        }

                        var variableTemplatesAvailableByStructuralTypeRef = joinPath.AllTypeRefs_Ordered.ToDictionary(x => x, x => DataState.GetVariableTemplates(x, null).Values.ToList(), ModelObjectReference.DimensionalComparer);
                        var previousTableGroup = (StructuralTableGroup)null;
                        var tableGroupsHandled = new Dictionary<ModelObjectReference, StructuralTableGroup>(ModelObjectReference.DimensionalComparer);
                        var tablesHandled = new Dictionary<string, GenericTable>();
                        var dimensionJoinStates = joinPath.TotalSpace_Maximal.Dimensions.ToDictionary(x => x.EntityTypeRefWithDimNum, x => new DimensionTableJoinState(x.EntityTypeRefWithDimNum), ModelObjectReference.DimensionalComparer);

                        foreach (var joinStep in joinSteps)
                        {
                            var tableGroupRef = joinStep.StructuralTypeRef;
                            var tableGroup = tableGroups[tableGroupRef];
                            var tableGroup_JoinText = string.Empty;
                            var wasTableGroupHandled = tableGroupsHandled.ContainsKey(tableGroupRef);

                            if (wasTableGroupHandled)
                            { throw new InvalidOperationException("The Structural Table Group has already been handled."); }

                            var rootTable = tableGroup.RootTable;
                            var wasRootTableHandled = tablesHandled.ContainsKey(rootTable.Alias);

                            if (wasRootTableHandled)
                            { throw new InvalidOperationException("The Root Structural Table has already been handled."); }

                            var variableTemplateRefsForGroup = DataProvider.DependencyMap.GetVariableTemplates(tableGroupRef);
                            var variableTemplatesForGroup = variableTemplateRefsForGroup.ToDictionary(x => x, x => DataState.VariableTemplatesByRef[x], ModelObjectReference.DimensionalComparer);
                            var variableTemplatesForGroup_Joinable = variableTemplatesForGroup.Where(x => x.Value.IsVariableTemplateJoinable()).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer);
                            var variableTemplatesForGroup_RootJoinable = variableTemplatesForGroup_Joinable.Where(x => rootTable.HasColumn(x.Key)).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer);

                            var handleJoinThroughFormula = false;
                            if (mainVariableTemplateIsStructuralAggr && formulaHasQuery)
                            {
                                if (DataProvider.StructuralMap.IsAccessible(mainStructuralTypeRef, tableGroupRef, true)
                                    && !DataProvider.StructuralMap.IsUnique(mainStructuralTypeRef, tableGroupRef, true))
                                { handleJoinThroughFormula = true; }
                            }

                            tableGroup_JoinText += baseIndent + GenericDatabaseUtils.Indent_L6;

                            if (tableGroup.IsMainGroup)
                            {
                                tableGroup_JoinText += string.Format("INNER JOIN [dbo].[{0}] {1} ON ({2}.[{3}] = {1}.[{4}])", rootTable.Name, rootTable.Alias, mainTable.Alias, mainTable.Column_ForStructure.Name, rootTable.Column_ForId.Name);
                            }
                            else if (tableGroupRef.ModelObjectType == ModelObjectType.GlobalType)
                            {
                                tableGroup_JoinText += string.Format("INNER JOIN [dbo].[{0}] {1} ON (1 = 1)", rootTable.Name, rootTable.Alias);
                            }
                            else if ((mainStructuralTypeRef.ModelObjectType == ModelObjectType.GlobalType) && (joinStep.OnlyCreatesJoinSettings()))
                            {
                                tableGroup_JoinText += string.Format("INNER JOIN [dbo].[{0}] {1} ON (1 = 1)", rootTable.Name, rootTable.Alias);
                            }
                            else if (navStructuralTypeRef.HasValue && (ModelObjectReference.DimensionalComparer.Equals(tableGroupRef, navStructuralTypeRef.Value)))
                            {
                                tableGroup_JoinText += string.Format("LEFT OUTER JOIN [dbo].[{0}] {1} ON (1 = 1)", rootTable.Name, rootTable.Alias);
                            }
                            else if (handleJoinThroughFormula)
                            {
                                tableGroup_JoinText += string.Format("INNER JOIN [dbo].[{0}] {1} ON (1 = 1)", rootTable.Name, rootTable.Alias);
                            }
                            else
                            {
                                var isFromNullableIdToIds = variableTemplatesForGroup_RootJoinable.Values.ToDictionary(x => x, x => dimensionJoinStates[x.GetVariableTemplate_JoinableEntityTypeRef().Value].IsDimensionNullable);
                                var isFromIdToNullableIds = variableTemplatesForGroup_RootJoinable.Values.ToDictionary(x => x, x => (x.IsVariableTemplateJoinableAndNullable() && !joinStep.CreatesJoinSetting(x.GetVariableTemplate_JoinableEntityTypeRef().Value)));

                                var isFromNullableIdToId = (isFromNullableIdToIds.Where(x => x.Value == true).Count() > 0);
                                var isFromIdToNullableId = (isFromIdToNullableIds.Where(x => x.Value == true).Count() > 0);
                                var isNullable = (isFromNullableIdToId || isFromIdToNullableId);

                                var pathAlreadyUsesOuterJoin = false;
                                var nonNullIdsToCheckForOuterJoin = isFromIdToNullableIds.Where(x => !x.Key.IsNavigationVariable && !x.Value).Select(x => x.Key).ToDictionary(x => x.ModelObjectRef, x => x);

                                var existingRootDimensions = variableTemplatesForGroup_RootJoinable.ToDictionary(x => x.Value.GetVariableTemplate_JoinableEntityTypeRef().Value, x => x.Key, ModelObjectReference.DimensionalComparer).Where(x => !joinStep.CreatesJoinSetting(x.Key)).ToList();
                                var joinConditionsText = "(1 = 1)";

                                foreach (var existingRootDimension in existingRootDimensions)
                                {
                                    var joinState = dimensionJoinStates[existingRootDimension.Key];

                                    if (!joinState.HasTraversalValue)
                                    { continue; }

                                    var sourceId = (joinState.HasRootValue) ? joinState.RootValue : joinState.TraversalValue;
                                    var localVariableTemplateRef = existingRootDimension.Value;
                                    var localId = string.Format("{0}.[{1}]", rootTable.Alias, rootTable.GetColumn(localVariableTemplateRef).Name);
                                    joinConditionsText += string.Format(" AND ({0} = {1})", sourceId, localId);

                                    if (existingRootDimension.Key == tableGroupRef)
                                    {
                                        joinState.SetRootValue(localVariableTemplateRef, localId);
                                        joinState.IsDimensionNullable = (joinState.IsDimensionNullable || isNullable);
                                    }

                                    foreach (var relatedVariableTemplate in nonNullIdsToCheckForOuterJoin.Values)
                                    {
                                        var relatedStructuralTypeRef = (relatedVariableTemplate.PredefinedVariableTemplateOption == PredefinedVariableTemplateOption.Id) ? relatedVariableTemplate.Containing_StructuralTypeId.ModelObjectRef : relatedVariableTemplate.RelatedEntityTypeRef.Value;

                                        if (!ModelObjectReference.DimensionalComparer.Equals(existingRootDimension.Key, relatedStructuralTypeRef))
                                        { continue; }

                                        var traversalVariableTemplateRef = joinState.TraversalVariableTemplateRef;
                                        var traversalStructuralTypeRef = ExportState.DataProvider.DependencyMap.GetStructuralType(traversalVariableTemplateRef);

                                        var relatedTableGroup = tableGroups[traversalStructuralTypeRef];

                                        if (relatedTableGroup.UseOuterJoin)
                                        { pathAlreadyUsesOuterJoin = true; }
                                    }
                                }

                                tableGroup.UseOuterJoin = (isNullable && !pathAlreadyUsesOuterJoin);

                                var joinOpText = (tableGroup.UseOuterJoin) ? "LEFT OUTER JOIN" : "INNER JOIN";
                                tableGroup_JoinText += string.Format("{0} [dbo].[{1}] {2} ON {3}", joinOpText, rootTable.Name, rootTable.Alias, joinConditionsText);
                            }

                            if (tableGroupRef.ModelObjectType == ModelObjectType.EntityType)
                            {
                                if (!dimensionJoinStates[tableGroupRef].HasRootValue)
                                {
                                    var joinState = dimensionJoinStates[tableGroupRef];
                                    var localColumn = rootTable.Column_ForId;
                                    var localVariableTemplateRef = localColumn.VariableTemplateRef.Value;
                                    var localId = string.Format("{0}.[{1}]", rootTable.Alias, localColumn.Name);
                                    joinState.SetRootValue(localVariableTemplateRef, localId);
                                }
                            }

                            var newRootDimensions = variableTemplatesForGroup_RootJoinable.ToDictionary(x => x.Value.GetVariableTemplate_JoinableEntityTypeRef().Value, x => x.Key, ModelObjectReference.DimensionalComparer).Where(x => joinStep.CreatesJoinSetting(x.Key)).ToList();
                            foreach (var newRootDimension in newRootDimensions)
                            {
                                var joinState = dimensionJoinStates[newRootDimension.Key];
                                var variableTemplate = variableTemplatesForGroup_RootJoinable[newRootDimension.Value];
                                var localVariableTemplateRef = newRootDimension.Value;
                                var localId = string.Format("{0}.[{1}]", rootTable.Alias, rootTable.GetColumn(localVariableTemplateRef).Name);

                                var variableTemplatesUsedByStructuralTypeRef = variableTemplatesAvailableByStructuralTypeRef.ToDictionary(x => x.Key, x => x.Value.Where(y => ((y.PredefinedVariableTemplateOption == PredefinedVariableTemplateOption.Id_Parent) || y.IsNavigationVariable) && ModelObjectReference.DimensionalComparer.Equals(tableGroupRef, y.RelatedEntityTypeRef.Value)).ToList(), ModelObjectReference.DimensionalComparer);
                                var variableTemplatesCountsByStructuralTypeRef = variableTemplatesUsedByStructuralTypeRef.ToDictionary(x => x.Key, x => x.Value.Count(), ModelObjectReference.DimensionalComparer);

                                var isFromNullableIdToId = variableTemplate.IsVariableTemplateJoinableAndNullable();
                                var isFromIdToNullableId = (variableTemplatesCountsByStructuralTypeRef.Where(x => x.Value > 0).Count() > 0);
                                var isNullable = (isFromNullableIdToId || isFromIdToNullableId);

                                joinState.SetTraversalValue(localVariableTemplateRef, localId);
                                joinState.IsDimensionNullable = isNullable;

                                if (ModelObjectReference.DimensionalComparer.Equals(newRootDimension.Key, tableGroupRef))
                                { joinState.SetRootValue(localVariableTemplateRef, localId); }
                            }
                            tableGroup_JoinText += Environment.NewLine;
                            tablesHandled.Add(rootTable.Alias, rootTable);

                            if (tableGroup.IsMainGroup && (rootTable != mainTable))
                            {
                                var wasMainTableHandled = tablesHandled.ContainsKey(mainTable.Alias);

                                if (wasMainTableHandled)
                                { throw new InvalidOperationException("The Main Structural Table has already been handled."); }

                                var variableTemplatesForGroup_MainJoinable = variableTemplatesForGroup_Joinable.Where(x => mainTable.HasColumn(x.Key)).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer);
                                var newMainDimensions = variableTemplatesForGroup_MainJoinable.ToDictionary(x => x.Value.GetVariableTemplate_JoinableEntityTypeRef().Value, x => x.Key, ModelObjectReference.DimensionalComparer).Where(x => joinStep.CreatesJoinSetting(x.Key)).ToList();
                                foreach (var newMainDimension in newMainDimensions)
                                {
                                    var joinState = dimensionJoinStates[newMainDimension.Key];

                                    if (joinState.HasRootValue)
                                    { continue; }

                                    var variableTemplate = variableTemplatesForGroup_MainJoinable[newMainDimension.Value];
                                    var localVariableTemplateRef = newMainDimension.Value;
                                    var localId = string.Format("{0}.[{1}]", mainTable.Alias, mainTable.GetColumn(localVariableTemplateRef).Name);

                                    joinState.SetTraversalValue(localVariableTemplateRef, localId);
                                    joinState.IsDimensionNullable = variableTemplate.IsVariableTemplateJoinableAndNullable();
                                }
                                tablesHandled.Add(mainTable.Alias, mainTable);
                            }

                            foreach (var subTableBucket in tableGroup.SubTableValuesByAlias)
                            {
                                var subTableAlias = subTableBucket.Key;
                                var subTableValue = subTableBucket.Value;
                                var subTable = subTableValue.Table;

                                if (tablesHandled.ContainsKey(subTableAlias))
                                { continue; }
                                if (subTableBucket.Value.ArgInfos.Select(x => x.VariableRef_TableAlias).Distinct().Count() != 1)
                                { throw new InvalidOperationException("Invalid join conditions encountered."); }

                                var variableTemplatesForGroup_NestedJoinable = variableTemplatesForGroup_Joinable.Where(x => subTable.HasColumn(x.Key)).ToDictionary(x => x.Key, x => x.Value, ModelObjectReference.DimensionalComparer);
                                var existingNestedDimensions = variableTemplatesForGroup_NestedJoinable.ToDictionary(x => x.Value.GetVariableTemplate_JoinableEntityTypeRef().Value, x => x.Key, ModelObjectReference.DimensionalComparer).Where(x => !joinStep.CreatesJoinSetting(x.Key)).ToList();
                                var newNestedDimensions = variableTemplatesForGroup_NestedJoinable.ToDictionary(x => x.Value.GetVariableTemplate_JoinableEntityTypeRef().Value, x => x.Key, ModelObjectReference.DimensionalComparer).Where(x => joinStep.CreatesJoinSetting(x.Key)).ToList();

                                var joinOpText = "INNER JOIN";
                                var structuralIdColumnRef = string.Format("{0}.[{1}]", rootTable.Alias, rootTable.Column_ForId.Name);
                                var resultSetColumnRef = string.Format("{0}.[{1}]", mainTable.Alias, mainTable.Column_ForResultSet.Name);
                                var joinConditionsText = subTableValue.GetJoinConditionsText(structuralIdColumnRef, resultSetColumnRef);

                                foreach (var existingNestedDimension in existingNestedDimensions)
                                {
                                    var joinState = dimensionJoinStates[existingNestedDimension.Key];
                                    var sourceId = (joinState.HasRootValue) ? joinState.RootValue : joinState.TraversalValue;
                                    var localId = string.Format("{0}.[{1}]", subTableAlias, subTable.GetColumn(existingNestedDimension.Value).Name);
                                    joinConditionsText += string.Format(" AND ({0} = {1})", sourceId, localId);

                                    if (joinState.IsDimensionNullable)
                                    { joinOpText = "LEFT OUTER JOIN"; }
                                }

                                tableGroup_JoinText += baseIndent + GenericDatabaseUtils.Indent_L6;
                                tableGroup_JoinText += string.Format("{0} [dbo].[{1}] {2} ON {3}", joinOpText, subTable.Name, subTableAlias, joinConditionsText);
                                tableGroup_JoinText += Environment.NewLine;

                                foreach (var newNestedDimension in newNestedDimensions)
                                {
                                    var joinState = dimensionJoinStates[newNestedDimension.Key];

                                    if (joinState.HasRootValue)
                                    { continue; }

                                    var localVariableTemplateRef = newNestedDimension.Value;
                                    var localVariableTemplate = variableTemplatesForGroup_NestedJoinable[localVariableTemplateRef];
                                    var localId = string.Format("{0}.[{1}]", subTable.Alias, subTable.GetColumn(localVariableTemplateRef).Name);

                                    joinState.SetTraversalValue(localVariableTemplateRef, localId);

                                    joinState.IsDimensionNullable = localVariableTemplate.IsVariableTemplateJoinableAndNullable();
                                }
                                tablesHandled.Add(subTableAlias, subTable);
                            }

                            innerQuery_Structural_JoinText += tableGroup_JoinText;
                            tableGroupsHandled.Add(tableGroupRef, tableGroup);
                            previousTableGroup = tableGroup;
                        }

                        innerQuery_Metadata_JoinText = GenerateMetadataJoinText((baseIndent + GenericDatabaseUtils.Indent_L6), formulaInfo);

                        innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L4 + innerQuery_SelectText + Environment.NewLine;

                        innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L5 + string.Format("FROM [dbo].[{0}] {1}", mainTable.Name, mainTable.Alias) + Environment.NewLine;
                        innerQueryDef += innerQuery_MainTps_JoinText;
                        innerQueryDef += innerQuery_ShiftedTps_JoinText;
                        innerQueryDef += innerQuery_Metadata_JoinText;
                        innerQueryDef += innerQuery_Structural_JoinText;

                        innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L5 + "WHERE (1 = 1)" + Environment.NewLine;
                        if (!string.IsNullOrWhiteSpace(whereIdsText))
                        { innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L6 + string.Format("AND ({0})", whereIdsText) + Environment.NewLine; }
                        if (!string.IsNullOrWhiteSpace(innerQuery_ShiftedTps_WhereText))
                        { innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L6 + string.Format("AND ({0})", innerQuery_ShiftedTps_WhereText) + Environment.NewLine; }
                        if (formulaInfo.HasFilter)
                        { innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L6 + string.Format("AND ({0})", formulaInfo.Filtering_WhereText) + Environment.NewLine; }
                        if (formulaInfo.HasAggregation_PrimaryDate)
                        {
                            var startDate = string.Format("{0}.[{1}]", mainArgInfo.TD1_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName);
                            var endDate = string.Format("{0}.[{1}]", mainArgInfo.TD1_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName);
                            innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L6 + string.Format("AND (({0} <= {1}) AND ({1} <= {2}))", startDate, formulaInfo.Aggregation_PrimaryDateText, endDate) + Environment.NewLine;
                        }
                        if (formulaInfo.HasAggregation_SecondaryDate)
                        {
                            var startDate = string.Format("{0}.[{1}]", mainArgInfo.TD2_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_StartDate_ColumnName);
                            var endDate = string.Format("{0}.[{1}]", mainArgInfo.TD2_TimePeriod_TableAlias, DeciaBaseUtils.Included_TimePeriod_EndDate_ColumnName);
                            innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L6 + string.Format("AND (({0} <= {1}) AND ({1} <= {2}))", startDate, formulaInfo.Aggregation_SecondaryDateText, endDate) + Environment.NewLine;
                        }

                        if (!string.IsNullOrWhiteSpace(innerQuery_GroupByText))
                        { innerQueryDef += baseIndent + GenericDatabaseUtils.Indent_L5 + innerQuery_GroupByText + Environment.NewLine; }


                        var updateDef = string.Empty;
                        updateDef += baseIndent + GenericDatabaseUtils.Indent_L0 + string.Format("UPDATE {0}", srcTableAlias) + Environment.NewLine;
                        if (formulaInfo.QueryType == SqlQueryType.SelectAggregation)
                        { updateDef += baseIndent + GenericDatabaseUtils.Indent_L1 + string.Format("SET [{0}] = CASE WHEN ({1}.[{2}] IS NOT NULL) THEN {1}.[{2}] ELSE [{0}] END", mainColumn.Name, aggrTableAlias, valueColumnName) + Environment.NewLine; }
                        else
                        { updateDef += baseIndent + GenericDatabaseUtils.Indent_L1 + string.Format("SET [{0}] = {1}.[{2}]", mainColumn.Name, aggrTableAlias, valueColumnName) + Environment.NewLine; }
                        updateDef += baseIndent + GenericDatabaseUtils.Indent_L1 + string.Format("FROM [dbo].[{0}] {1}", mainTable.Name, srcTableAlias) + Environment.NewLine;
                        updateDef += baseIndent + GenericDatabaseUtils.Indent_L2 + string.Format("INNER JOIN (") + Environment.NewLine;
                        updateDef += innerQueryDef + Environment.NewLine;
                        updateDef += baseIndent + GenericDatabaseUtils.Indent_L2 + string.Format(") {0}", aggrTableAlias) + Environment.NewLine;
                        updateDef += baseIndent + GenericDatabaseUtils.Indent_L3 + string.Format("ON {0}", joinIdsText);

                        if (!hasRowNumber)
                        { updateDef += ";" + Environment.NewLine; }
                        else
                        { updateDef += Environment.NewLine + baseIndent + GenericDatabaseUtils.Indent_L1 + string.Format("WHERE {0}.[{1}] BETWEEN 1 AND 1;", aggrTableAlias, rowNumberColumnName) + Environment.NewLine; }

                        return updateDef;
                    };

                    var mainTD1 = mainTimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType;
                    var mainTD2 = mainTimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType;

                    computeSubProc.AddComputeMethod(mainVariableTemplateRef, mainTD1, mainTD2, mainColumn, computeMethod);
                }
            }

            var script = GenericDb.ExportToScript(dbType).UpdateDeciaPrefix();
            script += PrereqData.Decia_Metadata.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_TimeDimensionSetting.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_TimePeriodType.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_DataType.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_ObjectType.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_TimePeriod.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_StructuralType.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_VariableTemplate.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_VariableTemplateDependency.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_VariableTemplateGroup.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_VariableTemplateGroupMember.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_ResultSet.GetAllInserts_AsSqlText().UpdateDeciaPrefix();
            script += PrereqData.Decia_ResultSetProcessingMember.GetAllInserts_AsSqlText().UpdateDeciaPrefix();

            var tableMappings = new Dictionary<GenericTable, DataTable>();
            var columnMappings = new Dictionary<GenericColumn, DataColumn>();
            var optionalDataSet = GenericDb.GenerateDataSet(out tableMappings, out columnMappings);
            var dataTableMappings = tableMappings.ToDictionary(x => x.Value, x => x.Key);

            if (!HasDataToExport)
            {
                var globalTable = ExportState.GetRootStructuralTable(DataState.GlobalTypeRef);
                var globalDataTable = tableMappings[globalTable];
                var globalIdColumn = globalTable.Column_ForId;
                var globalNameColumn = globalTable.Column_ForName;

                var dataRow = globalDataTable.NewRow();
                dataRow[globalIdColumn.Name] = ModelObjectReference.GlobalInstanceGuid;
                dataRow[globalNameColumn.Name] = GlobalInstance.DefaultGlobalInstanceName;
                globalDataTable.Rows.Add(dataRow);

                script += globalDataTable.GetAllInserts_AsSqlText(globalTable);
                script += GenericDb.ExportCleanup(dbType).UpdateDeciaPrefix() + Environment.NewLine;

                return script;
            }

            var modelInstanceRef = ModelInstanceId.ModelObjectRef;

            if (true)
            {
                var globalTypeRef = DataState.GlobalTypeRef;
                var globalType = DataState.GlobalType;
                var inputTemplates = DataState.GetVariableTemplates(globalTypeRef, null).Values.Where(x => x.VariableType == VariableType.Input).ToList();
                var templatesByTable = GetVariableTemplatesGroupedByTable(inputTemplates);

                foreach (var globalInstanceId in DataState.GetStructuralInstanceIds(modelInstanceRef, globalTypeRef))
                {
                    var globalInstance = DataState.GlobalInstancesByRef[globalInstanceId.ModelObjectRef];
                    ExportInstanceData(tableMappings, columnMappings, templatesByTable, globalInstance);
                }
            }
            foreach (var entityTypeRef in orderedEntityTypeRefs)
            {
                var entityType = DataState.EntityTypesByRef[entityTypeRef];
                var inputTemplates = DataState.GetVariableTemplates(entityTypeRef, null).Values.Where(x => x.VariableType == VariableType.Input).ToList();
                var templatesByTable = GetVariableTemplatesGroupedByTable(inputTemplates);

                foreach (var entityInstanceId in DataState.GetStructuralInstanceIds(modelInstanceRef, entityTypeRef))
                {
                    var entityInstance = DataState.EntityInstancesByRef[entityInstanceId.ModelObjectRef];
                    ExportInstanceData(tableMappings, columnMappings, templatesByTable, entityInstance);
                }
            }
            foreach (var relationType in orderedRelationTypes)
            {
                var relationTypeRef = relationType.ModelObjectRef;
                var inputTemplates = DataState.GetVariableTemplates(relationTypeRef, null).Values.Where(x => x.VariableType == VariableType.Input).ToList();
                var templatesByTable = GetVariableTemplatesGroupedByTable(inputTemplates);

                foreach (var relationInstanceId in DataState.GetStructuralInstanceIds(modelInstanceRef, relationTypeRef))
                {
                    var relationInstance = DataState.RelationInstancesByRef[relationInstanceId.ModelObjectRef];
                    ExportInstanceData(tableMappings, columnMappings, templatesByTable, relationInstance);
                }
            }

            foreach (DataTable dataTable in optionalDataSet.Tables)
            {
                var table = dataTableMappings[dataTable];
                script += dataTable.GetAllInserts_AsSqlText(table);
            }
            script += GenericDb.ExportCleanup(dbType).UpdateDeciaPrefix() + Environment.NewLine;

            return script;
        }

        public string ExportInsertStatements(SqlDb_TargetType dbType)
        {
            var resultsStmtDefs = string.Empty;
            var resultTables = GenericDb.Tables.Where(x => x.HasSubTable_ResultTrigger).ToList();

            foreach (var resultTable in resultTables)
            {
                var insertStmt = new InsertStatementGenerator();
                insertStmt.TableName_ForInsert = resultTable.Name;

                var variableTemplate_Column = resultTable.Columns.Where(x => (!resultTable.PrimaryKey.Columns.Contains(x)) && (x.VariableTemplateRef.HasValue)).First();
                var keyColumns = resultTable.PrimaryKey.Columns;
                var resultStructuralDim_Column = keyColumns.Where(x => x.StructuralTypeRef.HasValue).First();

                var variableTemplateRef = variableTemplate_Column.VariableTemplateRef.Value;
                var variableTemplate = DataState.VariableTemplatesByRef[variableTemplateRef];
                var timeDimensionality = DataProvider.GetValidatedTimeDimensions(variableTemplateRef);

                var structuralType = DataState.GetStructuralType(variableTemplate.Containing_StructuralTypeId);
                var structuralTypeRef = structuralType.ModelObjectRef;
                var primaryTimePeriodType = timeDimensionality.PrimaryTimeDimension.NullableTimePeriodType;
                var secondaryTimePeriodType = timeDimensionality.SecondaryTimeDimension.NullableTimePeriodType;

                var rootStructuralTable = ExportState.GetRootStructuralTable(structuralTypeRef);
                var rootStructuralDim_Column = rootStructuralTable.PrimaryKey.Columns.Where(x => x.VariableTemplateRef.HasValue).First();

                if (structuralTypeRef.ModelObjectType == ModelObjectType.GlobalType)
                {
                    var structuralHelper = new SqlInsert_TableHelper()
                    {
                        TableName = rootStructuralTable.Name,
                        TableAlias = "glbl",
                        IdColumnName = rootStructuralDim_Column.Name,
                        ColumnName_ForInsert = resultStructuralDim_Column.Name
                    };
                    insertStmt.SelectTables.Add(structuralHelper);
                }
                else if (structuralTypeRef.ModelObjectType == ModelObjectType.EntityType)
                {
                    var structuralHelper = new SqlInsert_TableHelper()
                    {
                        TableName = DeciaBaseUtils.Included_StructuralInstance_TableName,
                        TableAlias = "ent_" + rootStructuralTable.Name,
                        IdColumnName = DeciaBaseUtils.Included_StructuralInstance_Id_ColumnName,
                        ColumnName_ForInsert = resultStructuralDim_Column.Name
                    };
                    insertStmt.SelectTables.Add(structuralHelper);

                    structuralHelper.EqualsClauses.Add(new SqlEquals_ClauseHelper()
                    {
                        ColumnName_ForWhere = DeciaBaseUtils.Included_StructuralInstance_TypeId_ColumnName,
                        Equals_ValueText = string.Format("'{0}'", structuralTypeRef.ModelObjectId.ToString())
                    });
                }
                else if (structuralTypeRef.ModelObjectType == ModelObjectType.RelationType)
                {
                    var structuralHelper = new SqlInsert_TableHelper()
                    {
                        TableName = rootStructuralTable.Name,
                        TableAlias = "rel_" + rootStructuralTable.Name,
                        IdColumnName = rootStructuralDim_Column.Name,
                        ColumnName_ForInsert = resultStructuralDim_Column.Name
                    };
                    insertStmt.SelectTables.Add(structuralHelper);

                    var selectFormat = "SELECT {0} FROM {1} WHERE ({2} = '{3}')";
                    foreach (var structuralColumn in rootStructuralTable.MatrixTrigger.ConstrainedColumns)
                    {
                        var relatedVariableTemplateRef = structuralColumn.VariableTemplateRef.Value;
                        var relatedVariableTemplate = DataState.VariableTemplatesByRef[relatedVariableTemplateRef];
                        var relatedEntityTypeRef = relatedVariableTemplate.RelatedEntityTypeRef;

                        structuralHelper.ExistsClauses.Add(new SqlExists_ClauseHelper()
                        {
                            ColumnName_ForWhere = structuralColumn.Name,
                            ExistsIn_ValueText = string.Format(selectFormat, DeciaBaseUtils.Included_StructuralInstance_Id_ColumnName, DeciaBaseUtils.Included_StructuralInstance_TableName, DeciaBaseUtils.Included_StructuralInstance_TypeId_ColumnName, relatedEntityTypeRef.Value.ModelObjectId.ToString())
                        });
                    }
                }
                else
                { throw new InvalidOperationException("Unsupported ModelObjectType encountered."); }

                if (primaryTimePeriodType.HasValue)
                {
                    var resultTimeDim1_Column = keyColumns.Where(x => x.TimeDimensionType == TimeDimensionType.Primary).First();

                    var timeD1Helper = new SqlInsert_TableHelper()
                    {
                        TableName = DeciaBaseUtils.Included_TimePeriod_TableName,
                        TableAlias = "td1",
                        IdColumnName = DeciaBaseUtils.Included_TimePeriod_Id_ColumnName,
                        ColumnName_ForInsert = resultTimeDim1_Column.Name
                    };
                    insertStmt.SelectTables.Add(timeD1Helper);

                    timeD1Helper.EqualsClauses.Add(new SqlEquals_ClauseHelper()
                    {
                        ColumnName_ForWhere = DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName,
                        Equals_ValueText = ((int)primaryTimePeriodType.Value).ToString()
                    });
                }

                if (secondaryTimePeriodType.HasValue)
                {
                    var resultTimeDim2_Column = keyColumns.Where(x => x.TimeDimensionType == TimeDimensionType.Secondary).First();

                    var timeD2Helper = new SqlInsert_TableHelper()
                    {
                        TableName = DeciaBaseUtils.Included_TimePeriod_TableName,
                        TableAlias = "td2",
                        IdColumnName = DeciaBaseUtils.Included_TimePeriod_Id_ColumnName,
                        ColumnName_ForInsert = resultTimeDim2_Column.Name
                    };
                    insertStmt.SelectTables.Add(timeD2Helper);

                    timeD2Helper.EqualsClauses.Add(new SqlEquals_ClauseHelper()
                    {
                        ColumnName_ForWhere = DeciaBaseUtils.Included_TimePeriod_TypeId_ColumnName,
                        Equals_ValueText = ((int)primaryTimePeriodType.Value).ToString()
                    });
                }

                resultsStmtDefs += Environment.NewLine;
                resultsStmtDefs += insertStmt.ExportStatement(dbType, true);
                resultsStmtDefs += Environment.NewLine;
            }
            return resultsStmtDefs;
        }

        #endregion

        #region Helper Methods for Required Tables

        private static void AddTimePeriodTypesUsed(HashSet<TimePeriodType> usedTimePeriodTypes, List<VariableTemplate> entityType_VariableTemplates)
        {
            usedTimePeriodTypes.AddRange(entityType_VariableTemplates.Select(x => x.HasSpecific_TimePeriodType_Primary ? x.Specific_TimePeriodType_Primary : TimePeriodType.Years));
            usedTimePeriodTypes.AddRange(entityType_VariableTemplates.Select(x => x.HasSpecific_TimePeriodType_Secondary ? x.Specific_TimePeriodType_Secondary : TimePeriodType.Years));
        }

        private static void FillWithVariableTemplates(IFormulaDataProvider dataProvider, Sql_ExportState exportState, ModelObjectReference structuralTypeRef, IEnumerable<VariableTemplate> relevantVariableTemplates)
        {
            foreach (var variableTemplate in relevantVariableTemplates)
            {
                var variableTemplateRef = variableTemplate.ModelObjectRef;
                var variableTemplate_DataType = dataProvider.GetValidatedDataType(variableTemplateRef).Value;
                var variableTemplate_TimeDimensionality = dataProvider.GetValidatedTimeDimensions(variableTemplateRef);
                var relatedEntityTypeRef = variableTemplate.RelatedEntityTypeRef;

                var variableTemplateRow = exportState.PrereqData.Decia_VariableTemplate.NewDecia_VariableTemplateRow();
                variableTemplateRow.Id = variableTemplate.ModelObjectId;
                variableTemplateRow.Name = variableTemplate.Name;
                variableTemplateRow.SqlName = variableTemplate.Name.ToEscaped_VarName();
                variableTemplateRow.StructuralTypeId = variableTemplate.Containing_StructuralTypeId.ModelObjectId;
                variableTemplateRow.SetRelated_StructuralTypeIdNull();
                variableTemplateRow.SetRelated_StructuralDimensionNumberNull();
                variableTemplateRow.IsComputed = variableTemplate.VariableType.IsComputed();
                variableTemplateRow.TimeDimensionCount = variableTemplate_TimeDimensionality.UsedDimensionCount;
                variableTemplateRow.SetPrimaryTimePeriodTypeIdNull();
                variableTemplateRow.SetSecondaryTimePeriodTypeIdNull();
                variableTemplateRow.DataTypeId = (int)variableTemplate_DataType;
                variableTemplateRow.Instance_Column_Name = variableTemplateRow.SqlName;
                variableTemplateRow.SetInstance_Column_DefaultValueNull();

                if (relatedEntityTypeRef.HasValue)
                {
                    variableTemplateRow.Related_StructuralTypeId = relatedEntityTypeRef.Value.ModelObjectId;
                    variableTemplateRow.Related_StructuralDimensionNumber = relatedEntityTypeRef.Value.NonNullAlternateDimensionNumber;
                }

                if (variableTemplate_TimeDimensionality.PrimaryTimeDimension.HasTimeValue)
                {
                    if (!variableTemplate_TimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType.HasValue)
                    { throw new InvalidOperationException("Primary Time Period Type is not set."); }
                    variableTemplateRow.PrimaryTimePeriodTypeId = (int)variableTemplate_TimeDimensionality.PrimaryTimeDimension.TimePeriodType;
                }
                if (variableTemplate_TimeDimensionality.SecondaryTimeDimension.HasTimeValue)
                {
                    if (!variableTemplate_TimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType.HasValue)
                    { throw new InvalidOperationException("Secondary Time Period Type is not set."); }
                    variableTemplateRow.SecondaryTimePeriodTypeId = (int)variableTemplate_TimeDimensionality.SecondaryTimeDimension.TimePeriodType;
                }
                if (variableTemplate.DefaultValue != ((object)null))
                {
                    if (!variableTemplate.DefaultValue.IsNull && variableTemplate.DefaultValue.IsValid)
                    { variableTemplateRow.Instance_Column_DefaultValue = variableTemplate.DefaultValue.GetValue().ToString(); }
                }

                exportState.AddVariableTemplateRow(variableTemplateRef, variableTemplateRow);
            }
        }

        private static void CreateTablesAndColumnsForVariableTemplates(IFormulaDataProvider dataProvider, Sql_ExportState exportState, GenericTable rootTable, GenericColumn rootIdColumn, ICollection<VariableTemplate> remainingVariableTemplatesToAdd)
        {
            var tables = new Dictionary<string, GenericTable>();

            var rootTableKey = Sql_ExportUtils.GenerateExportKey(null, null, false);
            var structuralTypeRef = rootTable.StructuralTypeRef.Value;
            tables.Add(rootTableKey, rootTable);

            var tableNameBase = structuralTypeRef.HexBasedName;
            rootTable.Alias = tableNameBase;

            foreach (var variableTemplate in remainingVariableTemplatesToAdd.GetOrderedList())
            {
                var variableTemplateRef = variableTemplate.ModelObjectRef;
                var variableTemplate_DataType = dataProvider.GetValidatedDataType(variableTemplateRef).Value;
                var variableTemplate_TimeDimensionality = dataProvider.GetValidatedTimeDimensions(variableTemplateRef);

                var variableTemplateRow = exportState.GetVariableTemplateRow(variableTemplateRef);
                var timePeriodType_Primary = variableTemplate_TimeDimensionality.PrimaryTimeDimension.NullableTimePeriodType;
                var timePeriodType_Secondary = variableTemplate_TimeDimensionality.SecondaryTimeDimension.NullableTimePeriodType;
                var isComputed = variableTemplate.VariableType.IsComputed();
                var tableKey = Sql_ExportUtils.GenerateExportKey(timePeriodType_Primary, timePeriodType_Secondary, isComputed);
                var table = (GenericTable)null;

                if (tables.ContainsKey(tableKey))
                { table = tables[tableKey]; }
                else
                {
                    var primaryKeyColumnIds = new List<Guid>();
                    table = exportState.GenericDb.CreateTable((rootTable.Name + tableKey), !isComputed);
                    table.SetStructuralTypeRef(structuralTypeRef);
                    if (timePeriodType_Primary.HasValue)
                    { table.SetTD1_TimePeriodType(timePeriodType_Primary.Value); }
                    if (timePeriodType_Secondary.HasValue)
                    { table.SetTD2_TimePeriodType(timePeriodType_Secondary.Value); }
                    table.Alias = tableNameBase;
                    tables.Add(tableKey, table);
                    exportState.AddNestedStructuralTable(structuralTypeRef, tableKey, table);

                    var nestedStructuralIdColumnName = (rootTable.Name + "_" + rootIdColumn.Name);
                    var nestedStructuralIdColumn = table.CreateColumn(nestedStructuralIdColumnName, DeciaDataType.UniqueID, false, false);
                    nestedStructuralIdColumn.SetVariableTemplateRef(rootTable.Column_ForId.VariableTemplateRef.Value);
                    table.AddDynamicForeignKey(rootTable.Id, nestedStructuralIdColumn.Id, rootIdColumn.Id);
                    table.SetSubTable_StructureTrigger(nestedStructuralIdColumn.Id);
                    primaryKeyColumnIds.Add(nestedStructuralIdColumn.Id);

                    if (timePeriodType_Primary.HasValue)
                    {
                        var timeDimensionType = TimeDimensionType.Primary;
                        var timeDimensionNumber = TimeDimensionTypeUtils.GetTimeDimensionNumberForType(timeDimensionType);
                        table.Alias += string.Format("_td{0}{1}", timeDimensionNumber, timePeriodType_Primary.Value.ToString().ToLower()[0]);

                        var timeDimensionIdName = string.Format(GenericDatabaseUtils.Exported_TimePeriod_Id_ColumnNameFormat, timeDimensionNumber);
                        var nestedTD1IdColumn = table.CreateColumn(timeDimensionIdName, DeciaDataType.UniqueID, false, false);
                        nestedTD1IdColumn.SetTimeDimensionType(timeDimensionType);
                        table.AddStaticForeignKey(Foreign_StaticKey_Constraint.TimePeriod_TableData, nestedTD1IdColumn.Id, Foreign_StaticKey_Constraint.TimePeriod_ColumnData_Id, timeDimensionNumber);
                        table.SetSubTable_TimeD1Trigger(nestedTD1IdColumn.Id, isComputed);
                        primaryKeyColumnIds.Add(nestedTD1IdColumn.Id);
                    }
                    if (timePeriodType_Secondary.HasValue)
                    {
                        var timeDimensionType = TimeDimensionType.Secondary;
                        var timeDimensionNumber = TimeDimensionTypeUtils.GetTimeDimensionNumberForType(timeDimensionType);
                        table.Alias += string.Format("_td{0}{1}", timeDimensionNumber, timePeriodType_Secondary.Value.ToString().ToLower()[0]);

                        var timeDimensionIdName = string.Format(GenericDatabaseUtils.Exported_TimePeriod_Id_ColumnNameFormat, timeDimensionNumber);
                        var nestedTD2IdColumn = table.CreateColumn(timeDimensionIdName, DeciaDataType.UniqueID, false, false);
                        nestedTD2IdColumn.SetTimeDimensionType(timeDimensionType);
                        table.AddStaticForeignKey(Foreign_StaticKey_Constraint.TimePeriod_TableData, nestedTD2IdColumn.Id, Foreign_StaticKey_Constraint.TimePeriod_ColumnData_Id, timeDimensionNumber);
                        table.SetSubTable_TimeD2Trigger(nestedTD2IdColumn.Id, isComputed);
                        primaryKeyColumnIds.Add(nestedTD2IdColumn.Id);
                    }
                    if (isComputed)
                    {
                        table.Alias += "_rs";

                        var resultSetIdName = GenericDatabaseUtils.Exported_ResultSet_Id_ColumnName;
                        var nestedResultIdColumn = table.CreateColumn(resultSetIdName, DeciaDataType.UniqueID, false, false);
                        table.AddStaticForeignKey(Foreign_StaticKey_Constraint.ResultSet_TableData, nestedResultIdColumn.Id, Foreign_StaticKey_Constraint.ResultSet_ColumnData_Id);
                        table.SetSubTable_ResultTrigger(nestedResultIdColumn.Id);
                        primaryKeyColumnIds.Add(nestedResultIdColumn.Id);
                    }

                    table.SetPrimaryKey(primaryKeyColumnIds);
                }

                var column = table.CreateColumn(variableTemplateRow.SqlName, variableTemplate_DataType, true, false);
                column.SetVariableTemplateRef(variableTemplateRef);
                column.DefaultValue = (!variableTemplate.DefaultValue.IsNull) ? variableTemplate.DefaultValue.GetValue().ToString() : null;

                if (variableTemplate.IsNavigationVariable)
                {
                    var foreignTable = exportState.GetRootStructuralTable(variableTemplate.RelatedEntityTypeRef.Value);
                    table.AddDynamicForeignKey(foreignTable.Id, column.Id, foreignTable.ColumnId_ForId.Value);
                }

                exportState.AddVariableColumn(variableTemplateRef, column);
            }
        }

        private static void ResetChangeCount(SqlConnection connection)
        {
            using (connection)
            {
                connection.Open();

                var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = string.Format(ChangeCount_ResetCommand);
                dropCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        private Dictionary<GenericTable, List<VariableTemplate>> GetVariableTemplatesGroupedByTable(List<VariableTemplate> variableTemplates)
        {
            var templatesByTable = new Dictionary<GenericTable, List<VariableTemplate>>();
            foreach (var variableTemplate in variableTemplates)
            {
                var variableTemplateRef = variableTemplate.ModelObjectRef;
                var table = ExportState.GetVariableColumn(variableTemplateRef).ParentTable;

                if (!templatesByTable.ContainsKey(table))
                { templatesByTable.Add(table, new List<VariableTemplate>()); }

                templatesByTable[table].Add(variableTemplate);
            }
            return templatesByTable;
        }

        private static string GenerateMetadataJoinText(string baseIndent, SqlFormulaInfo formulaInfo)
        {
            var handledStructuralType_TableAliases = new HashSet<string>();
            var handledVariableTemplate_TableAliases = new HashSet<string>();
            var result = string.Empty;

            if (formulaInfo.Metadata_IsUsed)
            {
                var tableName = DeciaBaseUtils.DeciaBase_Schema.Decia_Metadata.TableName;
                var tableAlias = formulaInfo.Metadata_TableAlias;

                result += baseIndent;
                result += string.Format("INNER JOIN [dbo].[{0}] {1} ON (1 = 1)", tableName, tableAlias);
                result += Environment.NewLine;
            }
            foreach (var argInfo in formulaInfo.ArgumentInfos.Values)
            {
                if ((argInfo.StructuralType_IsUsed || argInfo.ObjectType_IsUsed)
                    && !handledStructuralType_TableAliases.Contains(argInfo.StructuralType_TableAlias))
                {
                    var stTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_StructuralType.TableName;
                    var stTableAlias = argInfo.StructuralType_TableAlias;
                    var stRowId = argInfo.StructuralRef.ModelObjectId;

                    result += baseIndent;
                    result += string.Format("INNER JOIN [dbo].[{0}] {1} ON ('{2}' = {1}.[Id])", stTableName, stTableAlias, stRowId);
                    result += Environment.NewLine;

                    var otTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_ObjectType.TableName;
                    var otTableAlias = argInfo.ObjectType_TableAlias;

                    result += baseIndent;
                    result += string.Format("INNER JOIN [dbo].[{0}] {1} ON ({2}.[ObjectTypeId] = {1}.[Id])", otTableName, otTableAlias, stTableAlias);
                    result += Environment.NewLine;

                    handledStructuralType_TableAliases.Add(stTableAlias);
                }
                if ((argInfo.VariableTemplate_IsUsed || argInfo.DataType_IsUsed)
                    && !handledVariableTemplate_TableAliases.Contains(argInfo.VariableTemplate_TableAlias))
                {
                    var vtTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_VariableTemplate.TableName;
                    var vtTableAlias = argInfo.VariableTemplate_TableAlias;
                    var vtRowId = argInfo.VariableRef.ModelObjectId;

                    result += baseIndent;
                    result += string.Format("INNER JOIN [dbo].[{0}] {1} ON ('{2}' = {1}.[Id])", vtTableName, vtTableAlias, vtRowId);
                    result += Environment.NewLine;

                    var dtTableName = DeciaBaseUtils.DeciaBase_Schema.Decia_DataType.TableName;
                    var dtTableAlias = argInfo.DataType_TableAlias;

                    result += baseIndent;
                    result += string.Format("INNER JOIN [dbo].[{0}] {1} ON ({2}.[DataTypeId] = {1}.[Id])", dtTableName, dtTableAlias, vtTableAlias);
                    result += Environment.NewLine;

                    handledVariableTemplate_TableAliases.Add(vtTableAlias);
                }
            }

            return result;
        }

        private void ExportInstanceData(Dictionary<GenericTable, DataTable> tableMappings, Dictionary<GenericColumn, DataColumn> columnMappings, Dictionary<GenericTable, List<VariableTemplate>> inputTemplatesByTable, IStructuralMember_Orderable structuralInstance)
        {
            var modelInstanceRef = structuralInstance.GetModelMemberId().ModelObjectRef;
            var structuralInstanceRef = structuralInstance.ModelObjectRef;

            foreach (var table in inputTemplatesByTable.Keys)
            {
                var inputTemplates = inputTemplatesByTable[table];
                var dataTable = tableMappings[table];

                var structuralIdColumn = table.PrimaryKey.Columns.Where(x => !x.TimeDimensionType.HasValue).First();
                var timeIdColumn_D1 = table.PrimaryKey.Columns.Where(x => x.TimeDimensionType == TimeDimensionType.Primary).FirstOrDefault();
                var timeIdColumn_D2 = table.PrimaryKey.Columns.Where(x => x.TimeDimensionType == TimeDimensionType.Secondary).FirstOrDefault();

                var timePeriods_D1 = table.TD1_TimePeriodType.HasValue ? ExportState.GetTimePeriodsByType(table.TD1_TimePeriodType.Value) : new TimePeriod[] { TimePeriod.ForeverPeriod };
                var timePeriods_D2 = table.TD2_TimePeriodType.HasValue ? ExportState.GetTimePeriodsByType(table.TD2_TimePeriodType.Value) : new TimePeriod[] { TimePeriod.ForeverPeriod };

                foreach (var timePeriod_D1 in timePeriods_D1)
                {
                    foreach (var timePeriod_D2 in timePeriods_D2)
                    {
                        var dataRow = dataTable.NewRow();
                        dataRow[structuralIdColumn.Name] = structuralInstance.ModelObjectId;
                        if (timeIdColumn_D1 != null)
                        { dataRow[timeIdColumn_D1.Name] = timePeriod_D1.Id; }
                        if (timeIdColumn_D2 != null)
                        { dataRow[timeIdColumn_D2.Name] = timePeriod_D2.Id; }
                        dataTable.Rows.Add(dataRow);

                        foreach (var inputTemplate in inputTemplates)
                        {
                            var inputTemplateRef = inputTemplate.ModelObjectRef;
                            var predefinedValueType = inputTemplate.PredefinedVariableTemplateOption;
                            var column = ExportState.GetVariableColumn(inputTemplateRef);

                            if (column.UsesSqlFormula)
                            { continue; }

                            var dataColumn = columnMappings[column];
                            var variableInstance = DataState.GetVariableInstance(modelInstanceRef, structuralInstanceRef, inputTemplateRef);
                            object value = null;

                            if (variableInstance != null)
                            {
                                var timeMatrix = DataProvider.GetAssessedTimeMatrix(modelInstanceRef, variableInstance.ModelObjectRef);
                                var dynamicValue = timeMatrix.GetValue(timePeriod_D1, timePeriod_D2);
                                value = dynamicValue.GetValue();
                            }
                            if (predefinedValueType.HasValue)
                            {
                                if (predefinedValueType == PredefinedVariableTemplateOption.Id)
                                { value = structuralInstance.StructuralInstanceGuid; }
                                if (predefinedValueType == PredefinedVariableTemplateOption.Name)
                                { value = structuralInstance.GetStructuralMember_Name(); }
                                if (predefinedValueType == PredefinedVariableTemplateOption.Order)
                                { value = structuralInstance.GetStructuralMember_OrderNumber(); }
                                if (predefinedValueType == PredefinedVariableTemplateOption.Id_Parent)
                                { value = (structuralInstance as EntityInstance).Parent_EntityInstanceGuid; }
                                if (predefinedValueType == PredefinedVariableTemplateOption.Id_Related)
                                {
                                    var relationInstance = (RelationInstance)structuralInstance;
                                    var entityInstanceId = relationInstance.RelatedEntityInstanceGuids[inputTemplate.RelatedEntityTypeRef.Value];
                                    value = entityInstanceId;
                                }
                            }

                            dataRow[column.Name] = (value != null) ? value : DBNull.Value;
                        }
                    }
                }
            }
        }

        #endregion
    }
}