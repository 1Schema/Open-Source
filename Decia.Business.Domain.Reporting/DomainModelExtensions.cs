using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Internals;

namespace Decia.Business.Domain.Reporting
{
    public static class DomainModelExtensions
    {
        #region Methods - DomainModel Initializers

        public static void Decia_Reporting_LoadData(this IDomainModel domainModel)
        {
            // do nothing
        }

        public static void Decia_Reporting_LoadConstraints(this IDomainModel domainModel)
        {
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportId, Report>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, Cell>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, Container>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, DimensionalTable>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, ColumnHeader>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, CommonTitleContainer>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, DataArea>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, RowHeader>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, TableHeader>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, VariableDataContainer>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, VariableTitleContainer>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, CommonTitleBox>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, StructuralTitleRange>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, TimeTitleRange>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, VariableDataBox>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, VariableDataRange>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, VariableTitleBox>(domainModel));
            domainModel.AddModelConstraint(new ModelDomainObject_ManagementConstraint<ReportElementId, VariableTitleRange>(domainModel));
        }

        #endregion

        #region Methods - Repository Getters

        public static IRepository<ReportId, Report> Reports(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportId, Report>();
        }

        public static IRepository<ReportElementId, Cell> Cells(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, Cell>();
        }

        public static IRepository<ReportElementId, Container> Containers(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, Container>();
        }

        public static IRepository<ReportElementId, DimensionalTable> DimensionalTables(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, DimensionalTable>();
        }

        public static IRepository<ReportElementId, ColumnHeader> ColumnHeaders(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, ColumnHeader>();
        }

        public static IRepository<ReportElementId, CommonTitleContainer> CommonTitleContainers(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, CommonTitleContainer>();
        }

        public static IRepository<ReportElementId, DataArea> DataAreas(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, DataArea>();
        }

        public static IRepository<ReportElementId, RowHeader> RowHeaders(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, RowHeader>();
        }

        public static IRepository<ReportElementId, TableHeader> TableHeaders(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, TableHeader>();
        }

        public static IRepository<ReportElementId, VariableDataContainer> VariableDataContainers(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, VariableDataContainer>();
        }

        public static IRepository<ReportElementId, VariableTitleContainer> VariableTitleContainers(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, VariableTitleContainer>();
        }

        public static IRepository<ReportElementId, CommonTitleBox> CommonTitleBoxes(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, CommonTitleBox>();
        }

        public static IRepository<ReportElementId, StructuralTitleRange> StructuralTitleRanges(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, StructuralTitleRange>();
        }

        public static IRepository<ReportElementId, TimeTitleRange> TimeTitleRanges(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, TimeTitleRange>();
        }

        public static IRepository<ReportElementId, VariableDataBox> VariableDataBoxes(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, VariableDataBox>();
        }

        public static IRepository<ReportElementId, VariableDataRange> VariableDataRanges(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, VariableDataRange>();
        }

        public static IRepository<ReportElementId, VariableTitleBox> VariableTitleBoxes(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, VariableTitleBox>();
        }

        public static IRepository<ReportElementId, VariableTitleRange> VariableTitleRanges(this IDomainModel domainModel)
        {
            return domainModel.GetRepository<ReportElementId, VariableTitleRange>();
        }

        #endregion

        #region Methods - Read Elements & Formulas For Report

        public static void ReadElementsAndFormulas_ForReport(this IDomainModel domainModel, RevisionChain revisionChain, int modelTemplateNumber, Guid reportGuid, out List<IReportElement> elements, out List<Formula> formulas)
        {
            var elementsList = new List<IReportElement>();
            var formulaGuidsList = new List<Guid>();

            ReadElementsAndFormulas_ForReport(domainModel, revisionChain, modelTemplateNumber, reportGuid, out elementsList, out formulaGuidsList);
            var formulasList = domainModel.Formulas().ReadCurrentObjects_ForDesiredRevision(formulaGuidsList, revisionChain).ToList();

            elements = elementsList;
            formulas = formulasList;
        }

        public static void ReadElementsAndFormulas_ForReport(this IDomainModel domainModel, RevisionChain revisionChain, int modelTemplateNumber, Guid reportGuid, out List<IReportElement> elements, out List<Guid> formulaGuids)
        {
            var elementsList = new List<IReportElement>();
            var formulaGuidsList = new List<Guid>();

            var cellIds = domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var cells = domainModel.Cells().Read(cellIds).ToList();
            elementsList.AddRange(cells.Select(x => (x as IReportElement)));
            cells.ForEach(x => formulaGuidsList.AddRange((x as IFormulaHost).ContainedFormulaGuids));

            var containerIds = domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var containers = domainModel.Containers().Read(containerIds);
            elementsList.AddRange(containers.Select(x => (x as IReportElement)));

            var dimensionalTableIds = domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var dimensionalTables = domainModel.DimensionalTables().Read(dimensionalTableIds);
            elementsList.AddRange(dimensionalTables.Select(x => (x as IReportElement)));

            var columnHeaderIds = domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var columnHeaders = domainModel.ColumnHeaders().Read(columnHeaderIds);
            elementsList.AddRange(columnHeaders.Select(x => (x as IReportElement)));

            var commonTitleContainerIds = domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var commonTitleContainers = domainModel.CommonTitleContainers().Read(commonTitleContainerIds);
            elementsList.AddRange(commonTitleContainers.Select(x => (x as IReportElement)));

            var dataAreaIds = domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var dataAreas = domainModel.DataAreas().Read(dataAreaIds);
            elementsList.AddRange(dataAreas.Select(x => (x as IReportElement)));

            var rowHeaderIds = domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var rowHeaders = domainModel.RowHeaders().Read(rowHeaderIds);
            elementsList.AddRange(rowHeaders.Select(x => (x as IReportElement)));

            var tableHeaderIds = domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var tableHeaders = domainModel.TableHeaders().Read(tableHeaderIds);
            elementsList.AddRange(tableHeaders.Select(x => (x as IReportElement)));

            var variableDataContainerIds = domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var variableDataContainers = domainModel.VariableDataContainers().Read(variableDataContainerIds);
            elementsList.AddRange(variableDataContainers.Select(x => (x as IReportElement)));

            var variableTitleContainerIds = domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var variableTitleContainers = domainModel.VariableTitleContainers().Read(variableTitleContainerIds);
            elementsList.AddRange(variableTitleContainers.Select(x => (x as IReportElement)));

            var commonTitleBoxIds = domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var commonTitleBoxes = domainModel.CommonTitleBoxes().Read(commonTitleBoxIds);
            elementsList.AddRange(commonTitleBoxes.Select(x => (x as IReportElement)));

            var structuralTitleRangeIds = domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var structuralTitleRanges = domainModel.StructuralTitleRanges().Read(structuralTitleRangeIds).ToList();
            elementsList.AddRange(structuralTitleRanges.Select(x => (x as IReportElement)));
            structuralTitleRanges.ForEach(x => formulaGuidsList.AddRange((x as IFormulaHost).ContainedFormulaGuids));

            var timeTitleRangeIds = domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var timeTitleRanges = domainModel.TimeTitleRanges().Read(timeTitleRangeIds).ToList();
            elementsList.AddRange(timeTitleRanges.Select(x => (x as IReportElement)));
            timeTitleRanges.ForEach(x => formulaGuidsList.AddRange((x as IFormulaHost).ContainedFormulaGuids));

            var variableDataBoxIds = domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var variableDataBoxes = domainModel.VariableDataBoxes().Read(variableDataBoxIds);
            elementsList.AddRange(variableDataBoxes.Select(x => (x as IReportElement)));

            var variableDataRangeIds = domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var variableDataRanges = domainModel.VariableDataRanges().Read(variableDataRangeIds).ToList();
            elementsList.AddRange(variableDataRanges.Select(x => (x as IReportElement)));
            variableDataRanges.ForEach(x => formulaGuidsList.AddRange((x as IFormulaHost).ContainedFormulaGuids));

            var variableTitleBoxIds = domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var variableTitleBoxes = domainModel.VariableTitleBoxes().Read(variableTitleBoxIds);
            elementsList.AddRange(variableTitleBoxes.Select(x => (x as IReportElement)));

            var variableTitleRangeIds = domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain);
            var variableTitleRanges = domainModel.VariableTitleRanges().Read(variableTitleRangeIds).ToList();
            elementsList.AddRange(variableTitleRanges.Select(x => (x as IReportElement)));
            variableTitleRanges.ForEach(x => formulaGuidsList.AddRange((x as IFormulaHost).ContainedFormulaGuids));

            elements = elementsList;
            formulaGuids = formulaGuidsList;
        }

        #endregion

        #region Queries to Read by Desired (Project, Revision)

        public static bool Elements_HaveChanges_ForRevisionRange(this IDomainModel domainModel, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            var changeCount = Elements_GetChangeCount_ForRevisionRange(domainModel, minRevisionNumber, maxRevisionNumber, revisionChain);
            return (changeCount > 0);
        }

        public static int Elements_GetChangeCount_ForRevisionRange(this IDomainModel domainModel, long minRevisionNumber, long maxRevisionNumber, RevisionChain revisionChain)
        {
            revisionChain.AssertRevisionRangeIsValid(minRevisionNumber, maxRevisionNumber);
            revisionChain.AssertRevisionIsAllowed(maxRevisionNumber);
            int changeCount = 0;

            changeCount += domainModel.Cells().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.ColumnHeaders().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.CommonTitleBoxes().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.CommonTitleContainers().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.Containers().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.DataAreas().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.DimensionalTables().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.RowHeaders().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.StructuralTitleRanges().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.TableHeaders().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.TimeTitleRanges().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.VariableDataBoxes().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.VariableDataContainers().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.VariableDataRanges().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.VariableTitleBoxes().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.VariableTitleContainers().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);
            changeCount += domainModel.VariableTitleRanges().GetChangeCount_ForRevisionRange(minRevisionNumber, maxRevisionNumber, revisionChain);

            return changeCount;
        }

        public static ICollection<ReportElementId> Elements_ReadCurrentKeys_ForDesiredRevision(this IDomainModel domainModel, RevisionChain revisionChain)
        {
            var currentKeys = new List<ReportElementId>();
            currentKeys.AddRange(domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(revisionChain));
            return currentKeys;
        }

        public static ICollection<ReportElementId> Elements_ReadCurrentKeys_ForDesiredRevision(this IDomainModel domainModel, int modelTemplateNumber, RevisionChain revisionChain)
        {
            var currentKeys = new List<ReportElementId>();
            currentKeys.AddRange(domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, revisionChain));
            return currentKeys;
        }

        public static ICollection<ReportElementId> Elements_ReadCurrentKeys_ForDesiredRevision(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, RevisionChain revisionChain)
        {
            var currentKeys = new List<ReportElementId>();
            currentKeys.AddRange(domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, revisionChain));
            return currentKeys;
        }

        public static ICollection<ReportElementId> Elements_ReadCurrentKeys_ForDesiredRevision(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, int? parentElementNumber, RevisionChain revisionChain)
        {
            var currentKeys = new List<ReportElementId>();
            currentKeys.AddRange(domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumber, revisionChain));
            return currentKeys;
        }

        public static ICollection<ReportElementId> Elements_ReadCurrentKeys_ForDesiredRevision(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, ICollection<int?> parentElementNumbers, RevisionChain revisionChain)
        {
            var currentKeys = new List<ReportElementId>();
            currentKeys.AddRange(domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            currentKeys.AddRange(domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, parentElementNumbers, revisionChain));
            return currentKeys;
        }

        public static ICollection<ReportElementId> Elements_ReadCurrentKeys_ForDesiredRevision(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, IEnumerable<int> elementNumbers, RevisionChain revisionChain)
        {
            var currentKeys = new List<ReportElementId>();
            currentKeys.AddRange(domainModel.Cells().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.ColumnHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.CommonTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.CommonTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.Containers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.DataAreas().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.DimensionalTables().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.RowHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.StructuralTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.TableHeaders().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.TimeTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.VariableDataBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.VariableDataContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.VariableDataRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.VariableTitleBoxes().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.VariableTitleContainers().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));
            currentKeys.AddRange(domainModel.VariableTitleRanges().ReadCurrentKeys_ForDesiredRevision(modelTemplateNumber, reportGuid, elementNumbers, revisionChain, false));

            var originalCount = currentKeys.Count;
            var distinctKeys = currentKeys.ToHashSet();
            var distinctCount = distinctKeys.Count;

            if (originalCount != distinctCount)
            { throw new InvalidOperationException("The Report Element Numbers all must be unique for a given Report."); }

            return distinctKeys;
        }

        public static Nullable<ReportElementId> Elements_ReadCurrentKey_ForDesiredRevision(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, int elementNumber, RevisionChain revisionChain)
        {
            var elementNumbers = new int[] { elementNumber };
            var currentKeys = Elements_ReadCurrentKeys_ForDesiredRevision(domainModel, modelTemplateNumber, reportGuid, elementNumbers, revisionChain);

            if (currentKeys.Count > 1)
            { throw new InvalidOperationException("The Report Element Numbers all must be unique for a given Report."); }
            if (currentKeys.Count < 1)
            { return null; }
            return currentKeys.First();
        }

        public static ICollection<IReportElement> Elements_Read(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, IEnumerable<int> elementNumbers, RevisionChain revisionChain)
        {
            var currentKeys = Elements_ReadCurrentKeys_ForDesiredRevision(domainModel, modelTemplateNumber, reportGuid, elementNumbers, revisionChain);
            return Elements_Read(domainModel, currentKeys);
        }

        public static ICollection<IReportElement> Elements_Read(this IDomainModel domainModel, IEnumerable<ReportElementId> reportElementIds)
        {
            var remainingElementIds = reportElementIds.ToHashSet();
            var results = new List<IReportElement>();

            domainModel.Cells().ReadIfExists(ref remainingElementIds, results);
            domainModel.ColumnHeaders().ReadIfExists(ref remainingElementIds, results);
            domainModel.CommonTitleBoxes().ReadIfExists(ref remainingElementIds, results);
            domainModel.CommonTitleContainers().ReadIfExists(ref remainingElementIds, results);
            domainModel.Containers().ReadIfExists(ref remainingElementIds, results);
            domainModel.DataAreas().ReadIfExists(ref remainingElementIds, results);
            domainModel.DimensionalTables().ReadIfExists(ref remainingElementIds, results);
            domainModel.RowHeaders().ReadIfExists(ref remainingElementIds, results);
            domainModel.StructuralTitleRanges().ReadIfExists(ref remainingElementIds, results);
            domainModel.TableHeaders().ReadIfExists(ref remainingElementIds, results);
            domainModel.TimeTitleRanges().ReadIfExists(ref remainingElementIds, results);
            domainModel.VariableDataBoxes().ReadIfExists(ref remainingElementIds, results);
            domainModel.VariableDataContainers().ReadIfExists(ref remainingElementIds, results);
            domainModel.VariableDataRanges().ReadIfExists(ref remainingElementIds, results);
            domainModel.VariableTitleBoxes().ReadIfExists(ref remainingElementIds, results);
            domainModel.VariableTitleContainers().ReadIfExists(ref remainingElementIds, results);
            domainModel.VariableTitleRanges().ReadIfExists(ref remainingElementIds, results);

            return results;
        }

        public static IReportElement Elements_Read(this IDomainModel domainModel, int modelTemplateNumber, Guid reportGuid, int elementNumber, RevisionChain revisionChain)
        {
            var elementNumbers = new int[] { elementNumber };
            var elements = Elements_Read(domainModel, modelTemplateNumber, reportGuid, elementNumbers, revisionChain);

            if (elements.Count > 1)
            { throw new InvalidOperationException("The Report Element Numbers all must be unique for a given Report."); }
            if (elements.Count < 1)
            { return null; }
            return elements.First();
        }

        public static IReportElement Elements_Read(this IDomainModel domainModel, ReportElementId reportElementId)
        {
            var reportElementIds = new ReportElementId[] { reportElementId };
            var reportElements = Elements_Read(domainModel, reportElementIds);
            return reportElements.FirstOrDefault();
        }

        #endregion
    }
}