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
using DExpression = Decia.Business.Domain.Formulas.Expressions.Expression;
using DArgument = Decia.Business.Domain.Formulas.Expressions.Argument;
using FormulaIdRow = Decia.Business.Domain.Formulas.Internals.FormulaIdRow;

namespace Decia.Business.Domain.Formulas
{
    #region Internals - Data Objects for Queries

    namespace Internals
    {
        public class FormulaIdRow
        {
            public static int ConstructionCount = 0;

            public FormulaIdRow()
            { ConstructionCount++; }

            public Guid EF_ProjectGuid { get; set; }
            public long EF_RevisionNumber { get; set; }
            public Guid EF_FormulaGuid { get; set; }

            public bool EF_IsDeleted { get; set; }
        }
    }

    #endregion

    public static class FormulaRepositoryExtensions
    {
        public static bool Prefer_StoredProcs = AdoNetUtils.Default_Prefer_StoredProcs;
        public const bool Default_AssertAllFound = true;

        #region Methods to Get Nested Data Store

        public static IQueryable<DExpression> GetNestedDataStore_Expressions(this IReadOnlyRepository<FormulaId, Formula> repository)
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_Expressions).SelectMany(x => x).ToList();
                return new EnumerableQuery<DExpression>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<DExpression>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        public static IQueryable<DArgument> GetNestedDataStore_Arguments(this IReadOnlyRepository<FormulaId, Formula> repository)
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_Arguments).SelectMany(x => x).ToList();
                return new EnumerableQuery<DArgument>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<DArgument>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        #endregion

        #region Queries to Read by Desired (Project, Revision)

        public static bool HasChanges_ForRevisionRange(this IReadOnlyRepository<FormulaId, Formula> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            var changeCount = GetChangeCount_ForRevisionRange(repository, minRevisionNumber, maxRevisionNumber, revisionChain);
            return (changeCount > 0);
        }

        public static int GetChangeCount_ForRevisionRange(this IReadOnlyRepository<FormulaId, Formula> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
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

        public static FormulaDataSet ReadCurrentDataSet_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, RevisionChain revisionChain)
        {
            return ReadCurrentDataSet_ForDesiredRevision(repository, null, revisionChain, false);
        }

        public static FormulaDataSet ReadCurrentDataSet_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, IEnumerable<Guid> formulaGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            var databaseContext = (repository.CurrentDataSource as DbContext);

            var formulaGuidsAsText = (string)null;
            if (formulaGuids != null)
            {
                var formulaObjects = formulaGuids.Select(x => (object)x.ToString().ToUpper()).ToList();
                formulaGuidsAsText = AdoNetUtils.CovertToKeyMatchingText(formulaObjects, AdoNetUtils.KeyMatching_Separator_Text, AdoNetUtils.KeyMatching_Separator_Text, false);
            }

            using (var connection = databaseContext.GetConnection())
            {
                var parameters = new Dictionary<string, object>();
                parameters.Add(Formula.SP_ReadCurrent_Parameter0_Name, revisionChain.ProjectGuid);
                parameters.Add(Formula.SP_ReadCurrent_Parameter1_Name, revisionChain.DesiredRevisionNumber);
                parameters.Add(Formula.SP_ReadCurrent_Parameter2_Name, formulaGuidsAsText);

                var dataSet_UnTyped = connection.ExecuteStoredProcedureWithResult(Formula.SP_ReadCurrent_Name, parameters);

                var dataSet_Typed = new FormulaDataSet();
                dataSet_Typed.EnforceConstraints = false;
                dataSet_Typed.Formulae.Merge(dataSet_UnTyped.Tables[1]);
                dataSet_Typed.Expressions.Merge(dataSet_UnTyped.Tables[2]);
                dataSet_Typed.Arguments.Merge(dataSet_UnTyped.Tables[3]);
                dataSet_Typed.EnforceConstraints = true;
                dataSet_Typed.AcceptChanges();

                if (assertAllFound)
                {
                    if (formulaGuids.Count() != dataSet_Typed.Formulae.Count())
                    { throw new InvalidOperationException("Some Formulas could not be found."); }
                }

                return dataSet_Typed;
            };
        }

        public static IQueryable<FormulaIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, RevisionChain revisionChain)
        {
            var projectGuid = revisionChain.ProjectGuid;
            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var disallowedRevisionNumbers = revisionChain.DisallowedRevisions;
            var versionedObjects = repository.GetActualDataStore();

            var possibleRows = (from vObj in versionedObjects
                                where vObj.EF_ProjectGuid == projectGuid
                                     && vObj.EF_RevisionNumber <= desiredRevisionNumber
                                     && !disallowedRevisionNumbers.Contains(vObj.EF_RevisionNumber)
                                group vObj by new { vObj.EF_ProjectGuid, vObj.EF_FormulaGuid, vObj.EF_IsDeleted } into gvObj
                                select new FormulaIdRow()
                                {
                                    EF_ProjectGuid = gvObj.Key.EF_ProjectGuid,
                                    EF_RevisionNumber = gvObj.Max(vObj => vObj.EF_RevisionNumber),
                                    EF_FormulaGuid = gvObj.Key.EF_FormulaGuid,
                                    EF_IsDeleted = gvObj.Key.EF_IsDeleted,
                                });

            var currentRows = (from vObj in possibleRows.Where(x => !x.EF_IsDeleted)
                               join vObjD in possibleRows.Where(x => x.EF_IsDeleted) on new { vObj.EF_ProjectGuid, vObj.EF_FormulaGuid } equals new { vObjD.EF_ProjectGuid, vObjD.EF_FormulaGuid } into gvObjD
                               from vObj_Deleted in gvObjD.DefaultIfEmpty()
                               select new FormulaIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_FormulaGuid = vObj.EF_FormulaGuid,
                                   EF_IsDeleted = (vObj_Deleted == null) ? vObj.EF_IsDeleted : ((vObj.EF_RevisionNumber > vObj_Deleted.EF_RevisionNumber) ? vObj.EF_IsDeleted : vObj_Deleted.EF_IsDeleted),
                               }).Where(vObj => !vObj.EF_IsDeleted);

            return currentRows;
        }

        public static ICollection<FormulaId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, RevisionChain revisionChain)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
                var currentIds = currentRows.ToList().Select(x => new FormulaId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_FormulaGuid)).ToList();
                return currentIds;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, revisionChain);
                var currentKeys = currentDataSet.Formulae.Select(x => new FormulaId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_FormulaGuid)).ToList();
                return currentKeys;
            }
        }

        public static ICollection<Formula> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, RevisionChain revisionChain)
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

        #region Read Collection by (formulaGuids)

        public static IQueryable<FormulaIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, IEnumerable<Guid> formulaGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => formulaGuids.Contains(vObj.EF_FormulaGuid));

            if (assertAllFound)
            {
                if (formulaGuids.Count() != currentRows.Count())
                { throw new InvalidOperationException("Some Formulas could not be found."); }
            }

            return currentRows;
        }

        public static ICollection<FormulaId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, IEnumerable<Guid> formulaGuids, RevisionChain revisionChain)
        {
            return ReadCurrentKeys_ForDesiredRevision(repository, formulaGuids, revisionChain, Default_AssertAllFound);
        }

        public static ICollection<FormulaId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, IEnumerable<Guid> formulaGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, formulaGuids, revisionChain, assertAllFound);
                var currentIds = currentRows.ToList().Select(x => new FormulaId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_FormulaGuid)).ToHashSet();
                return currentIds;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, formulaGuids, revisionChain, assertAllFound);
                var currentKeys = currentDataSet.Formulae.Select(x => new FormulaId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_FormulaGuid)).ToList();
                return currentKeys;
            }
        }

        public static ICollection<Formula> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, IEnumerable<Guid> formulaGuids, RevisionChain revisionChain)
        {
            return ReadCurrentObjects_ForDesiredRevision(repository, formulaGuids, revisionChain, Default_AssertAllFound);
        }

        public static ICollection<Formula> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, IEnumerable<Guid> formulaGuids, RevisionChain revisionChain, bool assertAllFound)
        {
            if (!Prefer_StoredProcs || (repository.CurrentPersistenceType != DataSourcePersistenceType.Database))
            {
                var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, formulaGuids, revisionChain, assertAllFound);
                var currentObjects = DoAggregateRead(repository, currentRows);
                return currentObjects;
            }
            else
            {
                var currentDataSet = ReadCurrentDataSet_ForDesiredRevision(repository, formulaGuids, revisionChain, assertAllFound);
                var currentObjects = DoAggregateRead(currentDataSet, revisionChain);
                return currentObjects;
            }
        }

        #endregion

        #region Read Object by (formulaGuid)

        public static FormulaId ReadCurrentKey_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, Guid formulaGuid, RevisionChain revisionChain)
        {
            var formulaGuids = new Guid[] { formulaGuid };
            var currentIds = ReadCurrentKeys_ForDesiredRevision(repository, formulaGuids, revisionChain);
            return currentIds.GetAndAssertUniqueValue(false).Value;
        }

        public static Formula ReadCurrentObject_ForDesiredRevision(this IReadOnlyRepository<FormulaId, Formula> repository, Guid formulaGuid, RevisionChain revisionChain)
        {
            var formulaGuids = new Guid[] { formulaGuid };
            var currentObjects = ReadCurrentObjects_ForDesiredRevision(repository, formulaGuids, revisionChain);
            return currentObjects.FirstOrDefault();
        }

        #endregion

        #endregion

        #region Queries to Read for Specific Revision ONLY

        public static ICollection<Formula> ReadForSpecificRevisionId(this IReadOnlyRepository<FormulaId, Formula> repository, RevisionId revisionId)
        {
            LinqQuery<Formula> query = new LinqQuery<Formula>((Formula dObj) => ((dObj.EF_ProjectGuid == revisionId.ProjectGuid) && (dObj.EF_RevisionNumber == revisionId.RevisionNumber_NonNull)));
            ICollection<Formula> result = repository.Read(query);
            return result;
        }

        public static IDictionary<FormulaId, Formula> ReadDictionaryForSpecificRevisionId(this IReadOnlyRepository<FormulaId, Formula> repository, RevisionId revisionId)
        {
            ICollection<Formula> list = ReadForSpecificRevisionId(repository, revisionId);
            IDictionary<FormulaId, Formula> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Creator

        public static ICollection<Formula> ReadForCreatorId(this IReadOnlyRepository<FormulaId, Formula> repository, UserId creatorId)
        {
            LinqQuery<Formula> query = new LinqQuery<Formula>((Formula dObj) => (dObj.EF_CreatorGuid == creatorId.UserGuid));
            ICollection<Formula> result = repository.Read(query);
            return result;
        }

        public static IDictionary<FormulaId, Formula> ReadDictionaryForCreatorId(this IReadOnlyRepository<FormulaId, Formula> repository, UserId creatorId)
        {
            ICollection<Formula> list = ReadForCreatorId(repository, creatorId);
            IDictionary<FormulaId, Formula> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Owner

        public static ICollection<Formula> ReadForOwnerId(this IReadOnlyRepository<FormulaId, Formula> repository, ISiteActor siteActorRef)
        {
            LinqQuery<Formula> query = new LinqQuery<Formula>((Formula dObj) => ((dObj.EF_OwnerType == (int)siteActorRef.ActorType) && (dObj.EF_OwnerGuid == siteActorRef.ActorGuid)));
            ICollection<Formula> result = repository.Read(query);
            return result;
        }

        public static IDictionary<FormulaId, Formula> ReadDictionaryForOwnerId(this IReadOnlyRepository<FormulaId, Formula> repository, ISiteActor siteActorRef)
        {
            ICollection<Formula> list = ReadForOwnerId(repository, siteActorRef);
            IDictionary<FormulaId, Formula> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Helper Methods for Reading Entire Aggregate

        public static ICollection<Formula> DoAggregateRead(this IReadOnlyRepository<FormulaId, Formula> repository, IQueryable<FormulaIdRow> currentRows)
        {
            var versionedObjects_Main = repository.GetActualDataStore();
            var versionedObjects_Nested_Expression = repository.GetNestedDataStore_Expressions();
            var versionedObjects_Nested_Argument = repository.GetNestedDataStore_Arguments();

            var currentObjects_Main = (from cRow in currentRows
                                       join vObj in versionedObjects_Main on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_FormulaGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_FormulaGuid }
                                       select vObj).ToList();
            var currentObjects_Nested_Expression = (from cRow in currentRows
                                                    join vObj in versionedObjects_Nested_Expression on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_FormulaGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_FormulaGuid }
                                                    select vObj).ToList();
            var currentObjects_Nested_Argument = (from cRow in currentRows
                                                  join vObj in versionedObjects_Nested_Argument on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_FormulaGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_FormulaGuid }
                                                  select vObj).ToList();

            var currentObjects_Nested_Expression_Dict = new Dictionary<FormulaId, List<DExpression>>();
            foreach (var currentObject_Nested_Expression in currentObjects_Nested_Expression)
            {
                var key = new FormulaId(currentObject_Nested_Expression.EF_ProjectGuid, currentObject_Nested_Expression.EF_RevisionNumber, currentObject_Nested_Expression.EF_FormulaGuid);
                if (!currentObjects_Nested_Expression_Dict.ContainsKey(key))
                { currentObjects_Nested_Expression_Dict.Add(key, new List<DExpression>()); }
                currentObjects_Nested_Expression_Dict[key].Add(currentObject_Nested_Expression);
            }

            var currentObjects_Nested_Argument_Dict = new Dictionary<FormulaId, List<DArgument>>();
            foreach (var currentObject_Nested_Argument in currentObjects_Nested_Argument)
            {
                var key = new FormulaId(currentObject_Nested_Argument.EF_ProjectGuid, currentObject_Nested_Argument.EF_RevisionNumber, currentObject_Nested_Argument.EF_FormulaGuid);
                if (!currentObjects_Nested_Argument_Dict.ContainsKey(key))
                { currentObjects_Nested_Argument_Dict.Add(key, new List<DArgument>()); }
                currentObjects_Nested_Argument_Dict[key].Add(currentObject_Nested_Argument);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(Formula.TypeName_Formula, currentObjects_Main);
            batchReadState.Add(Formula.TypeName_Expression, currentObjects_Nested_Expression_Dict);
            batchReadState.Add(Formula.TypeName_Argument, currentObjects_Nested_Argument_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<Formula>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        public static List<Formula> DoAggregateRead(this DataSet dataSet, RevisionChain revisionChain)
        {
            var formulasDataTable = dataSet.Tables[Formula.DataSetSchema.Formulae.TableName];
            var expressionsDataTable = dataSet.Tables[Formula.DataSetSchema.Expressions.TableName];
            var argumentsDataTable = dataSet.Tables[Formula.DataSetSchema.Arguments.TableName];
            return DoAggregateRead(formulasDataTable, expressionsDataTable, argumentsDataTable, revisionChain);
        }

        public static List<Formula> DoAggregateRead(this DataTable formulasDataTable, DataTable expressionsDataTable, DataTable argumentsDataTable, RevisionChain revisionChain)
        {
            var currentObjects_Main = new List<Formula>();
            var currentObjects_Nested_Expression = new List<DExpression>();
            var currentObjects_Nested_Argument = new List<DArgument>();

            foreach (var dataRow in formulasDataTable.Select())
            {
                var value = new Formula();
                value.CopyDataRowToObject<Formula>(dataRow);
                currentObjects_Main.Add(value);
            }

            foreach (var dataRow in expressionsDataTable.Select())
            {
                var value = new DExpression();
                value.CopyDataRowToObject<DExpression>(dataRow);
                currentObjects_Nested_Expression.Add(value);
            }

            foreach (var dataRow in argumentsDataTable.Select())
            {
                var value = new DArgument();
                value.CopyDataRowToObject<DArgument>(dataRow);
                currentObjects_Nested_Argument.Add(value);
            }

            var currentObjects_Nested_Expression_Dict = new Dictionary<FormulaId, List<DExpression>>();
            foreach (var currentObject_Nested_Expression in currentObjects_Nested_Expression)
            {
                var key = new FormulaId(currentObject_Nested_Expression.EF_ProjectGuid, currentObject_Nested_Expression.EF_RevisionNumber, currentObject_Nested_Expression.EF_FormulaGuid);
                if (!currentObjects_Nested_Expression_Dict.ContainsKey(key))
                { currentObjects_Nested_Expression_Dict.Add(key, new List<DExpression>()); }
                currentObjects_Nested_Expression_Dict[key].Add(currentObject_Nested_Expression);
            }

            var currentObjects_Nested_Argument_Dict = new Dictionary<FormulaId, List<DArgument>>();
            foreach (var currentObject_Nested_Argument in currentObjects_Nested_Argument)
            {
                var key = new FormulaId(currentObject_Nested_Argument.EF_ProjectGuid, currentObject_Nested_Argument.EF_RevisionNumber, currentObject_Nested_Argument.EF_FormulaGuid);
                if (!currentObjects_Nested_Argument_Dict.ContainsKey(key))
                { currentObjects_Nested_Argument_Dict.Add(key, new List<DArgument>()); }
                currentObjects_Nested_Argument_Dict[key].Add(currentObject_Nested_Argument);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(Formula.TypeName_Formula, currentObjects_Main);
            batchReadState.Add(Formula.TypeName_Expression, currentObjects_Nested_Expression_Dict);
            batchReadState.Add(Formula.TypeName_Argument, currentObjects_Nested_Argument_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<Formula>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        #endregion
    }
}