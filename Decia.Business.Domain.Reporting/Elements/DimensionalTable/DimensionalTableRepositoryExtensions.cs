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
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Permissions;
using Decia.Business.Domain;
using Decia.Business.Domain.Projects;

namespace Decia.Business.Domain.Reporting
{
    public static class DimensionalTableRepositoryExtensions
    {
        public static DimensionalTable_ObjectSet ReadCompleteObjectSet(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, int dimensionalTableNumber, RevisionChain revisionChain)
        {
            var dimensionalTableId_Nullable = domainModel.DimensionalTables().ReadCurrentKey_ForDesiredRevision(modelTemplateNumber, reportGuid, dimensionalTableNumber, revisionChain);
            var dimensionalTableId = dimensionalTableId_Nullable.Value;
            var dimensionalTable = domainModel.DimensionalTables().Read(dimensionalTableId);

            var stackingDimension = dimensionalTable.StackingDimension;

            var tableHeaderIds = domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, dimensionalTableId.ReportElementNumber, revisionChain);
            var tableHeaderId = tableHeaderIds.First();
            var tableHeader = domainModel.TableHeaders().Read(tableHeaderId);

            var rowHeaderIds = domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, dimensionalTableId.ReportElementNumber, revisionChain);
            var rowHeaderId = rowHeaderIds.First();
            var rowHeader = domainModel.RowHeaders().Read(rowHeaderId);

            var columnHeaderIds = domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, dimensionalTableId.ReportElementNumber, revisionChain);
            var columnHeaderId = columnHeaderIds.First();
            var columnHeader = domainModel.ColumnHeaders().Read(columnHeaderId);

            var dataAreaIds = domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, dimensionalTableId.ReportElementNumber, revisionChain);
            var dataAreaId = dataAreaIds.First();
            var dataArea = domainModel.DataAreas().Read(dataAreaId);

            var commonTitleContainerParentId = (stackingDimension == Dimension.X) ? rowHeaderId : columnHeaderId;
            var variableTitleContainerParentId = (stackingDimension == Dimension.X) ? columnHeaderId : rowHeaderId;
            var variableDataContainerParentId = dataAreaId;

            var commonTitleContainerIds = domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, commonTitleContainerParentId.ReportElementNumber, revisionChain);
            var commonTitleContainerId = commonTitleContainerIds.First();
            var commonTitleContainer = domainModel.CommonTitleContainers().Read(commonTitleContainerId);

            var variableTitleContainerIds = domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, variableTitleContainerParentId.ReportElementNumber, revisionChain);
            var variableTitleContainerId = variableTitleContainerIds.First();
            var variableTitleContainer = domainModel.VariableTitleContainers().Read(variableTitleContainerId);

            var variableDataContainerIds = domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, variableDataContainerParentId.ReportElementNumber, revisionChain);
            var variableDataContainerId = variableDataContainerIds.First();
            var variableDataContainer = domainModel.VariableDataContainers().Read(variableDataContainerId);

            var commonTitleBoxIds = domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, commonTitleContainerId.ReportElementNumber, revisionChain);
            var commonTitleBoxes = domainModel.CommonTitleBoxes().ReadDictionary(commonTitleBoxIds);

            var variableTitleBoxIds = domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, variableTitleContainerId.ReportElementNumber, revisionChain);
            var variableTitleBoxes = domainModel.VariableTitleBoxes().ReadDictionary(variableTitleBoxIds);

            var variableDataBoxIds = domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, variableDataContainerId.ReportElementNumber, revisionChain);
            var variableDataBoxes = domainModel.VariableDataBoxes().ReadDictionary(variableDataBoxIds);

            var allTitleBoxNumbers = new List<int?>();
            allTitleBoxNumbers.AddRange(commonTitleBoxIds.Select(x => (int?)x.ReportElementNumber).ToList());
            allTitleBoxNumbers.AddRange(variableTitleBoxIds.Select(x => (int?)x.ReportElementNumber).ToList());

            var allDataBoxNumbers = new List<int?>();
            allDataBoxNumbers.AddRange(variableDataBoxIds.Select(x => (int?)x.ReportElementNumber).ToList());

            var structuralTitleRangeIds = domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, allTitleBoxNumbers, revisionChain);
            var structuralTitleRanges = domainModel.StructuralTitleRanges().ReadDictionary(structuralTitleRangeIds);

            var timeTitleRangeIds = domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, allTitleBoxNumbers, revisionChain);
            var timeTitleRanges = domainModel.TimeTitleRanges().ReadDictionary(timeTitleRangeIds);

            var variableTitleRangeIds = domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, allTitleBoxNumbers, revisionChain);
            var variableTitleRanges = domainModel.VariableTitleRanges().ReadDictionary(variableTitleRangeIds);

            var variableDataRangeIds = domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, allDataBoxNumbers, revisionChain);
            var variableDataRanges = domainModel.VariableDataRanges().ReadDictionary(variableDataRangeIds);

            var result = new DimensionalTable_ObjectSet();
            result.DimensionalTable = dimensionalTable;
            result.TableHeader = tableHeader;
            result.RowHeader = rowHeader;
            result.ColumnHeader = columnHeader;
            result.DataArea = dataArea;
            result.CommonTitleContainer = commonTitleContainer;
            result.VariableTitleContainer = variableTitleContainer;
            result.VariableDataContainer = variableDataContainer;
            result.CommonTitleBoxes.AddRange(commonTitleBoxes);
            result.VariableTitleBoxes.AddRange(variableTitleBoxes);
            result.VariableDataBoxes.AddRange(variableDataBoxes);
            result.StructuralTitleRanges.AddRange(structuralTitleRanges);
            result.TimeTitleRanges.AddRange(timeTitleRanges);
            result.VariableTitleRanges.AddRange(variableTitleRanges);
            result.VariableDataRanges.AddRange(variableDataRanges);

            return result;
        }
    }
}