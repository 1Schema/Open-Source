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
    public partial class VariableDataBox : ReportElementBase<VariableDataBox>, IVariableDataBox
    {
        protected Nullable<int> m_RelatedVariableTitleBoxNumber;
        protected Nullable<int> m_RelatedCommonTitleBoxNumber;
        protected Dimension m_StackingDimension;

        protected Nullable<int> m_ContainedVariableDataRangeNumber;
        protected string m_StyleGroup;

        protected ModelObjectReference m_VariableTemplateRef;

        public VariableDataBox()
            : this(ReportId.DefaultId, ModelObjectReference.GlobalTypeReference)
        { }

        public VariableDataBox(ReportId reportId, ModelObjectReference variableTemplateRef)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, variableTemplateRef)
        { }

        public VariableDataBox(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, ModelObjectReference variableTemplateRef)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_RelatedVariableTitleBoxNumber = null;
            m_RelatedCommonTitleBoxNumber = null;
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_ContainedVariableDataRangeNumber = null;
            m_StyleGroup = null;

            m_VariableTemplateRef = variableTemplateRef;
        }

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_VariableDataBox;
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

        #region IVariableDataBox Implementation

        [NotMapped]
        public ReportElementId RelatedVariableTitleBoxId
        {
            get { return new ReportElementId(m_Key.ReportId, m_RelatedVariableTitleBoxNumber.Value); }
            internal set
            {
                if (!value.ReportId.Equals_Revisionless(this.ParentReportId))
                { throw new InvalidOperationException("Cannot mix elements from different reports."); }

                m_RelatedVariableTitleBoxNumber = value.ReportElementNumber;
            }
        }

        [NotMapped]
        public int RelatedVariableTitleBoxNumber
        {
            get { return RelatedVariableTitleBoxId.ReportElementNumber; }
        }

        [NotMapped]
        public ReportElementId RelatedCommonTitleBoxId
        {
            get { return new ReportElementId(m_Key.ReportId, m_RelatedCommonTitleBoxNumber.Value); }
            internal set
            {
                if (!value.ReportId.Equals_Revisionless(this.ParentReportId))
                { throw new InvalidOperationException("Cannot mix elements from different reports."); }

                m_RelatedCommonTitleBoxNumber = value.ReportElementNumber;
            }
        }

        [NotMapped]
        public int RelatedCommonTitleBoxNumber
        {
            get { return RelatedCommonTitleBoxId.ReportElementNumber; }
        }

        [NotMapped]
        public ICollection<ReportElementId> ChildIds
        {
            get
            {
                var siblings = new List<ReportElementId>();

                if (m_ContainedVariableDataRangeNumber.HasValue)
                { siblings.Add(ContainedVariableDataRangeId); }

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
            set
            {
                (EditabilitySpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.System;
                var dimensionLayout = ElementLayout.GetDimensionLayout(CommonDimension);
                dimensionLayout.ContentGroup_Value = value;
                (EditabilitySpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.User;
            }
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
            { return true; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for 2-D object."); }
        }

        public void SetOrder(Dimension dimension, int order)
        {
            if (!IsOrderEditable(dimension))
            { throw new InvalidOperationException("The specified Dimension is not Orderable withing the current object."); }

            if (dimension == StackingDimension)
            { StackingOrder = order; }
            else if (dimension == CommonDimension)
            { CommonOrder = order; }
            else
            { throw new InvalidOperationException("Invalid Dimension specified for editing Order."); }
        }

        [NotMapped]
        public ReportElementId ContainedVariableDataRangeId
        {
            get { return new ReportElementId(this.Key.ReportId, m_ContainedVariableDataRangeNumber.Value); }
        }

        [NotMapped]
        public int ContainedVariableDataRangeNumber
        {
            get { return ContainedVariableDataRangeId.ReportElementNumber; }
        }

        public void SetContainedVariableDataRange(IVariableDataRange element)
        {
            var typedElement = (element as VariableDataRange);

            if (typedElement == null)
            { throw new InvalidOperationException("Unrecognized type of VariableDataRange encountered."); }
            if (!typedElement.ParentReportId.Equals_Revisionless(this.ParentReportId))
            { throw new InvalidOperationException("The VariableDataRange specified belongs to a different Report."); }

            if (typedElement.ParentElementNumber.HasValue && (typedElement.ParentElementNumber != this.Key.ReportElementNumber))
            { throw new InvalidOperationException("The specified Child Object has already been added to a different VariableDataBox."); }
            if (m_ContainedVariableDataRangeNumber == typedElement.Key.ReportElementNumber)
            { throw new InvalidOperationException("The specified Child Object already exists in the current VariableDataBox."); }

            m_ContainedVariableDataRangeNumber = typedElement.Key.ReportElementNumber;
            typedElement.ParentElementId = this.Key;
            typedElement.ValueVariableTemplateRef = this.VariableTemplateRef;
            typedElement.StackingDimension = this.StackingDimension;
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
            set
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

        public static object GetVariableTemplateName(IFormulaDataProvider dataProvider, IVariableDataBox box)
        {
            var variableTemplateName = dataProvider.GetObjectName(box.VariableTemplateRef);
            return variableTemplateName;
        }

        #endregion
    }
}