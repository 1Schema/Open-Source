using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.DomainModeling.DataProviders;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;
using ReportElementIdRow = Decia.Business.Domain.Reporting.Internals.ReportElementIdRow;

namespace Decia.Business.Domain.Reporting
{
    #region Internals - Data Objects for Queries

    namespace Internals
    {
        public class ReportElementIdRow
        {
            public static int ConstructionCount = 0;

            public ReportElementIdRow()
            { ConstructionCount++; }

            public Guid EF_ProjectGuid { get; set; }
            public long EF_RevisionNumber { get; set; }
            public int EF_ModelTemplateNumber { get; set; }
            public Guid EF_ReportGuid { get; set; }
            public int EF_ReportElementNumber { get; set; }

            public bool EF_IsDeleted { get; set; }

            public string EF_Name { get; set; }
            public Nullable<long> EF_OrderNumber { get; set; }
            public Nullable<int> EF_Parent_ReportElementNumber { get; set; }

        }
    }

    #endregion

    public static class ReportElementBaseRepositoryExtensions
    {
        public const bool Default_AssertErrors = true;

        #region Methods to Get Nested Data Store

        public static IQueryable<SaveableElementStyle> GetNestedDataStore_ElementStyles<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository)
            where KDO : ReportElementBase<KDO>
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_ElementStyles).SelectMany(x => x).ToList();
                return new EnumerableQuery<SaveableElementStyle>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<SaveableElementStyle>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        public static IQueryable<SaveableDimensionLayout> GetNestedDataStore_DimensionLayouts<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository)
            where KDO : ReportElementBase<KDO>
        {
            if (repository.CurrentPersistenceType == DataSourcePersistenceType.None)
            {
                var mainObjects = repository.GetActualDataStore();
                var nestedObjects = (from vObj in mainObjects
                                     select vObj.EF_DimensionLayouts).SelectMany(x => x).ToList();
                return new EnumerableQuery<SaveableDimensionLayout>(nestedObjects);
            }
            else if (repository.CurrentPersistenceType == DataSourcePersistenceType.Database)
            {
                var dbContext = (DbContext)repository.CurrentDataSource;
                var nestedObjects = dbContext.Set<SaveableDimensionLayout>();
                return nestedObjects;
            }
            else
            { throw new InvalidOperationException("Unrecognized DataSourcePersistenceType encountered."); }
        }

        #endregion

        #region Queries to Read by Desired (Project, Revision)

        public static bool HasChanges_ForRevisionRange<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var changeCount = GetChangeCount_ForRevisionRange(repository, minRevisionNumber, maxRevisionNumber, revisionChain);
            return (changeCount > 0);
        }

        public static int GetChangeCount_ForRevisionRange<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
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

        public static IQueryable<ReportElementIdRow> ReadCurrentIdRows_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var projectGuid = revisionChain.ProjectGuid;
            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var disallowedRevisionNumbers = revisionChain.DisallowedRevisions;
            var versionedObjects = repository.GetActualDataStore();

            var possibleRows = (from vObj in versionedObjects
                                where vObj.EF_ProjectGuid == projectGuid
                                     && vObj.EF_RevisionNumber <= desiredRevisionNumber
                                     && !disallowedRevisionNumbers.Contains(vObj.EF_RevisionNumber)
                                group vObj by new { vObj.EF_ProjectGuid, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_ReportElementNumber, vObj.EF_IsDeleted } into gvObj
                                select new ReportElementIdRow()
                                {
                                    EF_ProjectGuid = gvObj.Key.EF_ProjectGuid,
                                    EF_RevisionNumber = gvObj.Max(vObj => vObj.EF_RevisionNumber),
                                    EF_ModelTemplateNumber = gvObj.Key.EF_ModelTemplateNumber,
                                    EF_ReportGuid = gvObj.Key.EF_ReportGuid,
                                    EF_ReportElementNumber = gvObj.Key.EF_ReportElementNumber,

                                    EF_IsDeleted = gvObj.Key.EF_IsDeleted,

                                    EF_Name = null,
                                    EF_OrderNumber = null,
                                    EF_Parent_ReportElementNumber = null,
                                });

            var currentRows = (from vObj in possibleRows.Where(x => !x.EF_IsDeleted)
                               join vObjD in possibleRows.Where(x => x.EF_IsDeleted) on new { vObj.EF_ProjectGuid, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_ReportElementNumber } equals new { vObjD.EF_ProjectGuid, vObjD.EF_ModelTemplateNumber, vObjD.EF_ReportGuid, vObjD.EF_ReportElementNumber } into gvObjD
                               from vObj_Deleted in gvObjD.DefaultIfEmpty()
                               select new ReportElementIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_ModelTemplateNumber = vObj.EF_ModelTemplateNumber,
                                   EF_ReportGuid = vObj.EF_ReportGuid,
                                   EF_ReportElementNumber = vObj.EF_ReportElementNumber,

                                   EF_IsDeleted = (vObj_Deleted == null) ? vObj.EF_IsDeleted : ((vObj.EF_RevisionNumber > vObj_Deleted.EF_RevisionNumber) ? vObj.EF_IsDeleted : vObj_Deleted.EF_IsDeleted),

                                   EF_Name = vObj.EF_Name,
                                   EF_OrderNumber = vObj.EF_OrderNumber,
                                   EF_Parent_ReportElementNumber = vObj.EF_Parent_ReportElementNumber,
                               }).Where(vObj => !vObj.EF_IsDeleted);

            return currentRows;
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new ReportElementId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid, x.EF_ReportElementNumber)).ToList();
            return currentIds;
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => (vObj.EF_ModelTemplateNumber == modelTemplateNumber));
            var currentIds = currentRows.ToList().Select(x => new ReportElementId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid, x.EF_ReportElementNumber)).ToList();
            return currentIds;
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => ((vObj.EF_ModelTemplateNumber == modelTemplateNumber) && (vObj.EF_ReportGuid == reportGuid)));
            var currentIds = currentRows.ToList().Select(x => new ReportElementId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid, x.EF_ReportElementNumber)).ToList();
            return currentIds;
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, int? parentElementNumber, RevisionChain revisionChain)
          where KDO : ReportElementBase<KDO>
        {
            var parentElementNumbers = new List<int?>();
            parentElementNumbers.Add(parentElementNumber);
            return ReadCurrentKeys_ForDesiredRevision(repository, modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain);
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, ICollection<int?> parentElementNumbers, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var possibleRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var versionedObjects = repository.GetActualDataStore();

            var currentRows = (from pRow in possibleRows
                               join vObj in versionedObjects on new { pRow.EF_ProjectGuid, pRow.EF_RevisionNumber, pRow.EF_ModelTemplateNumber, pRow.EF_ReportGuid, pRow.EF_ReportElementNumber } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_ReportElementNumber }
                               where (parentElementNumbers.Contains(vObj.EF_ParentElementNumber))
                               select new ReportElementIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_ModelTemplateNumber = vObj.EF_ModelTemplateNumber,
                                   EF_ReportGuid = vObj.EF_ReportGuid,
                                   EF_ReportElementNumber = vObj.EF_ReportElementNumber,

                                   EF_IsDeleted = vObj.EF_IsDeleted,

                                   EF_Name = vObj.EF_Name,
                                   EF_OrderNumber = vObj.EF_OrderNumber,
                                   EF_Parent_ReportElementNumber = vObj.EF_ParentElementNumber,
                               });

            var currentIds = currentRows.ToList().Select(x => new ReportElementId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid, x.EF_ReportElementNumber)).ToHashSet();
            return currentIds;
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, IEnumerable<int> elementNumbers, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            return ReadCurrentKeys_ForDesiredRevision(repository, modelTemplateNumber, reportGuid, elementNumbers, revisionChain, Default_AssertErrors);
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, IEnumerable<int> elementNumbers, RevisionChain revisionChain, bool assertErrors)
            where KDO : ReportElementBase<KDO>
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => ((vObj.EF_ModelTemplateNumber == modelTemplateNumber) && (vObj.EF_ReportGuid == reportGuid) && elementNumbers.Contains(vObj.EF_ReportElementNumber)));
            var currentIds = currentRows.ToList().Select(x => new ReportElementId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid, x.EF_ReportElementNumber)).ToHashSet();

            if (assertErrors && (currentIds.Count != elementNumbers.Count()))
            { throw new InvalidOperationException("The Report Element Numbers all must be unique for a given Report."); }

            return currentIds;
        }

        public static Nullable<ReportElementId> ReadCurrentKey_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, int elementNumber, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var elementNumbers = new int[] { elementNumber };
            var currentIds = ReadCurrentKeys_ForDesiredRevision(repository, modelTemplateNumber, reportGuid, elementNumbers, revisionChain, Default_AssertErrors);
            return currentIds.GetAndAssertUniqueValue();
        }

        public static Nullable<ReportElementId> ReadCurrentKey_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, int elementNumber, RevisionChain revisionChain, bool assertErrors)
            where KDO : ReportElementBase<KDO>
        {
            var elementNumbers = new int[] { elementNumber };
            var currentIds = ReadCurrentKeys_ForDesiredRevision(repository, modelTemplateNumber, reportGuid, elementNumbers, revisionChain, assertErrors);
            return currentIds.GetAndAssertUniqueValue();
        }

        public static Nullable<ReportElementId> ReadDefaultKey_ForDesiredRevision<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, int modelTemplateNumber, Guid reportGuid, RevisionChain revisionChain)
            where KDO : ReportElementBase<KDO>
        {
            var possibleRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var versionedObjects = repository.GetActualDataStore();

            var currentRows = (from pRow in possibleRows
                               join vObj in versionedObjects on new { pRow.EF_ProjectGuid, pRow.EF_RevisionNumber, pRow.EF_ModelTemplateNumber, pRow.EF_ReportGuid, pRow.EF_ReportElementNumber } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_ReportElementNumber }
                               where (vObj.EF_ReportGuid == reportGuid)
                                    && (vObj.EF_ModelTemplateNumber == modelTemplateNumber)
                                    && !vObj.EF_IsDeleted
                               select new ReportElementIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_ModelTemplateNumber = vObj.EF_ModelTemplateNumber,
                                   EF_ReportGuid = vObj.EF_ReportGuid,
                                   EF_ReportElementNumber = vObj.EF_ReportElementNumber,

                                   EF_IsDeleted = vObj.EF_IsDeleted,

                                   EF_Name = vObj.EF_Name,
                                   EF_OrderNumber = vObj.EF_OrderNumber,
                                   EF_Parent_ReportElementNumber = vObj.EF_ParentElementNumber,
                               });

            var defaultRow = (from cRow in currentRows
                              orderby cRow.EF_OrderNumber, cRow.EF_Name
                              select new ReportElementIdRow()
                              {
                                  EF_ProjectGuid = cRow.EF_ProjectGuid,
                                  EF_RevisionNumber = cRow.EF_RevisionNumber,
                                  EF_ModelTemplateNumber = cRow.EF_ModelTemplateNumber,
                                  EF_ReportGuid = cRow.EF_ReportGuid,
                                  EF_ReportElementNumber = cRow.EF_ReportElementNumber,

                                  EF_IsDeleted = cRow.EF_IsDeleted,

                                  EF_Name = cRow.EF_Name,
                                  EF_OrderNumber = cRow.EF_OrderNumber,
                                  EF_Parent_ReportElementNumber = cRow.EF_Parent_ReportElementNumber,
                              }).FirstOrDefault();

            if (defaultRow == null)
            { return null; }

            return new ReportElementId(defaultRow.EF_ProjectGuid, defaultRow.EF_RevisionNumber, defaultRow.EF_ModelTemplateNumber, defaultRow.EF_ReportGuid, defaultRow.EF_ReportElementNumber);
        }

        #endregion

        #region Queries to Read for Specific Revision ONLY

        public static ICollection<KDO> ReadForSpecificRevisionId<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, RevisionId revisionId)
            where KDO : ReportElementBase<KDO>
        {
            LinqQuery<KDO> query = new LinqQuery<KDO>((KDO dObj) => ((dObj.EF_ProjectGuid == revisionId.ProjectGuid) && (dObj.EF_RevisionNumber == revisionId.RevisionNumber_NonNull)));
            ICollection<KDO> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ReportElementId, KDO> ReadDictionaryForSpecificRevisionId<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, RevisionId revisionId)
            where KDO : ReportElementBase<KDO>
        {
            ICollection<KDO> list = ReadForSpecificRevisionId(repository, revisionId);
            IDictionary<ReportElementId, KDO> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Creator

        public static ICollection<KDO> ReadForCreatorId<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, UserId creatorId)
            where KDO : ReportElementBase<KDO>
        {
            LinqQuery<KDO> query = new LinqQuery<KDO>((KDO dObj) => (dObj.EF_CreatorGuid == creatorId.UserGuid));
            ICollection<KDO> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ReportElementId, KDO> ReadDictionaryForCreatorId<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, UserId creatorId)
            where KDO : ReportElementBase<KDO>
        {
            ICollection<KDO> list = ReadForCreatorId(repository, creatorId);
            IDictionary<ReportElementId, KDO> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Owner

        public static ICollection<KDO> ReadForOwnerId<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, ISiteActor siteActorRef)
            where KDO : ReportElementBase<KDO>
        {
            LinqQuery<KDO> query = new LinqQuery<KDO>((KDO dObj) => ((dObj.EF_OwnerType == (int)siteActorRef.ActorType) && (dObj.EF_OwnerGuid == siteActorRef.ActorGuid)));
            ICollection<KDO> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ReportElementId, KDO> ReadDictionaryForOwnerId<KDO>(this IReadOnlyRepository<ReportElementId, KDO> repository, ISiteActor siteActorRef)
            where KDO : ReportElementBase<KDO>
        {
            ICollection<KDO> list = ReadForOwnerId(repository, siteActorRef);
            IDictionary<ReportElementId, KDO> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion
    }
}