using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Queries;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Exports;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Exports
{
    public class ExportHistoryFilter
    {
        public UserId? ExporterId { get; set; }
        public ProjectId? ProjectId { get; set; }
        public KeyValuePair<long, long>? RevisionRange { get; set; }
        public KeyValuePair<DateTime, DateTime>? DateRange { get; set; }
        public TargetTypeCategory? TargetTypeCategory { get; set; }
        public int? TargetTypeValue { get; set; }
    }

    public static class ExportHistoryItemRepositoryExtensions
    {
        #region Queries to Read for Filter Values

        public static ICollection<ExportHistoryItem> ReadForFilterValues(this IReadOnlyRepository<ExportHistoryItemId, ExportHistoryItem> repository, ExportHistoryFilter exportHistoryFilter)
        {
            return ReadForFilterValues(repository, exportHistoryFilter.ExporterId, exportHistoryFilter.ProjectId, exportHistoryFilter.RevisionRange, exportHistoryFilter.DateRange, exportHistoryFilter.TargetTypeCategory, exportHistoryFilter.TargetTypeValue);
        }

        public static ICollection<ExportHistoryItem> ReadForFilterValues(this IReadOnlyRepository<ExportHistoryItemId, ExportHistoryItem> repository, UserId? exporterId, ProjectId? projectId, KeyValuePair<long, long>? revisionRange, KeyValuePair<DateTime, DateTime>? dateRange, TargetTypeCategory? targetTypeCategory, int? targetTypeValue)
        {
            return ReadForFilterValues(repository, exporterId.ConvertToNullableGuid(), projectId.ConvertToNullableGuid(), revisionRange.HasValue ? revisionRange.Value.Key : (long?)null, revisionRange.HasValue ? revisionRange.Value.Value : (long?)null, dateRange.HasValue ? dateRange.Value.Key : (DateTime?)null, dateRange.HasValue ? dateRange.Value.Value : (DateTime?)null, targetTypeCategory.GetAsInt(), targetTypeValue);
        }

        private static ICollection<ExportHistoryItem> ReadForFilterValues(this IReadOnlyRepository<ExportHistoryItemId, ExportHistoryItem> repository, Guid? exporterGuid, Guid? projectGuid, long? revisionRange_Min, long? revisionRange_Max, DateTime? dateRange_Min, DateTime? dateRange_Max, int? targetTypeCategory, int? targetTypeValue)
        {
            LinqQuery<ExportHistoryItem> query = new LinqQuery<ExportHistoryItem>((ExportHistoryItem dObj) =>
                    (!exporterGuid.HasValue || (exporterGuid.HasValue && (dObj.EF_ExporterGuid == exporterGuid))) &&
                    (!projectGuid.HasValue || (projectGuid.HasValue && (dObj.EF_ProjectGuid == projectGuid))) &&
                    ((!revisionRange_Min.HasValue || !revisionRange_Max.HasValue) || ((revisionRange_Min.HasValue && revisionRange_Max.HasValue) && ((revisionRange_Min <= dObj.EF_RevisionNumber) && (dObj.EF_RevisionNumber <= revisionRange_Max)))) &&
                    ((!dateRange_Min.HasValue || !dateRange_Max.HasValue) || ((dateRange_Min.HasValue && dateRange_Max.HasValue) && ((dateRange_Min <= dObj.EF_DateOfExport) && (dObj.EF_DateOfExport <= dateRange_Max)))) &&
                    (!targetTypeCategory.HasValue || (targetTypeCategory.HasValue && (dObj.EF_TargetTypeCategory == targetTypeCategory))) &&
                    (!targetTypeValue.HasValue || (targetTypeValue.HasValue && (dObj.EF_TargetTypeValue == targetTypeValue)))
                );
            ICollection<ExportHistoryItem> result = repository.Read(query);
            return result;
        }

        public static IDictionary<ExportHistoryItemId, ExportHistoryItem> ReadDictionaryForFilterValues(this IReadOnlyRepository<ExportHistoryItemId, ExportHistoryItem> repository, ExportHistoryFilter exportHistoryFilter)
        {
            return ReadDictionaryForFilterValues(repository, exportHistoryFilter.ExporterId, exportHistoryFilter.ProjectId, exportHistoryFilter.RevisionRange, exportHistoryFilter.DateRange, exportHistoryFilter.TargetTypeCategory, exportHistoryFilter.TargetTypeValue);
        }

        public static IDictionary<ExportHistoryItemId, ExportHistoryItem> ReadDictionaryForFilterValues(this IReadOnlyRepository<ExportHistoryItemId, ExportHistoryItem> repository, UserId? exporterId, ProjectId? projectId, KeyValuePair<long, long>? revisionRange, KeyValuePair<DateTime, DateTime>? dateRange, TargetTypeCategory? targetTypeCategory, int? targetTypeValue)
        {
            ICollection<ExportHistoryItem> list = ReadForFilterValues(repository, exporterId, projectId, revisionRange, dateRange, targetTypeCategory, targetTypeValue);
            IDictionary<ExportHistoryItemId, ExportHistoryItem> dictionary = repository.ConvertToDictionary(list);
            return dictionary;
        }

        #endregion
    }
}