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
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Rendering;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class StructuralTitleRange : ValueRangeBase<StructuralTitleRange>, IStructuralTitleRange
    {
        #region Members

        protected bool m_IsVariableTitleRelated;
        protected Dimension m_StackingDimension;

        protected bool m_IsHidden;
        protected string m_StyleGroup;

        protected bool m_OnlyRepeatOnChange;
        protected bool m_MergeRepeatedValues;

        protected ModelObjectReference m_EntityTypeRef;

        #endregion

        #region Constructors

        public StructuralTitleRange()
            : this(ReportId.DefaultId, new ModelObjectReference(ModelObjectType.EntityType, 1))
        { }

        public StructuralTitleRange(ReportId reportId, ModelObjectReference entityTypeRef)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, entityTypeRef)
        { }

        public StructuralTitleRange(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, ModelObjectReference entityTypeRef)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_IsVariableTitleRelated = false;
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_IsHidden = false;
            m_StyleGroup = null;

            m_OnlyRepeatOnChange = false;
            m_MergeRepeatedValues = false;

            EntityTypeRef = entityTypeRef;
            m_OutputValueType = OutputValueType.ReferencedId;
        }

        #endregion

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_StructuralTitleRange;
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
            var objectName = GetStructuralDimensionName(dataProvider, this);
            return objectName;
        }

        protected override bool UsesCustom_RefValue()
        { return false; }

        protected override object GetCustom_RefValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        { return null; }

        #endregion

        #region IStructuralTitleRange Implementation

        [NotMapped]
        public bool IsVariableTitleRelated
        {
            get { return m_IsVariableTitleRelated; }
            set { m_IsVariableTitleRelated = value; }
        }

        [NotMapped]
        public bool IsCommonTitleRelated
        {
            get { return !IsVariableTitleRelated; }
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
        public bool OnlyRepeatOnChange
        {
            get { return m_OnlyRepeatOnChange; }
            set { m_OnlyRepeatOnChange = value; }
        }

        [NotMapped]
        public bool MergeRepeatedValues
        {
            get { return m_MergeRepeatedValues; }
            set { m_MergeRepeatedValues = value; }
        }

        [NotMapped]
        public ModelObjectReference DimensionalTypeRef
        {
            get { return EntityTypeRef; }
        }

        [NotMapped]
        [PropertyDisplayData("Structural Dimension", "The Structural Dimension that the Structural Title Range represents.", false, EditorType.Input, OrderNumber_NonNull = 10, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public ModelObjectReference EntityTypeRef
        {
            get { return m_EntityTypeRef; }
            internal set
            {
                if (value.ModelObjectType != ModelObjectType.EntityType)
                { throw new InvalidOperationException("Structural Title Ranges can only use information from Entity Types."); }

                m_EntityTypeRef = value;
                m_RefValue = value;
            }
        }

        #endregion

        #region IValueRange_Configurable Overrides

        public override bool HasRefValue { get { return true; } }

        public override ModelObjectReference RefValue { get { return DimensionalTypeRef; } }

        public override void SetToRefValue(ModelObjectReference reference)
        {
            throw new InvalidOperationException("A Structural Title Range does not allow its \"RefValue\" to be changed.");
        }

        #endregion

        #region IPropertyDisplayDataOverrider_WithValue Overrides

        public override object Get_Value_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.EntityTypeRef))
            {
                var dataProvider = (modelDataProviderAsObj as IFormulaDataProvider);
                return GetStructuralDimensionName(dataProvider, this);
            }
            else
            {
                return base.Get_Value_ForProperty(propertyAttribute, modelDataProviderAsObj, reportDataObjectAsObj, args);
            }
        }

        #endregion

        #region Static Methods

        public static string GetStructuralDimensionName(IFormulaDataProvider dataProvider, IStructuralTitleRange range)
        {
            var structuralName = dataProvider.GetObjectName(range.EntityTypeRef);
            if (range.EntityTypeRef.NonNullAlternateDimensionNumber > ModelObjectReference.MinimumAlternateDimensionNumber)
            { structuralName += " D-" + range.EntityTypeRef.NonNullAlternateDimensionNumber; }
            return structuralName;
        }

        #endregion
    }
}