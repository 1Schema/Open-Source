using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Presentation;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Dimensionality;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableTitleBox : ReportElementBase<VariableTitleBox>, IVariableTitleBox
    {
        protected Dimension m_StackingDimension;

        protected Nullable<int> m_ContainedVariableTitleRangeNumber;
        protected List<int> m_ContainedStructuralTitleRangeNumbers;
        protected List<int> m_ContainedTimeTitleRangeNumbers;

        protected string m_RepeatGroup;
        protected RepeatMode m_RepeatMode;
        protected string m_StyleGroup;

        protected ModelObjectReference m_VariableTemplateRef;

        public VariableTitleBox()
            : this(ReportId.DefaultId, ModelObjectReference.GlobalTypeReference)
        { }

        public VariableTitleBox(ReportId reportId, ModelObjectReference variableTemplateRef)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, variableTemplateRef)
        { }

        public VariableTitleBox(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, ModelObjectReference variableTemplateRef)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_ContainedVariableTitleRangeNumber = null;
            m_ContainedStructuralTitleRangeNumbers = new List<int>();
            m_ContainedTimeTitleRangeNumbers = new List<int>();

            m_RepeatGroup = DimensionalRepeatGroup.InitialGroupName;
            m_RepeatMode = RepeatModeUtils.DefaultRepeatMode;
            m_StyleGroup = null;

            m_VariableTemplateRef = variableTemplateRef;
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_VariableTitleBox;
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
            return null;
        }

        #endregion

        #region IVariableTitleBox Implementation

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var siblings = new List<ReportElementId>();

                if (m_ContainedVariableTitleRangeNumber.HasValue)
                { siblings.Add(ContainedVariableTitleRangeId); }

                siblings.AddRange(ContainedStructuralTitleRangeIds);
                siblings.AddRange(ContainedTimeTitleRangeIds);

                return siblings;
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
            get { return m_StackingDimension; }
            internal set
            {
                value.AssertIsValidTableDimension();
                var isChanging = (m_StackingDimension != value);
                m_StackingDimension = value;

                if (!isChanging)
                { return; }

                this.TransposeElementLayoutAndStyle();
            }
        }

        [NotMapped]
        public Dimension CommonDimension
        {
            get { return StackingDimension.Invert(); }
        }

        [NotMapped]
        public bool IsTransposed
        {
            get
            {
                var stackingResult = StackingDimension.IsStackingDimensionTransposed();
                var commonResult = CommonDimension.IsCommonDimensionTransposed();

                if (stackingResult != commonResult)
                { throw new InvalidOperationException("Unexpected IsTransposed setting encountered."); }

                return stackingResult;
            }
        }

        [NotMapped]
        public int StackingOrder
        {
            get { return ElementLayout.GetDimensionLayout(StackingDimension).ContentGroup_Value; }
            set
            {
                (EditabilitySpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.System;
                var dimensionLayout = ElementLayout.GetDimensionLayout(StackingDimension);
                dimensionLayout.ContentGroup_Value = value;
                (EditabilitySpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.User;
            }
        }

        [NotMapped]
        public int CommonOrder
        {
            get { return ElementLayout.GetDimensionLayout(CommonDimension).ContentGroup_Value; }
        }

        public int GetOrder(Dimension dimension)
        {
            if (dimension == StackingDimension)
            { return StackingOrder; }
            else if (dimension == CommonDimension)
            { return CommonOrder; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for 2-D object."); }
        }

        public bool IsOrderEditable(Dimension dimension)
        {
            if (dimension == StackingDimension)
            { return true; }
            else if (dimension == CommonDimension)
            { return false; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for 2-D object."); }
        }

        public void SetOrder(Dimension dimension, int order)
        {
            if (!IsOrderEditable(dimension))
            { throw new InvalidOperationException("The specified Dimension is not Orderable withing the current object."); }

            if (dimension == StackingDimension)
            { StackingOrder = order; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for editing Order."); }
        }

        [NotMapped]
        public ReportElementId ContainedVariableTitleRangeId
        {
            get { return new ReportElementId(this.Key.ReportId, m_ContainedVariableTitleRangeNumber.Value); }
        }

        [NotMapped]
        public int ContainedVariableTitleRangeNumber
        {
            get { return ContainedVariableTitleRangeId.ReportElementNumber; }
        }

        public void SetContainedVariableTitleRange(IVariableTitleRange element)
        {
            var typedElement = (element as VariableTitleRange);

            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of VariableTitleRange encountered."); }
            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The VariableTitleRange specified belongs to a different Report."); }

            if (typedElement.ParentElementNumber.HasValue && (typedElement.ParentElementNumber != this.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object has already been added to a different VariableTitleBox."); }
            if (m_ContainedVariableTitleRangeNumber == typedElement.Key.ReportElementNumber)
            { throw new InvalidOperationException("The specified Child Object already exists in the current VariableTitleBox."); }

            m_ContainedVariableTitleRangeNumber = typedElement.Key.ReportElementNumber;
            typedElement.ParentElementId = this.Key;
            typedElement.StackingDimension = this.StackingDimension;
        }

        [NotMapped]
        public ICollection<ReportElementId> ContainedStructuralTitleRangeIds
        {
            get
            {
                var siblings = m_ContainedStructuralTitleRangeNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return siblings;
            }
        }

        [NotMapped]
        public ICollection<int> ContainedStructuralTitleRangeNumbers
        {
            get { return ContainedStructuralTitleRangeIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        [NotMapped]
        public ICollection<ReportElementId> ContainedTimeTitleRangeIds
        {
            get
            {
                var siblings = m_ContainedTimeTitleRangeNumbers.Select(x => new ReportElementId(m_Key.ReportId, x, false)).ToList();
                return siblings;
            }
        }

        [NotMapped]
        public ICollection<int> ContainedTimeTitleRangeNumbers
        {
            get { return ContainedTimeTitleRangeIds.Select(x => x.ReportElementNumber).ToList(); }
        }

        public void AddContainedStructuralTitleRange(IStructuralTitleRange element)
        {
            var typedElement = (element as StructuralTitleRange);

            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of StructuralTitleRange encountered."); }
            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (typedElement.ParentElementNumber.HasValue && (typedElement.ParentElementNumber != this.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object has already been added to a different VariableTitleBox."); }
            if (m_ContainedStructuralTitleRangeNumbers.Contains(typedElement.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the VariableTitleBox."); }

            m_ContainedStructuralTitleRangeNumbers.Add(typedElement.Key.ReportElementNumber);
            typedElement.ParentElementId = this.Key;
            typedElement.IsVariableTitleRelated = true;
            typedElement.StackingDimension = this.StackingDimension;
        }

        public void RemoveContainedStructuralTitleRange(IStructuralTitleRange element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_ContainedStructuralTitleRangeNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the VariableTitleBox."); }

            m_ContainedStructuralTitleRangeNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        public void AddContainedTimeTitleRange(ITimeTitleRange element)
        {
            var typedElement = (element as TimeTitleRange);

            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of TimeTitleRange encountered."); }
            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (typedElement.ParentElementNumber.HasValue && (typedElement.ParentElementNumber != this.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object has already been added to a different VariableTitleBox."); }
            if (m_ContainedTimeTitleRangeNumbers.Contains(typedElement.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object already exists in the VariableTitleBox."); }

            m_ContainedTimeTitleRangeNumbers.Add(typedElement.Key.ReportElementNumber);
            typedElement.ParentElementId = this.Key;
            typedElement.IsVariableTitleRelated = true;
            typedElement.StackingDimension = this.StackingDimension;
        }

        public void RemoveContainedTimeTitleRange(ITimeTitleRange element)
        {
            if (!element.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("Cannot mix elements from different reports."); }

            if (!m_ContainedTimeTitleRangeNumbers.Contains(element.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object does not exist in the VariableTitleBox."); }

            m_ContainedTimeTitleRangeNumbers.Remove(element.Key.ReportElementNumber);
            element.ParentElementId = null;
        }

        [NotMapped]
        [PropertyDisplayData("Repeat Group", "The name of the group that the VariableTemplate is repeated with.", true, EditorType.Input, OrderNumber_NonNull = 10, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public string RepeatGroup
        {
            get { return m_RepeatGroup; }
            set { m_RepeatGroup = value; }
        }

        [NotMapped]
        [PropertyDisplayData("Repeat Mode", "How to repeat duplicates?", true, EditorType.Select, typeof(RepeatMode), OrderNumber_NonNull = 11, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public RepeatMode RepeatMode
        {
            get { return m_RepeatMode; }
            set { m_RepeatMode = value; }
        }

        [NotMapped]
        public string StyleGroup
        {
            get { return m_StyleGroup; }
            set { m_StyleGroup = value; }
        }

        [NotMapped]
        [PropertyDisplayData("Variable Template", "The Variable Template to use to obtain the rendered text of the Range.", false, EditorType.Input, OrderNumber_NonNull = 9, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public ModelObjectReference VariableTemplateRef
        {
            get { return m_VariableTemplateRef; }
            internal set
            {
                if (m_VariableTemplateRef.ModelObjectType != ModelObjectType.VariableTemplate)
                { throw new InvalidOperationException("Variable Title Ranges can only use information from Variable Templates."); }

                m_VariableTemplateRef = value;
            }
        }

        #endregion

        #region IPropertyDisplayDataOverrider_WithValue Overrides

        public override object Get_Value_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.VariableTemplateRef))
            {
                var dataProvider = (modelDataProviderAsObj as IFormulaDataProvider);
                return GetVariableTemplateName(dataProvider, this);
            }
            else
            {
                return base.Get_Value_ForProperty(propertyAttribute, modelDataProviderAsObj, reportDataObjectAsObj, args);
            }
        }

        #endregion

        #region Static Methods

        public static object GetVariableTemplateName(IFormulaDataProvider dataProvider, IVariableTitleBox box)
        {
            var variableTemplateName = dataProvider.GetObjectName(box.VariableTemplateRef);
            return variableTemplateName;
        }

        #endregion
    }
}