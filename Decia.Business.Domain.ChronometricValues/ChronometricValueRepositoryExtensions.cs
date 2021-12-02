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
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.ChronometricValues.TimeAssessments;
using Decia.Business.Domain.ChronometricValues.TimeDimensions;
using ChronometricValueIdRow = Decia.Business.Domain.ChronometricValues.Internals.ChronometricValueIdRow;

namespace Decia.Business.Domain.ChronometricValues
{
    #region Internals - Data Objects for Queries

    namespace Internals
    {
        public class ChronometricValueIdRow
        {
            public static int ConstructionCount = 0;

            public ChronometricValueIdRow()
            { ConstructionCount++; }

            public Guid EF_ProjectGuid { get; set; }
            public long EF_RevisionNumber { get; set; }
            public Guid EF_ChronometricValueGuid { get; set; }

            public bool EF_IsDeleted { get; set; }
        }
    }

    #endregion

    public static class ChronometricValueRepositoryExtensions
    {
        public static bool Prefer_StoredProcs = AdoNetUtils.Default_Prefer_StoredProcs;
        public const bool Default_AssertAllFound = true;

        #region Methods to Get Nested Data Store

        public static IQueryable<SaveableTimeDimension> GetNestedDataStore_TimeDimensions(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository)
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_TimeDimensions).SelectMany(x => x).ToList();
                return new EnumerableQuery<SaveableTimeDimension>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<SaveableTimeDimension>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        public static IQueryable<TimeAssessment> GetNestedDataStore_TimeAssessments(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository)
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_TimeAssessments).SelectMany(x => x).ToList();
                return new EnumerableQuery<TimeAssessment>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<TimeAssessment>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        #endregion

        #region Queries to Read by Desired (Project, Revision)

        public static bool HasChanges_ForRevisionRange(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            var changeCount = GetChangeCount_ForRevisionRange(repository, minRevisionNumber, maxRevisionNumber, revisionChain);
            return (changeCount > 0);
        }

        public static int GetChangeCount_ForRevisionRange(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
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

        public static ChronometricValueDataSet ReadCurrentDataSet_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, RevisionChain revisionChain)
        {
            return ReadCurrentDataSet_ForDesiredRevision(repository, null, revisionChain, false);
        }

        public static ChronometricValueDataSet ReadCurrentDataSet_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IEnumerable<Guid> chronometricValueGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            var databaseContext = (repository.CurrentDataSource as DbContext);

            var chronometricValuesGuidsAsText = (string)null;
            if (chronometricValueGuids != null)
            {
                var chronometricValueObjects = chronometricValueGuids.Select(x => (object)x.ToString().ToUpper()).ToList();
                chronometricValuesGuidsAsText = AdoNetUtils.CovertToKeyMatchingText(chronometricValueObjects, AdoNetUtils.KeyMatching_Separator_Text, AdoNetUtils.KeyMatching_Separator_Text, false);
            }

            using (var connection = databaseContext.GetConnection())
            {
                var parameters = new Dictionary<string, object>();
                parameters.Add(ChronometricValue.SP_ReadCurrent_Parameter0_Name, revisionChain.ProjectGuid);
                parameters.Add(ChronometricValue.SP_ReadCurrent_Parameter1_Name, revisionChain.DesiredRevisionNumber);
                parameters.Add(ChronometricValue.SP_ReadCurrent_Parameter2_Name, chronometricValuesGuidsAsText);

                var dataSet_UnTyped = connection.ExecuteStoredProcedureWithResult(ChronometricValue.SP_ReadCurrent_Name, parameters);

                var dataSet_Typed = new ChronometricValueDataSet();
                dataSet_Typed.EnforceConstraints = false;
                dataSet_Typed.ChronometricValues.Merge(dataSet_UnTyped.Tables[1]);
                dataSet_Typed.SaveableTimeDimensions.Merge(dataSet_UnTyped.Tables[2]);
                dataSet_Typed.TimeAssessments.Merge(dataSet_UnTyped.Tables[3]);
                dataSet_Typed.EnforceConstraints = true;
                dataSet_Typed.AcceptChanges();

                if (assertAllFound)
                {
                    if (chronometricValueGuids.Count() != dataSet_Typed.ChronometricValues.Count())
                    { throw new InvalidOperationException("Some ChronometricValues could not be found."); }
                }

                return dataSet_Typed;
            };
        }

        public static IQueryable<ChronometricValueIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, RevisionChain revisionChain)
        {
            var projectGuid = revisionChain.ProjectGuid;
            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var disallowedRevisionNumbers = revisionChain.DisallowedRevisions;
            var versionedObjects = repository.GetActualDataStore();

            var possibleRows = (from vObj in versionedObjects
                                where vObj.EF_ProjectGuid == projectGuid
                                     && vObj.EF_RevisionNumber <= desiredRevisionNumber
                                     && !disallowedRevisionNumbers.Contains(vObj.EF_RevisionNumber)
                                group vObj by new { vObj.EF_ProjectGuid, vObj.EF_ChronometricValueGuid, vObj.EF_IsDeleted } into gvObj
                                select new ChronometricValueIdRow()
                                {
                                    EF_ProjectGuid = gvObj.Key.EF_ProjectGuid,
                                    EF_RevisionNumber = gvObj.Max(vObj => vObj.EF_RevisionNumber),
                                    EF_ChronometricValueGuid = gvObj.Key.EF_ChronometricValueGuid,
                                    EF_IsDeleted = gvObj.Key.EF_IsDeleted,
                                });

            var currentRows = (from vObj in possibleRows.Where(x => !x.EF_IsDeleted)
                               join vObjD in possibleRows.Where(x => x.EF_IsDeleted) on new { vObj.EF_ProjectGuid, vObj.EF_ChronometricValueGuid } equals new { vObjD.EF_ProjectGuid, vObjD.EF_ChronometricValueGuid } into gvObjD
                               from vObj_Deleted in gvObjD.DefaultIfEmpty()
                               select new ChronometricValueIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_ChronometricValueGuid = vObj.EF_ChronometricValueGuid,
                                   EF_IsDeleted = (vObj_Deleted == null) ? vObj.EF_IsDeleted : ((vObj.EF_RevisionNumber > vObj_Deleted.EF_RevisionNumber) ? vObj.EF_IsDeleted : vObj_Deleted.EF_IsDeleted),
                               }).Where(vObj => !vObj.EF_IsDeleted);

            return currentRows;
        }

        public static ICollection<ChronometricValueId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, RevisionChain revisionChain)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
                var currentKeys = currentRows.ToList().Select(x => new ChronometricValueId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ChronometricValueGuid)).ToList();
                return currentKeys;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, revisionChain);
                var currentKeys = currentDataSet.ChronometricValues.Select(x => new ChronometricValueId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ChronometricValueGuid)).ToList();
                return currentKeys;
            }
        }

        public static ICollection<ChronometricValue> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, RevisionChain revisionChain)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
                var currentObjects = DoAggregateRead(repository, currentRows);
                return currentObjects;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, revisionChain);
                var currentObjects = DoAggregateRead(currentDataSet, revisionChain);
                return currentObjects;
            }
        }

        #endregion

        #region Read Collection by (chronometricValueGuids)

        public static IQueryable<ChronometricValueIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IEnumerable<Guid> chronometricValueGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => chronometricValueGuids.Contains(vObj.EF_ChronometricValueGuid));

            if (assertAllFound)
            {
                if (chronometricValueGuids.Count() != currentRows.Count())
                { throw new InvalidOperationException("Some ChronometricValues could not be found."); }
            }

            return currentRows;
        }

        public static ICollection<ChronometricValueId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IEnumerable<Guid> chronometricValueGuids, RevisionChain revisionChain)
        {
            return ReadCurrentKeys_ForDesiredRevision(repository, chronometricValueGuids, revisionChain, Default_AssertAllFound);
        }

        public static ICollection<ChronometricValueId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IEnumerable<Guid> chronometricValueGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, chronometricValueGuids, revisionChain, assertAllFound);
                var currentIds = currentRows.ToList().Select(x => new ChronometricValueId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ChronometricValueGuid)).ToHashSet();
                return currentIds;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, chronometricValueGuids, revisionChain, assertAllFound);
                var currentKeys = currentDataSet.ChronometricValues.Select(x => new ChronometricValueId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ChronometricValueGuid)).ToList();
                return currentKeys;
            }
        }

        public static ICollection<ChronometricValue> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IEnumerable<Guid> chronometricValueGuids, RevisionChain revisionChain)
        {
            return ReadCurrentObjects_ForDesiredRevision(repository, chronometricValueGuids, revisionChain, Default_AssertAllFound);
        }

        public static ICollection<ChronometricValue> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IEnumerable<Guid> chronometricValueGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, chronometricValueGuids, revisionChain, assertAllFound);
                var currentObjects = DoAggregateRead(repository, currentRows);
                return currentObjects;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, chronometricValueGuids, revisionChain, assertAllFound);
                var currentObjects = DoAggregateRead(currentDataSet, revisionChain);
                return currentObjects;
            }
        }

        #endregion

        #region Read Object by (chronometricValueGuid)

        public static ChronometricValueId ReadCurrentKey_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, Guid chronometricValueGuid, RevisionChain revisionChain)
        {
            var chronometricValueGuids = new Guid[] { chronometricValueGuid };
            var currentIds = ReadCurrentKeys_ForDesiredRevision(repository, chronometricValueGuids, revisionChain);
            return currentIds.GetAndAssertUniqueValue(false).Value;
        }

        public static ChronometricValue ReadCurrentObject_ForDesiredRevision(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, Guid chronometricValueGuid, RevisionChain revisionChain)
        {
            var chronometricValueGuids = new Guid[] { chronometricValueGuid };
            var currentObjects = ReadCurrentObjects_ForDesiredRevision(repository, chronometricValueGuids, revisionChain);
            return currentObjects.FirstOrDefault();
        }

        #endregion

        #endregion

        #region Queries to Read for Specific Revision ONLY

        public static ICollection<ChronometricValue> ReadForSpecificRevisionId(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, RevisionId revisionId)
        {
            LinqQuery<ChronometricValue> query = new LinqQuery<ChronometricValue>((ChronometricValue dObj) => ((dObj.EF_ProjectGuid == revisionId.ProjectGuid) && (dObj.EF_RevisionNumber == revisionId.RevisionNumber_NonNull)));
            ICollection<ChronometricValue> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ChronometricValueId, ChronometricValue> ReadDictionaryForSpecificRevisionId(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, RevisionId revisionId)
        {
            ICollection<ChronometricValue> list = ReadForSpecificRevisionId(repository, revisionId);
            IDictionary<ChronometricValueId, ChronometricValue> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Creator

        public static ICollection<ChronometricValue> ReadForCreatorId(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, UserId creatorId)
        {
            LinqQuery<ChronometricValue> query = new LinqQuery<ChronometricValue>((ChronometricValue dObj) => (dObj.EF_CreatorGuid == creatorId.UserGuid));
            ICollection<ChronometricValue> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ChronometricValueId, ChronometricValue> ReadDictionaryForCreatorId(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, UserId creatorId)
        {
            ICollection<ChronometricValue> list = ReadForCreatorId(repository, creatorId);
            IDictionary<ChronometricValueId, ChronometricValue> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Owner

        public static ICollection<ChronometricValue> ReadForOwnerId(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, ISiteActor siteActorRef)
        {
            LinqQuery<ChronometricValue> query = new LinqQuery<ChronometricValue>((ChronometricValue dObj) => ((dObj.EF_OwnerType == (int)siteActorRef.ActorType) && (dObj.EF_OwnerGuid == siteActorRef.ActorGuid)));
            ICollection<ChronometricValue> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ChronometricValueId, ChronometricValue> ReadDictionaryForOwnerId(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, ISiteActor siteActorRef)
        {
            ICollection<ChronometricValue> list = ReadForOwnerId(repository, siteActorRef);
            IDictionary<ChronometricValueId, ChronometricValue> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Helper Methods for Reading Entire Aggregate

        public static List<ChronometricValue> DoAggregateRead(this IReadOnlyRepository<ChronometricValueId, ChronometricValue> repository, IQueryable<ChronometricValueIdRow> currentRows)
        {
            var versionedObjects_Main = repository.GetActualDataStore();
            var versionedObjects_Nested_TimeDimension = repository.GetNestedDataStore_TimeDimensions();
            var versionedObjects_Nested_TimeAssessment = repository.GetNestedDataStore_TimeAssessments();

            var timeBinding_HasValue = ChronometricValue.TimeBindings_ForPartialRead.HasValue;
            var timeBinding_PrimaryPeriod = timeBinding_HasValue ? ChronometricValue.TimeBindings_ForPartialRead.Value.NullablePrimaryTimePeriod : (TimePeriod?)null;
            var timeBinding_SecondaryPeriod = timeBinding_HasValue ? ChronometricValue.TimeBindings_ForPartialRead.Value.NullableSecondaryTimePeriod : (TimePeriod?)null;

            var timeBinding_HasPrimaryDates = timeBinding_PrimaryPeriod.HasValue;
            var timeBinding_PrimaryStartDate = timeBinding_PrimaryPeriod.HasValue ? timeBinding_PrimaryPeriod.Value.StartDate : (DateTime?)null;
            var timeBinding_PrimaryEndDate = timeBinding_PrimaryPeriod.HasValue ? timeBinding_PrimaryPeriod.Value.EndDate : (DateTime?)null;
            var timeBinding_HasSecondaryDates = timeBinding_SecondaryPeriod.HasValue;
            var timeBinding_SecondaryStartDate = timeBinding_SecondaryPeriod.HasValue ? timeBinding_SecondaryPeriod.Value.StartDate : (DateTime?)null;
            var timeBinding_SecondaryEndDate = timeBinding_SecondaryPeriod.HasValue ? timeBinding_SecondaryPeriod.Value.EndDate : (DateTime?)null;

            var currentObjects_Main_Query = (from cRow in currentRows
                                             join vObj in versionedObjects_Main on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_ChronometricValueGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ChronometricValueGuid }
                                             select vObj);
            var currentObjects_Nested_TimeDimension_Query = (from cRow in currentRows
                                                             join vObj in versionedObjects_Nested_TimeDimension on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_ChronometricValueGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ChronometricValueGuid }
                                                             select vObj);
            var currentObjects_Nested_TimeAssessment_Query = (from cRow in currentRows
                                                              join vObj in versionedObjects_Nested_TimeAssessment on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_ChronometricValueGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ChronometricValueGuid }
                                                              where (!timeBinding_HasPrimaryDates ||
                                                                          (timeBinding_HasPrimaryDates && !vObj.EF_HasPrimaryTimeDimension) ||
                                                                          (timeBinding_HasPrimaryDates && vObj.EF_HasPrimaryTimeDimension && !((vObj.EF_PrimaryEndDate < timeBinding_PrimaryStartDate) || (vObj.EF_PrimaryStartDate > timeBinding_PrimaryEndDate)))) &&
                                                                      (!timeBinding_HasSecondaryDates ||
                                                                          (timeBinding_HasSecondaryDates && !vObj.EF_HasSecondaryTimeDimension) ||
                                                                          (timeBinding_HasSecondaryDates && vObj.EF_HasSecondaryTimeDimension && !((vObj.EF_SecondaryEndDate < timeBinding_SecondaryStartDate) || (vObj.EF_SecondaryStartDate > timeBinding_SecondaryEndDate))))
                                                              select vObj);

            var currentObjects_Main = currentObjects_Main_Query.ToList();
            var currentObjects_Nested_TimeDimension = currentObjects_Nested_TimeDimension_Query.ToList();
            var currentObjects_Nested_TimeAssessment = currentObjects_Nested_TimeAssessment_Query.ToList();
            currentObjects_Nested_TimeAssessment.ForEach(x => x.IsPartialObject = GetIsPartialRead(timeBinding_HasValue, timeBinding_HasPrimaryDates, timeBinding_HasSecondaryDates, x));

            var currentObjects_Nested_TimeDimension_Dict = new Dictionary<ChronometricValueId, List<SaveableTimeDimension>>();
            foreach (var currentObject_Nested_TimeDimension in currentObjects_Nested_TimeDimension)
            {
                var key = new ChronometricValueId(currentObject_Nested_TimeDimension.EF_ProjectGuid, currentObject_Nested_TimeDimension.EF_RevisionNumber, currentObject_Nested_TimeDimension.EF_ChronometricValueGuid);
                if (!currentObjects_Nested_TimeDimension_Dict.ContainsKey(key))
                { currentObjects_Nested_TimeDimension_Dict.Add(key, new List<SaveableTimeDimension>()); }
                currentObjects_Nested_TimeDimension_Dict[key].Add(currentObject_Nested_TimeDimension);
            }

            var currentObjects_Nested_TimeAssessment_Dict = new Dictionary<ChronometricValueId, List<TimeAssessment>>();
            foreach (var currentObject_Nested_TimeAssessment in currentObjects_Nested_TimeAssessment)
            {
                var key = new ChronometricValueId(currentObject_Nested_TimeAssessment.EF_ProjectGuid, currentObject_Nested_TimeAssessment.EF_RevisionNumber, currentObject_Nested_TimeAssessment.EF_ChronometricValueGuid);
                if (!currentObjects_Nested_TimeAssessment_Dict.ContainsKey(key))
                { currentObjects_Nested_TimeAssessment_Dict.Add(key, new List<TimeAssessment>()); }
                currentObjects_Nested_TimeAssessment_Dict[key].Add(currentObject_Nested_TimeAssessment);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(ChronometricValue.TypeName_ChronometricValue, currentObjects_Main);
            batchReadState.Add(ChronometricValue.TypeName_SaveableTimeDimension, currentObjects_Nested_TimeDimension_Dict);
            batchReadState.Add(ChronometricValue.TypeName_TimeAssessment, currentObjects_Nested_TimeAssessment_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<ChronometricValue>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        public static List<ChronometricValue> DoAggregateRead(this DataSet dataSet, RevisionChain revisionChain)
        {
            var chronometricValuesDataTable = dataSet.Tables[ChronometricValue.DataSetSchema.ChronometricValues.TableName];
            var timeDimensionsDataTable = dataSet.Tables[ChronometricValue.DataSetSchema.SaveableTimeDimensions.TableName];
            var timeAssessmentsDataTable = dataSet.Tables[ChronometricValue.DataSetSchema.TimeAssessments.TableName];
            return DoAggregateRead(chronometricValuesDataTable, timeDimensionsDataTable, timeAssessmentsDataTable, revisionChain);
        }

        public static List<ChronometricValue> DoAggregateRead(this DataTable chronometricValuesDataTable, DataTable timeDimensionsDataTable, DataTable timeAssessmentsDataTable, RevisionChain revisionChain)
        {
            var currentObjects_Main = new List<ChronometricValue>();
            var currentObjects_Nested_TimeDimension = new List<SaveableTimeDimension>();
            var currentObjects_Nested_TimeAssessment = new List<TimeAssessment>();

            foreach (var dataRow in chronometricValuesDataTable.Select())
            {
                var value = new ChronometricValue();
                value.CopyDataRowToObject<ChronometricValue>(dataRow);
                currentObjects_Main.Add(value);
            }

            foreach (var dataRow in timeDimensionsDataTable.Select())
            {
                var value = new SaveableTimeDimension();
                value.CopyDataRowToObject<SaveableTimeDimension>(dataRow);
                currentObjects_Nested_TimeDimension.Add(value);
            }

            foreach (var dataRow in timeAssessmentsDataTable.Select())
            {
                var value = new TimeAssessment();
                value.CopyDataRowToObject<TimeAssessment>(dataRow);
                currentObjects_Nested_TimeAssessment.Add(value);
            }

            var currentObjects_Nested_TimeDimension_Dict = new Dictionary<ChronometricValueId, List<SaveableTimeDimension>>();
            foreach (var currentObject_Nested_TimeDimension in currentObjects_Nested_TimeDimension)
            {
                var key = new ChronometricValueId(currentObject_Nested_TimeDimension.EF_ProjectGuid, currentObject_Nested_TimeDimension.EF_RevisionNumber, currentObject_Nested_TimeDimension.EF_ChronometricValueGuid);
                if (!currentObjects_Nested_TimeDimension_Dict.ContainsKey(key))
                { currentObjects_Nested_TimeDimension_Dict.Add(key, new List<SaveableTimeDimension>()); }
                currentObjects_Nested_TimeDimension_Dict[key].Add(currentObject_Nested_TimeDimension);
            }

            var currentObjects_Nested_TimeAssessment_Dict = new Dictionary<ChronometricValueId, List<TimeAssessment>>();
            foreach (var currentObject_Nested_TimeAssessment in currentObjects_Nested_TimeAssessment)
            {
                var key = new ChronometricValueId(currentObject_Nested_TimeAssessment.EF_ProjectGuid, currentObject_Nested_TimeAssessment.EF_RevisionNumber, currentObject_Nested_TimeAssessment.EF_ChronometricValueGuid);
                if (!currentObjects_Nested_TimeAssessment_Dict.ContainsKey(key))
                { currentObjects_Nested_TimeAssessment_Dict.Add(key, new List<TimeAssessment>()); }
                currentObjects_Nested_TimeAssessment_Dict[key].Add(currentObject_Nested_TimeAssessment);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(ChronometricValue.TypeName_ChronometricValue, currentObjects_Main);
            batchReadState.Add(ChronometricValue.TypeName_SaveableTimeDimension, currentObjects_Nested_TimeDimension_Dict);
            batchReadState.Add(ChronometricValue.TypeName_TimeAssessment, currentObjects_Nested_TimeAssessment_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<ChronometricValue>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        internal static bool GetIsPartialRead(bool timeBinding_HasValue, bool timeBinding_HasPrimaryDates, bool timeBinding_HasSecondaryDates, TimeAssessment timeAssessment)
        {
            if (!timeBinding_HasValue)
            { return false; }

            var isPartialRead_Primary = (timeBinding_HasPrimaryDates && timeAssessment.EF_HasPrimaryTimeDimension);
            var isPartialRead_Secondary = (timeBinding_HasSecondaryDates && timeAssessment.EF_HasSecondaryTimeDimension);
            var isPartialRead = (isPartialRead_Primary || isPartialRead_Secondary);
            return isPartialRead;
        }

        #endregion
    }
}