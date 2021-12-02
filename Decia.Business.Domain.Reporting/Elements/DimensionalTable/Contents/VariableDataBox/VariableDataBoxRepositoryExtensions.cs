using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Domain.Reporting.Internals;

namespace Decia.Business.Domain.Reporting
{
    public static class VariableDataBoxRepositoryExtensions
    {
        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevisionAndVariableTitleBox(this IReadOnlyRepository<ReportElementId, VariableDataBox> repository, int modelTemplateNumber, Guid reportGuid, int relatedVariableTitleBoxNumber, RevisionChain revisionChain)
        {
            var relatedVariableTitleBoxNumbers = new int[] { relatedVariableTitleBoxNumber };
            return ReadCurrentKeys_ForDesiredRevisionAndVariableTitleBoxes(repository, modelTemplateNumber, reportGuid, relatedVariableTitleBoxNumbers, revisionChain);
        }

        public static ICollection<ReportElementId> ReadCurrentKeys_ForDesiredRevisionAndVariableTitleBoxes(this IReadOnlyRepository<ReportElementId, VariableDataBox> repository, int modelTemplateNumber, Guid reportGuid, IEnumerable<int> relatedVariableTitleBoxNumbers, RevisionChain revisionChain)
        {
            var possibleRows = repository.ReadCurrentIdRows_ForDesiredRevision(revisionChain);
            var versionedObjects = repository.GetActualDataStore();
            var relatedVariableTitleBoxNumbers_Nullable = relatedVariableTitleBoxNumbers.Select(x => (int?)x).ToHashSet();

            var currentRows = (from pRow in possibleRows
                               join vObj in versionedObjects on new { pRow.EF_ProjectGuid, pRow.EF_RevisionNumber, pRow.EF_ModelTemplateNumber, pRow.EF_ReportGuid, pRow.EF_ReportElementNumber } equals new { vObj.EF_ProjectGuid, vObj.EF_RevisionNumber, vObj.EF_ModelTemplateNumber, vObj.EF_ReportGuid, vObj.EF_ReportElementNumber }
                               where relatedVariableTitleBoxNumbers_Nullable.Contains(vObj.EF_RelatedVariableTitleBoxNumber)
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
    }
}