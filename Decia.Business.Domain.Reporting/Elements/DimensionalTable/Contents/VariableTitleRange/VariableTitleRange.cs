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
using Decia.Business.Domain.Reporting.Rendering;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class VariableTitleRange : ValueRangeBase<VariableTitleRange>, IVariableTitleRange
    {
        #region Members

        protected Dimension m_StackingDimension;

        protected bool m_IsHidden;
        protected string m_StyleGroup;

        protected ModelObjectReference m_NameVariableTemplateRef;

        #endregion

        #region Constructors

        public VariableTitleRange()
            : this(ReportId.DefaultId, new ModelObjectReference(ModelObjectType.VariableTemplate, 1))
        { }

        public VariableTitleRange(ReportId reportId, ModelObjectReference nameVariableTemplateRef)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, nameVariableTemplateRef)
        { }

        public VariableTitleRange(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, ModelObjectReference nameVariableTemplateRef)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_IsHidden = false;
            m_StyleGroup = null;

            NameVariableTemplateRef = nameVariableTemplateRef;
            m_OutputValueType = OutputValueType.ReferencedId;
        }

        #endregion

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_VariableTitleRange;
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

        protected override bool UsesCustom_RefName()
        { return true; }

        protected override string GetCustom_RefName(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        {
            var objectName = GetVariableTemplateName(dataProvider, this);
            return objectName;
        }

        protected override bool UsesCustom_RefValue()
        { return true; }

        protected override object GetCustom_RefValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        {
            var objectName = GetVariableTemplateName(dataProvider, this);
            return objectName;
        }

        #endregion

        #region IVariableTitleRange Implementation

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
        [PropertyDisplayData("Is Hidden", "Whether to hide the Title Range when rendering Production Reports.", true, EditorType.Input, OrderNumber_NonNull = 9, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public bool IsHidden
        {
            get { return m_IsHidden; }
            set { m_IsHidden = value; }
        }

        [NotMapped]
        public string StyleGroup
        {
            get { return m_StyleGroup; }
            set { m_StyleGroup = value; }
        }

        [NotMapped]
        [PropertyDisplayData("Name Variable", "The Variable Template to use to obtain the rendered text of the Range.", false, EditorType.Input, OrderNumber_NonNull = 10, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public ModelObjectReference NameVariableTemplateRef
        {
            get { return m_NameVariableTemplateRef; }
            set
            {
                if (value.ModelObjectType != ModelObjectType.VariableTemplate)
                { throw new InvalidOperationException("Variable Title Ranges can only use information from Variable Templates."); }

                m_NameVariableTemplateRef = value;
                m_RefValue = value;
            }
        }

        #endregion

        #region IValueRange_Configurable Overrides

        public override bool HasRefValue { get { return true; } }

        public override ModelObjectReference RefValue { get { return NameVariableTemplateRef; } }

        public override void SetToRefValue(ModelObjectReference reference)
        {
            throw new InvalidOperationException("A Variable Title Range does not allow its \"RefValue\" to be changed.");
        }

        #endregion

        #region IPropertyDisplayDataOverrider_WithValue Overrides

        public override object Get_Value_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.NameVariableTemplateRef))
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

        public static string GetVariableTemplateName(IFormulaDataProvider dataProvider, IVariableTitleRange range)
        {
            var variableTemplateName = dataProvider.GetObjectName(range.NameVariableTemplateRef);
            return variableTemplateName;
        }

        #endregion
    }
}