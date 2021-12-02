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
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;
using ReportIdRow = Decia.Business.Domain.Reporting.Internals.ReportIdRow;

namespace Decia.Business.Domain.Reporting
{
    #region Internals - Data Objects for Queries

    namespace Internals
    {
        public class ReportIdRow
        {
            public static int ConstructionCount = 0;

            public ReportIdRow()
            { ConstructionCount++; }

            public Guid EF_ProjectGuid { get; set; }
            public long EF_RevisionNumber { get; set; }
            public int EF_ModelTemplateNumber { get; set; }
            public Guid EF_ReportGuid { get; set; }

            public bool EF_IsDeleted { get; set; }

            public string EF_Name { get; set; }
            public Nullable<long> EF_OrderNumber { get; set; }
            public Nullable<int> EF_StructuralTypeType { get; set; }
            public Nullable<int> EF_StructuralTypeId { get; set; }
        }
    }

    #endregion

    public static class ReportRepositoryExtensions
    {
        #region Methods to Get Nested Data Store

        public static IQueryable<SaveableElementStyle> GetNestedDataStore_ElementStyles(this IReadOnlyRepository<ReportId, Report> repository)
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

        public static IQueryable<SaveableDimensionLayout> GetNestedDataStore_DimensionLayouts(this IReadOnlyRepository<ReportId, Report> repository)
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

        public static bool HasChanges_ForRevisionRange(this IReadOnlyRepository<ReportId, Report> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            var changeCount = GetChangeCount_ForRevisionRange(repository, minRevisionNumber, maxRevisionNumber, revisionChain);
            return (changeCount > 0);
        }

        public static int GetChangeCount_ForRevisionRange(this IReadOnlyRepository<ReportId, Report> repository, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
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

        public static IQueryable<ReportIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, RevisionChain revisionChain)
        {
            var projectGuid = revisionChain.ProjectGuid;
            var desiredRevisionNumber = revisionChain.DesiredRevisionNumber;
            var disallowedRevisionNumbers = revisionChain.DisallowedRevisions;
            var versionedObjects = repository.GetActualDataStore();

            var possibleRows = (from vObj in versionedObjects
                                where vObj.EF_ProjectGuid == projectGuid
                                     && vObj.EF_RevisionNumber <= desiredRevisionNumber
                                     && !disallowedRevisionNumbers.Contains(vObj.EF_RevisionNumber)
                                group vObj by new { vObj.EF_ProjectGuid, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_IsDeleted } into gvObj
                                select new ReportIdRow()
                                {
                                    EF_ProjectGuid = gvObj.Key.EF_ProjectGuid,
                                    EF_RevisionNumber = gvObj.Max(vObj => vObj.EF_RevisionNumber),
                                    EF_ModelTemplateNumber = gvObj.Key.EF_ModelTemplateNumber,
                                    EF_ReportGuid = gvObj.Key.EF_ReportGuid,

                                    EF_IsDeleted = gvObj.Key.EF_IsDeleted,

                                    EF_Name = null,
                                    EF_OrderNumber = null,
                                    EF_StructuralTypeType = null,
                                    EF_StructuralTypeId = null,
                                });

            var currentRows = (from vObj in possibleRows.Where(x => !x.EF_IsDeleted)
                               join vObjD in possibleRows.Where(x => x.EF_IsDeleted) on new { vObj.EF_ProjectGuid, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid } equals new { vObjD.EF_ProjectGuid, vObjD.EF_ModelTemplateNumber, vObjD.EF_ReportGuid } into gvObjD
                               from vObj_Deleted in gvObjD.DefaultIfEmpty()
                               select new ReportIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_ModelTemplateNumber = vObj.EF_ModelTemplateNumber,
                                   EF_ReportGuid = vObj.EF_ReportGuid,

                                   EF_IsDeleted = (vObj_Deleted == null) ? vObj.EF_IsDeleted : ((vObj.EF_RevisionNumber > vObj_Deleted.EF_RevisionNumber) ? vObj.EF_IsDeleted : vObj_Deleted.EF_IsDeleted),

                                   EF_Name = vObj.EF_Name,
                                   EF_OrderNumber = vObj.EF_OrderNumber,
                                   EF_StructuralTypeType = vObj.EF_StructuralTypeType,
                                   EF_StructuralTypeId = vObj.EF_StructuralTypeId,
                               }).Where(vObj => !vObj.EF_IsDeleted);

            return currentRows;
        }

        public static ICollection<ReportId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new ReportId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid)).ToList();
            return currentIds;
        }

        public static ICollection<Report> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var currentObjects = DoAggregateRead(repository, currentRows);
            return currentObjects;
        }

        public static IQueryable<ReportIdRow> ReadCurrentIdRows_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => (vObj.EF_ModelTemplateNumber == modelTemplateNumber));
            return currentRows;
        }

        public static ICollection<ReportId> ReadCurrentKeys_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, modelTemplateNumber, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new ReportId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid)).ToList();
            return currentIds;
        }

        public static ICollection<Report> ReadCurrentObjects_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, modelTemplateNumber, revisionChain);
            var currentObjects = DoAggregateRead(repository, currentRows);
            return currentObjects;
        }

        public static IQueryable<ReportIdRow> ReadCurrentIdRow_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Guid reportGuid, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain)
                .Where(vObj => ((vObj.EF_ModelTemplateNumber == modelTemplateNumber) && (vObj.EF_ReportGuid == reportGuid)))
                .Take(1);
            return currentRows;
        }

        public static Nullable<ReportId> ReadCurrentKey_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Guid reportGuid, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRow_ForDesiredRevision(repository, modelTemplateNumber, reportGuid, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new ReportId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid)).ToHashSet();
            return currentIds.GetAndAssertUniqueValue();
        }

        public static Report ReadCurrentObject_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Guid reportGuid, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRow_ForDesiredRevision(repository, modelTemplateNumber, reportGuid, revisionChain);
            var currentObjects = DoAggregateRead(repository, currentRows);
            return currentObjects.FirstOrDefault();
        }

        public static Nullable<ReportId> ReadDefaultKey_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, RevisionChain revisionChain)
        {
            var currentIds = ReadCurrentKeys_ForDesiredRevision(repository, modelTemplateNumber, revisionChain);
            var currentId = (currentIds.Count > 0) ? currentIds.First() : (ReportId?)null;
            return currentId;
        }

        public static Report ReadDefaultObject_ForDesiredRevision(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, RevisionChain revisionChain)
        {
            var currentObjects = ReadCurrentObjects_ForDesiredRevision(repository, modelTemplateNumber, revisionChain);
            var currentObject = currentObjects.FirstOrDefault();
            return currentObject;
        }

        public static IQueryable<ReportIdRow> ReadCurrentIdRows_ForDesiredRevisionAndContainingType(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Nullable<StructuralTypeOption> containing_StructuralTypeOption, int containing_StructuralTypeNumber, RevisionChain revisionChain)
        {
            var possibleRows = ReadCurrentIdRows_ForDesiredRevision(repository, revisionChain);
            var versionedObjects = repository.GetActualDataStore();

            var containing_StructuralTypeTypeAsInt = containing_StructuralTypeOption.HasValue ? (Nullable<int>)containing_StructuralTypeOption.Value.GetModelObjectType() : (Nullable<int>)null;

            var currentRows = (from pRow in possibleRows
                               join vObj in versionedObjects on new { pRow.EF_ProjectGuid, pRow.EF_RevisionNumber, pRow.EF_ModelTemplateNumber, pRow.EF_ReportGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid }
                               where (!containing_StructuralTypeTypeAsInt.HasValue || (vObj.EF_StructuralTypeType == containing_StructuralTypeTypeAsInt))
                                    && (vObj.EF_StructuralTypeId == containing_StructuralTypeNumber)
                                    && (vObj.EF_ModelTemplateNumber == modelTemplateNumber)
                                    && !vObj.EF_IsDeleted
                               select new ReportIdRow()
                               {
                                   EF_ProjectGuid = vObj.EF_ProjectGuid,
                                   EF_RevisionNumber = vObj.EF_RevisionNumber,
                                   EF_ModelTemplateNumber = vObj.EF_ModelTemplateNumber,
                                   EF_ReportGuid = vObj.EF_ReportGuid,

                                   EF_IsDeleted = vObj.EF_IsDeleted,

                                   EF_Name = vObj.EF_Name,
                                   EF_OrderNumber = vObj.EF_OrderNumber,
                                   EF_StructuralTypeType = vObj.EF_StructuralTypeType,
                                   EF_StructuralTypeId = vObj.EF_StructuralTypeId,
                               });

            return currentRows;
        }

        public static ICollection<ReportId> ReadCurrentKeys_ForDesiredRevisionAndContainingType(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Nullable<StructuralTypeOption> containing_StructuralTypeOption, int containing_StructuralTypeNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevisionAndContainingType(repository, modelTemplateNumber, containing_StructuralTypeOption, containing_StructuralTypeNumber, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new ReportId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid)).ToList();
            return currentIds;
        }

        public static ICollection<Report> ReadCurrentObjects_ForDesiredRevisionAndContainingType(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Nullable<StructuralTypeOption> containing_StructuralTypeOption, int containing_StructuralTypeNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevisionAndContainingType(repository, modelTemplateNumber, containing_StructuralTypeOption, containing_StructuralTypeNumber, revisionChain);
            var currentObjects = DoAggregateRead(repository, currentRows);
            return currentObjects;
        }

        public static IQueryable<ReportIdRow> ReadDefaultIdRow_ForDesiredRevisionAndContainingType(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Nullable<StructuralTypeOption> containing_StructuralTypeOption, int containing_StructuralTypeNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadCurrentIdRows_ForDesiredRevisionAndContainingType(repository, modelTemplateNumber, containing_StructuralTypeOption, containing_StructuralTypeNumber, revisionChain);

            var defaultRows = (from cRow in currentRows
                               orderby cRow.EF_OrderNumber, cRow.EF_Name
                               select new ReportIdRow()
                               {
                                   EF_ProjectGuid = cRow.EF_ProjectGuid,
                                   EF_RevisionNumber = cRow.EF_RevisionNumber,
                                   EF_ModelTemplateNumber = cRow.EF_ModelTemplateNumber,
                                   EF_ReportGuid = cRow.EF_ReportGuid,

                                   EF_IsDeleted = cRow.EF_IsDeleted,

                                   EF_Name = cRow.EF_Name,
                                   EF_OrderNumber = cRow.EF_OrderNumber,
                                   EF_StructuralTypeType = cRow.EF_StructuralTypeType,
                                   EF_StructuralTypeId = cRow.EF_StructuralTypeId,
                               }).Take(1);

            return defaultRows;
        }

        public static Nullable<ReportId> ReadDefaultKey_ForDesiredRevisionAndContainingType(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Nullable<StructuralTypeOption> containing_StructuralTypeOption, int containing_StructuralTypeNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadDefaultIdRow_ForDesiredRevisionAndContainingType(repository, modelTemplateNumber, containing_StructuralTypeOption, containing_StructuralTypeNumber, revisionChain);
            var currentIds = currentRows.ToList().Select(x => new ReportId(x.EF_ProjectGuid, x.EF_RevisionNumber, x.EF_ModelTemplateNumber, x.EF_ReportGuid)).ToHashSet();
            return currentIds.GetAndAssertUniqueValue();
        }

        public static Report ReadDefaultObject_ForDesiredRevisionAndContainingType(this IReadOnlyRepository<ReportId, Report> repository, int modelTemplateNumber, Nullable<StructuralTypeOption> containing_StructuralTypeOption, int containing_StructuralTypeNumber, RevisionChain revisionChain)
        {
            var currentRows = ReadDefaultIdRow_ForDesiredRevisionAndContainingType(repository, modelTemplateNumber, containing_StructuralTypeOption, containing_StructuralTypeNumber, revisionChain);
            var currentObjects = DoAggregateRead(repository, currentRows);
            return currentObjects.FirstOrDefault();
        }

        #endregion

        #region Queries to Read for Specific Revision ONLY

        public static ICollection<Report> ReadForSpecificRevisionId(this IReadOnlyRepository<ReportId, Report> repository, RevisionId revisionId)
        {
            LinqQuery<Report> query = new LinqQuery<Report>((Report dObj) => ((dObj.EF_ProjectGuid == revisionId.ProjectGuid) && (dObj.EF_RevisionNumber == revisionId.RevisionNumber_NonNull)));
            ICollection<Report> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ReportId, Report> ReadDictionaryForSpecificRevisionId(this IReadOnlyRepository<ReportId, Report> repository, RevisionId revisionId)
        {
            ICollection<Report> list = ReadForSpecificRevisionId(repository, revisionId);
            IDictionary<ReportId, Report> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Creator

        public static ICollection<Report> ReadForCreatorId(this IReadOnlyRepository<ReportId, Report> repository, UserId creatorId)
        {
            LinqQuery<Report> query = new LinqQuery<Report>((Report dObj) => (dObj.EF_CreatorGuid == creatorId.UserGuid));
            ICollection<Report> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ReportId, Report> ReadDictionaryForCreatorId(this IReadOnlyRepository<ReportId, Report> repository, UserId creatorId)
        {
            ICollection<Report> list = ReadForCreatorId(repository, creatorId);
            IDictionary<ReportId, Report> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Queries to Read by Owner

        public static ICollection<Report> ReadForOwnerId(this IReadOnlyRepository<ReportId, Report> repository, ISiteActor siteActorRef)
        {
            LinqQuery<Report> query = new LinqQuery<Report>((Report dObj) => ((dObj.EF_OwnerType == (int)siteActorRef.ActorType) && (dObj.EF_OwnerGuid == siteActorRef.ActorGuid)));
            ICollection<Report> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ReportId, Report> ReadDictionaryForOwnerId(this IReadOnlyRepository<ReportId, Report> repository, ISiteActor siteActorRef)
        {
            ICollection<Report> list = ReadForOwnerId(repository, siteActorRef);
            IDictionary<ReportId, Report> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion

        #region Helper Methods for Reading Entire Aggregate

        private static ICollection<Report> DoAggregateRead(this IReadOnlyRepository<ReportId, Report> repository, IQueryable<ReportIdRow> currentRows)
        {
            var versionedObjects_Main = repository.GetActualDataStore();
            var versionedObjects_Nested_ElementStyle = repository.GetNestedDataStore_ElementStyles();
            var versionedObjects_Nested_DimensionLayout = repository.GetNestedDataStore_DimensionLayouts();

            var requiredElementNumbers = Report.Report_AllElementNumbers.ToList();

            var currentObjects_Main = (from cRow in currentRows
                                       join vObj in versionedObjects_Main on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_ModelTemplateNumber, cRow.EF_ReportGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid }
                                       select vObj).ToList();
            var currentObjects_Nested_ElementStyle = (from cRow in currentRows
                                                      join vObj in versionedObjects_Nested_ElementStyle on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_ModelTemplateNumber, cRow.EF_ReportGuid } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid }
                                                      where requiredElementNumbers.Contains(vObj.EF_ReportElementNumber)
                                                      select vObj).ToList();
            var currentObjects_Nested_DimensionLayout = (from cRow in currentRows
                                                         join vObj in versionedObjects_Nested_DimensionLayout on new { cRow.EF_ProjectGuid, cRow.EF_RevisionNumber, cRow.EF_ModelTemplateNumber, cRow.EF_ReportGuid, EF_ReportElementNumber = Report.Report_ReportElementNumber } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_ReportElementNumber }
                                                         select vObj).ToList();

            var currentObjects_Nested_ElementStyle_Dict = new Dictionary<ReportId, List<SaveableElementStyle>>();
            foreach (var currentObject_Nested_ElementStyle in currentObjects_Nested_ElementStyle)
            {
                var key = new ReportId(currentObject_Nested_ElementStyle.EF_ProjectGuid, currentObject_Nested_ElementStyle.EF_RevisionNumber, currentObject_Nested_ElementStyle.EF_ModelTemplateNumber, currentObject_Nested_ElementStyle.EF_ReportGuid);
                if (!currentObjects_Nested_ElementStyle_Dict.ContainsKey(key))
                { currentObjects_Nested_ElementStyle_Dict.Add(key, new List<SaveableElementStyle>()); }
                currentObjects_Nested_ElementStyle_Dict[key].Add(currentObject_Nested_ElementStyle);
            }

            var currentObjects_Nested_DimensionLayout_Dict = new Dictionary<ReportId, List<SaveableDimensionLayout>>();
            foreach (var currentObject_Nested_DimensionLayout in currentObjects_Nested_DimensionLayout)
            {
                var key = new ReportId(currentObject_Nested_DimensionLayout.EF_ProjectGuid, currentObject_Nested_DimensionLayout.EF_RevisionNumber, currentObject_Nested_DimensionLayout.EF_ModelTemplateNumber, currentObject_Nested_DimensionLayout.EF_ReportGuid);
                if (!currentObjects_Nested_DimensionLayout_Dict.ContainsKey(key))
                { currentObjects_Nested_DimensionLayout_Dict.Add(key, new List<SaveableDimensionLayout>()); }
                currentObjects_Nested_DimensionLayout_Dict[key].Add(currentObject_Nested_DimensionLayout);
            }

            var batchReadState = new Dictionary<string, object>();
            batchReadState.Add(Report.TypeName_Report, currentObjects_Main);
            batchReadState.Add(Report.TypeName_ElementStyle, currentObjects_Nested_ElementStyle_Dict);
            batchReadState.Add(Report.TypeName_DimensionLayout, currentObjects_Nested_DimensionLayout_Dict);

            foreach (var currentObject_Main in currentObjects_Main)
            {
                var currentObject_Main_Aggr = (currentObject_Main as IEfAggregate<Report>);
                currentObject_Main_Aggr.ReadNestedAggregateValues(null, null, batchReadState);
            }
            return currentObjects_Main;
        }

        #endregion
    }
}