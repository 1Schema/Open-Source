using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.Layouts;
using Decia.Business.Common.Presentation;
using Decia.Business.Domain.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class DimensionalTable : ReportElementBase<DimensionalTable>, IDimensionalTable
    {
        protected bool m_IsTransposed;

        protected Nullable<int> m_TableHeaderNumber;
        protected Nullable<int> m_ColumnHeaderNumber;
        protected Nullable<int> m_RowHeaderNumber;
        protected Nullable<int> m_DataAreaNumber;
        protected Nullable<int> m_CommonTitleContainerNumber;
        protected Nullable<int> m_VariableTitleContainerNumber;
        protected Nullable<int> m_VariableDataContainerNumber;

        public DimensionalTable()
            : this(ReportId.DefaultId)
        { }

        public DimensionalTable(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public DimensionalTable(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_IsTransposed = ITransposableElementUtils.Default_IsTransposed;

            m_TableHeaderNumber = null;
            m_ColumnHeaderNumber = null;
            m_RowHeaderNumber = null;
            m_DataAreaNumber = null;
            m_CommonTitleContainerNumber = null;
            m_VariableTitleContainerNumber = null;
            m_VariableDataContainerNumber = null;
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable;
        }

        protected override IEditabilitySpecification GetEditabilitySpecForLayout(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return EditabilitySpec; }
            else if (dimension == Dimension.Y)
            { return EditabilitySpec; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        protected override IEditabilitySpecification GetEditabilitySpecForStyle()
        {
            return null;
        }

        protected override IDimensionLayout GetDefaultLayout(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return DefaultLayout_X; }
            else if (dimension == Dimension.Y)
            { return DefaultLayout_Y; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        protected override IElementStyle GetDefaultStyle()
        {
            return DefaultStyle;
        }

        #endregion

        #region IDimensionalTable Implementation

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var children = new List<ReportElementId>();

                if (m_TableHeaderNumber.HasValue)
                { children.Add(TableHeaderId); }
                if (m_ColumnHeaderNumber.HasValue)
                { children.Add(ColumnHeaderId); }
                if (m_RowHeaderNumber.HasValue)
                { children.Add(RowHeaderId); }
                if (m_DataAreaNumber.HasValue)
                { children.Add(DataAreaId); }

                return children;
            }
        }

        [NotMapped]
        public ICollection<int> ChildNumbers
        {
            get { return ChildIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        [NotMapped]
        public Dimension StackingDimension
        {
            get { return this.IsTransposed.GetStackingDimension(); }
        }

        [NotMapped]
        public Dimension CommonDimension
        {
            get { return this.IsTransposed.GetCommonDimension(); }
        }

        [NotMapped]
        [PropertyDisplayData("Is Transposed", "Is the Dimensional Table transposed?", false, EditorType.Input, OrderNumber_NonNull = 9, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public bool IsTransposed
        {
            get { return m_IsTransposed; }
        }

        [NotMapped]
        public ReportElementId TableHeaderId
        {
            get { return new ReportElementId(this.Key.ReportId, m_TableHeaderNumber.Value); }
        }

        [NotMapped]
        public int TableHeaderNumber
        {
            get { return TableHeaderId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId ColumnHeaderId
        {
            get { return new ReportElementId(this.Key.ReportId, m_ColumnHeaderNumber.Value); }
        }

        [NotMapped]
        public int ColumnHeaderNumber
        {
            get { return ColumnHeaderId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId RowHeaderId
        {
            get { return new ReportElementId(this.Key.ReportId, m_RowHeaderNumber.Value); }
        }

        [NotMapped]
        public int RowHeaderNumber
        {
            get { return RowHeaderId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId DataAreaId
        {
            get { return new ReportElementId(this.Key.ReportId, m_DataAreaNumber.Value); }
        }

        [NotMapped]
        public int DataAreaNumber
        {
            get { return DataAreaId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId CommonTitleContainerId
        {
            get { return new ReportElementId(this.Key.ReportId, m_CommonTitleContainerNumber.Value); }
        }

        [NotMapped]
        public int CommonTitleContainerNumber
        {
            get { return CommonTitleContainerId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId VariableTitleContainerId
        {
            get { return new ReportElementId(this.Key.ReportId, m_VariableTitleContainerNumber.Value); }
        }

        [NotMapped]
        public int VariableTitleContainerNumber
        {
            get { return VariableTitleContainerId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId VariableDataContainerId
        {
            get { return new ReportElementId(this.Key.ReportId, m_VariableDataContainerNumber.Value); }
        }

        [NotMapped]
        public int VariableDataContainerNumber
        {
            get { return VariableDataContainerId.ReportElementNumber; }
        }

        public void SetContainedElements(ITableHeader tableHeader, IColumnHeader columnHeader, IRowHeader rowHeader, IDataArea dataArea, ICommonTitleContainer commonTitleContainer, IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer)
        {
            var typedColumnHeader = (columnHeader as ColumnHeader);
            var typedRowHeader = (rowHeader as RowHeader);
            var typedDataArea = (dataArea as DataArea);
            var typedCommonTitleContainer = (commonTitleContainer as CommonTitleContainer);
            var typedVariableTitleContainer = (variableTitleContainer as VariableTitleContainer);

            if (typedColumnHeader == null)
            { throw new InvalidOperationException("Unrecognized type of ColumnHeader encountered."); }
            if (typedRowHeader == null)
            { throw new InvalidOperationException("Unrecognized type of RowHeader encountered."); }
            if (typedDataArea == null)
            { throw new InvalidOperationException("Unrecognized type of DataArea encountered."); }
            if (typedCommonTitleContainer == null)
            { throw new InvalidOperationException("Unrecognized type of CommonTitleContainer encountered."); }
            if (typedVariableTitleContainer == null)
            { throw new InvalidOperationException("Unrecognized type of VariableTitleContainer encountered."); }

            if (!tableHeader.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The TableHeader specified belongs to a different Report."); }
            if (!typedColumnHeader.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The ColumnHeader specified belongs to a different Report."); }
            if (!typedRowHeader.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The RowHeader specified belongs to a different Report."); }
            if (!typedDataArea.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The DataArea specified belongs to a different Report."); }
            if (!typedCommonTitleContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The CommonTitleContainer specified belongs to a different Report."); }
            if (!typedVariableTitleContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The VariableTitleContainer specified belongs to a different Report."); }
            if (!variableDataContainer.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The VariableDataContainer specified belongs to a different Report."); }

            commonTitleContainer.AssertContainersDoNotHaveOverlappingDimensions(variableTitleContainer);

            m_TableHeaderNumber = tableHeader.Key.ReportElementNumber;
            m_ColumnHeaderNumber = typedColumnHeader.Key.ReportElementNumber;
            m_RowHeaderNumber = typedRowHeader.Key.ReportElementNumber;
            m_DataAreaNumber = typedDataArea.Key.ReportElementNumber;
            tableHeader.ParentElementId = this.Key;
            typedColumnHeader.ParentElementId = this.Key;
            typedRowHeader.ParentElementId = this.Key;
            typedDataArea.ParentElementId = this.Key;

            m_CommonTitleContainerNumber = typedCommonTitleContainer.Key.ReportElementNumber;
            m_VariableTitleContainerNumber = typedVariableTitleContainer.Key.ReportElementNumber;
            m_VariableDataContainerNumber = variableDataContainer.Key.ReportElementNumber;

            if (!this.IsTransposed)
            {
                typedColumnHeader.SetToCommonTitleContainer(commonTitleContainer, new List<ITransposableElement>());
                typedRowHeader.SetToVariableTitleContainer(variableTitleContainer, variableDataContainer, new List<ITransposableElement>());
                typedDataArea.SetVariableDataContainer(variableDataContainer);
            }
            else
            {
                typedColumnHeader.SetToVariableTitleContainer(variableTitleContainer, variableDataContainer, new List<ITransposableElement>());
                typedRowHeader.SetToCommonTitleContainer(commonTitleContainer, new List<ITransposableElement>());
                typedDataArea.SetVariableDataContainer(variableDataContainer);
            }

            typedVariableTitleContainer.SetVariableDataContainer(variableDataContainer);
            typedCommonTitleContainer.SetVariableDataContainer(variableDataContainer);
        }

        public void TransposeContainedElements(IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> nestedElements)
        {
            var transpose = !this.IsTransposed;
            SetTransposedForContainedElements(transpose, columnHeader, rowHeader, commonTitleContainer, variableTitleContainer, variableDataContainer, nestedElements);
        }

        public void TransposeContainedElements(IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements)
        {
            var transpose = !this.IsTransposed;
            SetTransposedForContainedElements(transpose, columnHeader, rowHeader, commonTitleContainer, commonTitleElements, variableTitleContainer, variableTitleElements, variableDataContainer, variableDataElements);
        }

        public void SetTransposedForContainedElements(bool transpose, IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IVariableTitleContainer variableTitleContainer, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> nestedElements)
        {
            var nestedElementsByKey = nestedElements.OrderBy(x => x.Key.ReportElementNumber).ToDictionary(x => x.Key, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId);

            var childParents = nestedElements.Select(x => new KeyValuePair<ReportElementId, ReportElementId?>(x.Key, x.ParentElementId)).ToDictionary(x => x.Key, x => x.Value, ReportRenderingEngine.EqualityComparer_ReportElementId);
            childParents.Add(commonTitleContainer.Key, null);
            childParents.Add(variableTitleContainer.Key, null);
            childParents.Add(variableDataContainer.Key, null);
            var elementSubTree = new CachedTree<ReportElementId>(childParents, ReportRenderingEngine.EqualityComparer_ReportElementId);

            if (elementSubTree.RootNodes.Count != 3)
            { throw new InvalidOperationException("Unexpected DimensionTable elements encountered."); }

            var commonTitleElements = elementSubTree.GetDecendants(commonTitleContainer.Key).Select(x => nestedElementsByKey[x]).ToList();
            var variableTitleElements = elementSubTree.GetDecendants(variableTitleContainer.Key).Select(x => nestedElementsByKey[x]).ToList();
            var variableDataElements = elementSubTree.GetDecendants(variableDataContainer.Key).Select(x => nestedElementsByKey[x]).ToList();

            SetTransposedForContainedElements(transpose, columnHeader, rowHeader, commonTitleContainer, commonTitleElements, variableTitleContainer, variableTitleElements, variableDataContainer, variableDataElements);
        }

        public void SetTransposedForContainedElements(bool transpose, IColumnHeader columnHeader, IRowHeader rowHeader, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements)
        {
            var typedColumnHeader = (columnHeader as ColumnHeader);
            var typedRowHeader = (rowHeader as RowHeader);

            this.AssertAllRequiredElementsArePresentAndProperlyConfigured(commonTitleContainer, commonTitleElements, variableTitleContainer, variableTitleElements, variableDataContainer, variableDataElements);

            var allVariableElements = variableTitleElements.Union(variableDataElements).ToList();

            m_IsTransposed = transpose;
            if (!transpose)
            {
                typedColumnHeader.SetToCommonTitleContainer(commonTitleContainer, commonTitleElements);
                typedRowHeader.SetToVariableTitleContainer(variableTitleContainer, variableDataContainer, allVariableElements);
            }
            else
            {
                typedColumnHeader.SetToVariableTitleContainer(variableTitleContainer, variableDataContainer, allVariableElements);
                typedRowHeader.SetToCommonTitleContainer(commonTitleContainer, commonTitleElements);
            }
        }

        #endregion
    }
}