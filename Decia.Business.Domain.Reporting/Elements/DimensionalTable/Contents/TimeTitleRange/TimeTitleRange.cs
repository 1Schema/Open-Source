using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
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
    public partial class TimeTitleRange : ValueRangeBase<TimeTitleRange>, ITimeTitleRange
    {
        public const TimeDimensionType DefaultTimeDimensionType = TimeDimensionType.Primary;
        public const TimePeriodType DefaultTimePeriodType = TimePeriodType.Years;

        #region Members

        protected bool m_IsVariableTitleRelated;
        protected Dimension m_StackingDimension;

        protected bool m_IsHidden;
        protected string m_StyleGroup;

        protected bool m_OnlyRepeatOnChange;
        protected bool m_MergeRepeatedValues;

        protected TimeDimensionType m_TimeDimensionType;
        protected TimePeriodType m_TimePeriodType;

        #endregion

        #region Constructors

        public TimeTitleRange()
            : this(ReportId.DefaultId, DefaultTimeDimensionType)
        { }

        public TimeTitleRange(ReportId reportId, TimeDimensionType timeDimensionType)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid, timeDimensionType)
        { }

        public TimeTitleRange(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid, TimeDimensionType timeDimensionType)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            ITimeDimensionUtils.AssertTimeDimensionTypeIsValid(timeDimensionType);

            m_IsVariableTitleRelated = false;
            m_StackingDimension = ITransposableElementUtils.Default_StackingDimension;

            m_IsHidden = false;
            m_StyleGroup = null;

            m_OnlyRepeatOnChange = false;
            m_MergeRepeatedValues = false;

            TimeDimensionType = timeDimensionType;
            m_TimePeriodType = DefaultTimePeriodType;
            m_OutputValueType = OutputValueType.ReferencedId;
        }

        #endregion

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.DimensionalTable_TimeTitleRange;
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
            string dimensionName = TimeDimensionType.ToString();
            string periodName = TimePeriodType.ToString();

            var objectName = string.Format("{0} Time Dimension, in {1}", dimensionName, periodName);
            return objectName;
        }

        protected override bool UsesCustom_RefValue()
        { return true; }

        protected override object GetCustom_RefValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        {
            var periodNameFormat = "{0} - {1}";
            var timePeriod = (this.TimeDimensionType == TimeDimensionType.Primary) ? renderingState.CurrentTimeBindings.PrimaryTimePeriod : renderingState.CurrentTimeBindings.SecondaryTimePeriod;
            var timePeriodName = string.Format(periodNameFormat, timePeriod.StartDate.ToShortDateString(), timePeriod.EndDate.ToShortDateString());

            var objectValue = timePeriodName;
            return objectValue;
        }

        #endregion

        #region ITimeTitleRange Implementation

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
            get { return new ModelObjectReference(m_TimeDimensionType); }
        }

        [NotMapped]
        [PropertyDisplayData("Time Dimension", "The Time Dimension that the Time Title Range represents.", false, EditorType.Select, typeof(TimeDimensionType), OrderNumber_NonNull = 10, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public TimeDimensionType TimeDimensionType
        {
            get { return m_TimeDimensionType; }
            internal set
            {
                ITimeDimensionUtils.AssertTimeDimensionTypeIsValid(value);
                m_TimeDimensionType = value;
                m_RefValue = new ModelObjectReference(m_TimeDimensionType);
            }
        }

        [NotMapped]
        public TimeValueType TimeValueType
        {
            get { return TimeValueType.PeriodValue; }
        }

        [NotMapped]
        [PropertyDisplayData("Time Period", "The Time Period used for the given Time Dimension.", false, EditorType.Select, typeof(TimePeriodType), OrderNumber_NonNull = 11, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public TimePeriodType TimePeriodType
        {
            get { return m_TimePeriodType; }
            set { m_TimePeriodType = value; }
        }

        #endregion

        #region IValueRange_Configurable Overrides

        public override bool HasRefValue { get { return true; } }

        public override ModelObjectReference RefValue { get { return DimensionalTypeRef; } }

        public override void SetToRefValue(ModelObjectReference reference)
        {
            throw new InvalidOperationException("A Time Title Range does not allow its \"RefValue\" to be changed.");
        }

        #endregion
    }
}