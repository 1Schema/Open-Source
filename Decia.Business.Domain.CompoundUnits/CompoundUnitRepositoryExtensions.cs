using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.DomainModeling.DataProviders;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.Repositories;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.CompoundUnits.BaseUnitValues;
using CompoundUnitIdRow = Decia.Business.Domain.CompoundUnits.Internals.CompoundUnitIdRow;

namespace Decia.Business.Domain.CompoundUnits
{
    #region Internals - Data Objects for Queries

    namespace Internals
    {
        public class CompoundUnitIdRow
        {
            public static int ConstructionCount = 0;

            public CompoundUnitIdRow()
            { ConstructionCount++; }

            public Guid EF_ProjectGuid { get; set; }
            public long EF_RevisionNumber { get; set; }
            public Guid EF_CompoundUnitGuid { get; set; }

            public bool EF_IsDeleted { get; set; }
        }
    }

    #endregion

    public static class CompoundUnitRepositoryExtensions
    {
        public static bool Prefer_StoredProcs = AdoNetUtils.Default_Prefer_StoredProcs;
        public const bool Default_AssertAllFound = true;

        #region Methods to Get Nested Data Store

        public static IQueryable<BaseUnitExponentiationValue> GetNestedDataStore(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository)
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_BaseUnitExponentiationValues).SelectMany(x => x).ToList();
                return new EnumerableQuery<BaseUnitExponentiationValue>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<BaseUnitExponentiationValue>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        #endregion

        #region Queries to Read by Desired (Project, Revision)

        public static bool HasChanges_ForRevisionRange(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            var changeCount = GetChangeCount_ForRevisionRange(repository, minRevisionNumber, maxRevisionNumber, revisionChain);
            return (changeCount > 0);
        }

        public static int GetChangeCount_ForRevisionRange(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            revisionChain.AssertRevisionRangeIsValid(minRevisionNumber, maxRevisionNumber);
            revisionChain.AssertRevisionIsAllowed(maxRevisionNumber);

            var projectGuid = revisionChain.ProjectGuid;
            var disallowedRevisionNumbers = revisionChain.DisallowedRevisions;
            var versionedObjects = repository.GetActualDataStore();

            var changeCount = (from vObj in versionedObjects
                               where vObj.EF_ProjectGuid == projectGuid
                                    && (vObj.EF_RevisionNumber >= minRevisionNumber && vObj.EF_RevisionNumber <= maxRevisionNumber)
                                    && !disallowedRevisionNumbers.Contains(vObj.EF_RevisionNumber)
                               select 1).Count();

            return changeCount;
        }

        #region Read Collection by ()

        public static CompoundUnitDataSet ReadCurrentDataSet_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, RevisionChain revisionChain)
        {
            return ReadCurrentDataSet_ForDesiredRevision(repository, null, revisionChain, false);
        }

        public static CompoundUnitDataSet ReadCurrentDataSet_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IEnumerable<Guid> compoundUnitGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            var databaseContext = (repository.CurrentDataSource as DbContext);

            var compoundUnitGuidsAsText = (string)null;
            if (compoundUnitGuids != null)
            {
                var compoundUnitObjects = compoundUnitGuids.Select(x => (object)x.ToString().ToUpper()).ToList();
                compoundUnitGuidsAsText = AdoNetUtils.CovertToKeyMatchingText(compoundUnitObjects, AdoNetUtils.KeyMatching_Separator_Text, AdoNetUtils.KeyMatching_Separator_Text, false);
            }

            using (var connection = databaseContext.GetConnection())
            {
                var parameters = new Dictionary<string, object>();
                parameters.Add(CompoundUnit.SP_ReadCurrent_Parameter0_Name, revisionChain.ProjectGuid);
                parameters.Add(CompoundUnit.SP_ReadCurrent_Parameter1_Name, revisionChain.DesiredRevisionNumber);
                parameters.Add(CompoundUnit.SP_ReadCurrent_Parameter2_Name, compoundUnitGuidsAsText);

                var dataSet_UnTyped = connection.ExecuteStoredProcedureWithResult(CompoundUnit.SP_ReadCurrent_Name, parameters);

                var dataSet_Typed = new CompoundUnitDataSet();
                dataSet_Typed.EnforceConstraints = false;
                dataSet_Typed.CompoundUnits.Merge(dataSet_UnTyped.Tables[1]);
                dataSet_Typed.BaseUnitExponentiationValues.Merge(dataSet_UnTyped.Tables[2]);
                dataSet_Typed.EnforceConstraints = true;
                dataSet_Typed.AcceptChanges();

                if (assertAllFound)
                {
                    if (compoundUnitGuids.Count() != dataSet_Typed.CompoundUnits.Count())
                    { throw new InvalidOperationException("Some Formulas could not be found."); }
                }

                return dataSet_Typed;
            };
        }

        public static IQueryable<CompoundUnitIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, RevisionChain revisionChain)
        {
            var projectGuid = revisionChain.ProjectGuid;
            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var disallowedRevisionNumbers = revisionChain.DisallowedRevisions;
            var versionedObjects = repository.GetActualDataStore();

            var possibleRows = (from vObj in versionedObjects
                                where vObj.EF_ProjectGuid == projectGuid
                                     && vObj.EF_RevisionNumber <= desiredRevisionNumber
                                     && !disallowedRevisionNumbers.Contains(vObj.EF_RevisionNumber)
                                group vObj by new { vObj.EF_ProjectGuid, vObj.EF_CompoundUnitGuid, vObj.EF_IsDeleted } into gvObj
                                select new CompoundUnitIdRow()
                                {
                                    EF_ProjectGuid = gvObj.Key.EF_ProjectGuid,
                                    EF_RevisionNumber = gvObj.Max(vObj => vObj.EF_RevisionNumber),
                                    EF_CompoundUnitGuid = gvObj.Key.EF_CompoundUnitGuid,
                                    EF_IsDeleted = gvObj.Key.EF_IsDeleted,
                                });

            var currentRows = (from vObj in possibleRows.Where(x => !x.EF_IsDeleted)
                               join vObjD in possibleRows.Where(x => x.EF_IsDeleted) on new { vObj.EF_ProjectGuid, vObj.EF_CompoundUnitGuid } equals new { vObjD.EF_ProjectGuid, vObjD.EF_CompoundUnitGuid } into gvObjD
                               from vObj_Deleted in gvObjD.DefaultIfEmpty()
                               select new CompoundUnitIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_CompoundUnitGuid = vObj.EF_CompoundUnitGuid,
                                   EF_IsDeleted = (vObj_Deleted == null) ? vObj.EF_IsDeleted : ((vObj.EF_RevisionNumber > vObj_Deleted.EF_RevisionNumber) ? vObj.EF_IsDeleted : vObj_Deleted.EF_IsDeleted),
                               }).Where(vObj => !vObj.EF_IsDeleted);

            return currentRows;
        }

        public static ICollection<CompoundUnitId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new CompoundUnitId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_CompoundUnitGuid)).ToList();
            return currentIds;
        }

        public static ICollection<CompoundUnit> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var currentObjects = DoAggregateRead(repository, currentRows);
            return currentObjects;
        }

        #endregion

        #region Read Collection by (compoundUnitGuids)

        public static IQueryable<CompoundUnitIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IEnumerable<Guid> compoundUnitGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => compoundUnitGuids.Contains(vObj.EF_CompoundUnitGuid));

            if (assertAllFound)
            {
                if (compoundUnitGuids.Count() != currentRows.Count())
                { throw new InvalidOperationException("Some CompoundUnits could not be found."); }
            }

            return currentRows;
        }

        public static ICollection<CompoundUnitId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IEnumerable<Guid> compoundUnitGuids, RevisionChain revisionChain)
        {
            return ReadCurrentKeys_ForDesiredRevision(repository, compoundUnitGuids, revisionChain, Default_AssertAllFound);
        }

        public static ICollection<CompoundUnitId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IEnumerable<Guid> compoundUnitGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, compoundUnitGuids, revisionChain, assertAllFound);
                var currentIds = currentRows.ToList().Select(x => new CompoundUnitId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_CompoundUnitGuid)).ToHashSet();
                return currentIds;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, compoundUnitGuids, revisionChain, assertAllFound);
                var currentKeys = currentDataSet.CompoundUnits.Select(x => new CompoundUnitId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_CompoundUnitGuid)).ToList();
                return currentKeys;
            }
        }

        public static ICollection<CompoundUnit> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IEnumerable<Guid> compoundUnitGuids, RevisionChain revisionChain)
        {
            return ReadCurrentObjects_ForDesiredRevision(repository, compoundUnitGuids, revisionChain, Default_AssertAllFound);
        }

        public static ICollection<CompoundUnit> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IEnumerable<Guid> compoundUnitGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, compoundUnitGuids, revisionChain, assertAllFound);
                var currentObjects = DoAggregateRead(repository, currentRows);
                return currentObjects;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, compoundUnitGuids, revisionChain, assertAllFound);
                var currentObjects = DoAggregateRead(currentDataSet, revisionChain);
                return currentObjects;
            }
        }

        #endregion

        #region Read Object by (compoundUnitGuid)

        public static CompoundUnitId ReadCurrentKey_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, Guid compoundUnitGuid, RevisionChain revisionChain)
        {
            var compoundUnitGuids = new Guid[] { compoundUnitGuid };
            var currentIds = ReadCurrentKeys_ForDesiredRevision(repository, compoundUnitGuids, revisionChain);
            return currentIds.GetAndAssertUniqueValue(false).Value;
        }

        public static CompoundUnit ReadCurrentObject_ForDesiredRevision(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, Guid compoundUnitGuid, RevisionChain revisionChain)
        {
            var compoundUnitGuids = new Guid[] { compoundUnitGuid };
            var currentObjects = ReadCurrentObjects_ForDesiredRevision(repository, compoundUnitGuids, revisionChain);
            return currentObjects.FirstOrDefault();
        }

        #endregion

        #endregion

        #region Queries to Read for Specific Revision ONLY

        public static ICollection<CompoundUnit> ReadForSpecificRevisionId(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, RevisionId revisionId)
        {
            LinqQuery<CompoundUnit> query = new LinqQuery<CompoundUnit>((CompoundUnit dObj) => ((dObj.EF_ProjectGuid == revisionId.ProjectGuid) && (dObj.EF_RevisionNumber == revisionId.RevisionNumber_NonNull)));
            ICollection<CompoundUnit> result = repository.Read(query);
            return result;
        }

        public static IDictionary<CompoundUnitId, CompoundUnit> ReadDictionaryForSpecificRevisionId(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, RevisionId revisionId)
        {
            ICollection<CompoundUnit> list = ReadForSpecificRevisionId(repository, revisionId);
            IDictionary<CompoundUnitId, CompoundUnit> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Creator

        public static ICollection<CompoundUnit> ReadForCreatorId(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, UserId creatorId)
        {
            LinqQuery<CompoundUnit> query = new LinqQuery<CompoundUnit>((CompoundUnit dObj) => (dObj.EF_CreatorGuid == creatorId.UserGuid));
            ICollection<CompoundUnit> result = repository.Read(query);
            return result;
        }

        public static IDictionary<CompoundUnitId, CompoundUnit> ReadDictionaryForCreatorId(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, UserId creatorId)
        {
            ICollection<CompoundUnit> list = ReadForCreatorId(repository, creatorId);
            IDictionary<CompoundUnitId, CompoundUnit> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Owner

        public static ICollection<CompoundUnit> ReadForOwnerId(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, ISiteActor siteActorRef)
        {
            LinqQuery<CompoundUnit> query = new LinqQuery<CompoundUnit>((CompoundUnit dObj) => ((dObj.EF_OwnerType == (int)siteActorRef.ActorType) && (dObj.EF_OwnerGuid == siteActorRef.ActorGuid)));
            ICollection<CompoundUnit> result = repository.Read(query);
            return result;
        }

        public static IDictionary<CompoundUnitId, CompoundUnit> ReadDictionaryForOwnerId(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, ISiteActor siteActorRef)
        {
            ICollection<CompoundUnit> list = ReadForOwnerId(repository, siteActorRef);
            IDictionary<CompoundUnitId, CompoundUnit> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Helper Methods for Reading Entire Aggregate

        private static List<CompoundUnit> DoAggregateRead(this IReadOnlyRepository<CompoundUnitId, CompoundUnit> repository, IQueryable<CompoundUnitIdRow> currentRows)
        {
            var versionedObjects_Main = repository.GetActualDataStore();
            var versionedObjects_Nested = repository.GetNestedDataStore();

            var currentObjects_Main = (from cRow in currentRows
                                       join vObj in versionedObjects_Main on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_CompoundUnitGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_CompoundUnitGuid }
                                       select vObj).ToList();
            var currentObjects_Nested = (from cRow in currentRows
                                         join vObj in versionedObjects_Nested on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_CompoundUnitGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_CompoundUnitGuid }
                                         select vObj).ToList();

            var currentObjects_Nested_Dict = new Dictionary<CompoundUnitId, List<BaseUnitExponentiationValue>>();
            foreach (var currentObject_Nested_TimeDimension in currentObjects_Nested)
            {
                var key = new CompoundUnitId(currentObject_Nested_TimeDimension.EF_ProjectGuid, currentObject_Nested_TimeDimension.EF_RevisionNumber, currentObject_Nested_TimeDimension.EF_CompoundUnitGuid);
                if (!currentObjects_Nested_Dict.ContainsKey(key))
                { currentObjects_Nested_Dict.Add(key, new List<BaseUnitExponentiationValue>()); }
                currentObjects_Nested_Dict[key].Add(currentObject_Nested_TimeDimension);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(CompoundUnit.TypeName_CompoundUnit, currentObjects_Main);
            batchReadState.Add(CompoundUnit.TypeName_BaseUnitExponentiationValue, currentObjects_Nested_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<CompoundUnit>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        public static List<CompoundUnit> DoAggregateRead(this DataSet dataSet, RevisionChain revisionChain)
        {
            var compoundUnitsDataTable = dataSet.Tables[CompoundUnit.DataSetSchema.CompoundUnits.TableName];
            var exponentiationValuesDataTable = dataSet.Tables[CompoundUnit.DataSetSchema.BaseUnitExponentiationValues.TableName];
            return DoAggregateRead(compoundUnitsDataTable, exponentiationValuesDataTable, revisionChain);
        }

        public static List<CompoundUnit> DoAggregateRead(this DataTable compoundUnitsDataTable, DataTable exponentiationValuesDataTable, RevisionChain revisionChain)
        {
            var currentObjects_Main = new List<CompoundUnit>();
            var currentObjects_Nested_ExponentiationValue = new List<BaseUnitExponentiationValue>();

            foreach (var dataRow in compoundUnitsDataTable.Select())
            {
                var value = new CompoundUnit();
                value.CopyDataRowToObject<CompoundUnit>(dataRow);
                currentObjects_Main.Add(value);
            }

            foreach (var dataRow in exponentiationValuesDataTable.Select())
            {
                var value = new BaseUnitExponentiationValue();
                value.CopyDataRowToObject<BaseUnitExponentiationValue>(dataRow);
                currentObjects_Nested_ExponentiationValue.Add(value);
            }

            var currentObjects_Nested_ExponentiationValue_Dict = new Dictionary<CompoundUnitId, List<BaseUnitExponentiationValue>>();
            foreach (var currentObject_Nested_ExponentiationValue in currentObjects_Nested_ExponentiationValue)
            {
                var key = new CompoundUnitId(currentObject_Nested_ExponentiationValue.EF_ProjectGuid, currentObject_Nested_ExponentiationValue.EF_RevisionNumber, currentObject_Nested_ExponentiationValue.EF_CompoundUnitGuid);
                if (!currentObjects_Nested_ExponentiationValue_Dict.ContainsKey(key))
                { currentObjects_Nested_ExponentiationValue_Dict.Add(key, new List<BaseUnitExponentiationValue>()); }
                currentObjects_Nested_ExponentiationValue_Dict[key].Add(currentObject_Nested_ExponentiationValue);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(CompoundUnit.TypeName_CompoundUnit, currentObjects_Main);
            batchReadState.Add(CompoundUnit.TypeName_BaseUnitExponentiationValue, currentObjects_Nested_ExponentiationValue_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<CompoundUnit>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        #endregion
    }
}