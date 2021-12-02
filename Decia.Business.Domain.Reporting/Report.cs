using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Presentation;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Domain;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Rendering;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class Report : ModelDomainObjectBase_DeleteableOrderable<ReportId, Report>, IReport<Report>, IReportElement, IPropertyDisplayDataOverrider
    {
        #region Static Members

        public static readonly bool ShowDebugText = true;

        public const ReservedElementType Report_ReportElementType = ReservedElementTypeUtils.Report_ReportElementType;
        public const int Report_ReportElementNumber = ReservedElementTypeUtils.Report_ReportElementNumber;
        public static readonly IEnumerable<int> Report_AllElementNumbers = ReservedElementTypeUtils.Report_AllElementNumbers;

        public static readonly ModelObjectReference Default_StructuralTypeRef = ModelObjectReference.GlobalTypeReference;
        public static readonly string Default_Name = "New Report";
        public static readonly double Default_ZoomFactor = CellLayoutManagerUtils.DefaultZoomFactor;
        public static readonly int Default_MinSize = 20;
        public static readonly int Default_ZOrder = 0;

        #endregion

        #region Members

        protected string m_Name;
        protected string m_Description;
        protected bool? m_IsLocked;

        protected ModelObjectReference m_StructuralTypeRef;
        protected bool m_HasPrimaryTimePeriod;
        protected bool m_HasSecondaryTimePeriod;

        protected double m_ZoomFactor;
        protected SaveableDimensionLayout m_ReportArea_DimensionLayout_X;
        protected SaveableDimensionLayout m_ReportArea_DimensionLayout_Y;
        protected SaveableElementStyle m_ReportAreaStyle;
        protected SaveableElementStyle m_OutsideAreaStyle;

        protected SaveableElementStyle m_DefaultTitleStyle;
        protected SaveableElementStyle m_DefaultHeaderStyle;
        protected SaveableElementStyle m_DefaultDataStyle;

        #endregion

        #region Constructors

        public Report()
            : this(ReportId.DefaultId.ProjectGuid, ReportId.DefaultId.RevisionNumber_NonNull, ReportId.DefaultId.ModelTemplateNumber)
        { }

        public Report(Guid projectGuid, long revisionNumber, int modelTemplateNumber)
            : this(projectGuid, revisionNumber, new ModelObjectReference(ModelObjectType.ModelTemplate, modelTemplateNumber), false, null)
        { }

        public Report(Guid projectGuid, long revisionNumber, int modelTemplateNumber, int reportEnumValue)
            : this(projectGuid, revisionNumber, new ModelObjectReference(ModelObjectType.ModelTemplate, modelTemplateNumber), true, reportEnumValue)
        { }

        public Report(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef)
            : this(projectGuid, revisionNumber, modelTemplateRef, false, null)
        { }

        public Report(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef, int reportEnumValue)
            : this(projectGuid, revisionNumber, modelTemplateRef, true, reportEnumValue)
        { }

        protected Report(Guid projectGuid, long revisionNumber, ModelObjectReference modelTemplateRef, bool useReportEnum, int? reportEnumValue)
            : base(projectGuid, revisionNumber, modelTemplateRef.ModelObjectIdAsInt, null)
        {
            if (modelTemplateRef.ModelObjectType != ModelObjectType.ModelTemplate)
            { throw new InvalidOperationException("The specified ModelObjectReference does not refer to a ModelTemplate."); }
            if (useReportEnum && !reportEnumValue.HasValue)
            { throw new InvalidOperationException("No Report Enum value was specified."); }

            var reportGuid = (useReportEnum) ? reportEnumValue.Value.ConvertToGuid() : Guid.NewGuid();

            m_Key = new ReportId(projectGuid, revisionNumber, modelTemplateRef.ModelObjectIdAsInt, reportGuid);
            m_Name = Default_Name;
            m_Description = string.Empty;
            m_IsLocked = false;

            m_StructuralTypeRef = Default_StructuralTypeRef;
            m_HasPrimaryTimePeriod = false;
            m_HasSecondaryTimePeriod = false;

            m_ZoomFactor = Default_ZoomFactor;
            m_ReportArea_DimensionLayout_X = new SaveableDimensionLayout(ReportAreaTemplateId, Dimension.X, GetEditabilitySpecForLayout(Dimension.X));
            m_ReportArea_DimensionLayout_Y = new SaveableDimensionLayout(ReportAreaTemplateId, Dimension.Y, GetEditabilitySpecForLayout(Dimension.Y));
            m_ReportArea_DimensionLayout_X.DefaultLayout_Value = GetDefaultLayout(Dimension.X);
            m_ReportArea_DimensionLayout_Y.DefaultLayout_Value = GetDefaultLayout(Dimension.Y);

            m_ReportAreaStyle = new SaveableElementStyle(ReportAreaTemplateId);
            m_ReportAreaStyle.DefaultStyle_Value = GetDefaultStyle();
            m_OutsideAreaStyle = new SaveableElementStyle(OutsideAreaTemplateId);
            m_OutsideAreaStyle.DefaultStyle_Value = GetDefaultStyle();
            m_DefaultTitleStyle = new SaveableElementStyle(TitleTemplateId);
            m_DefaultTitleStyle.DefaultStyle_Value = GetDefaultStyle();
            m_DefaultTitleStyle.FontStyle_Value = (DeciaFontStyle.Bold | DeciaFontStyle.Italic);
            m_DefaultHeaderStyle = new SaveableElementStyle(HeaderTemplateId);
            m_DefaultHeaderStyle.DefaultStyle_Value = GetDefaultStyle();
            m_DefaultHeaderStyle.FontStyle_Value = DeciaFontStyle.Bold;
            m_DefaultDataStyle = new SaveableElementStyle(DataTemplateId);
            m_DefaultDataStyle.DefaultStyle_Value = GetDefaultStyle();
        }

        #endregion

        #region Base-Class Method Overrides

        protected override Guid GetProjectGuid()
        {
            return EF_ProjectGuid;
        }

        protected override long GetRevisionNumber()
        {
            return EF_RevisionNumber;
        }

        protected override int GetModelTemplateNumber()
        {
            return EF_ModelTemplateNumber;
        }

        protected override Guid? GetModelInstanceGuid()
        {
            return null;
        }

        protected override string GetOrderValue()
        {
            return m_Name;
        }

        protected override void SetProjectGuid(Guid projectGuid)
        {
            EF_ProjectGuid = projectGuid;
        }

        protected override void SetRevisionNumber(long revisionNumber)
        {
            EF_RevisionNumber = revisionNumber;
        }

        protected override void SetModelTemplateNumber(int modelTemplateNumber)
        {
            EF_ModelTemplateNumber = modelTemplateNumber;
        }

        protected override void SetModelInstanceGuid(Guid? modelInstanceNumber)
        {
            // do nothing
        }

        #endregion

        internal ReportElementId ReportAreaTemplateId
        {
            get { return new ReportElementId(m_Key, Report.Report_ReportElementType); }
        }

        internal ReportElementId OutsideAreaTemplateId
        {
            get { return new ReportElementId(m_Key, ReservedElementType.AreaOutsideReport); }
        }

        internal ReportElementId TitleTemplateId
        {
            get { return new ReportElementId(m_Key, ReservedElementType.TitleTemplate); }
        }

        internal ReportElementId HeaderTemplateId
        {
            get { return new ReportElementId(m_Key, ReservedElementType.HeaderTemplate); }
        }

        internal ReportElementId DataTemplateId
        {
            get { return new ReportElementId(m_Key, ReservedElementType.DataTemplate); }
        }

        #region IReport Implementation

        public override string ToString()
        {
            if (!ShowDebugText)
            { return base.ToString(); }
            else
            { return Name; }
        }

        [NotMapped]
        public ReportElementId KeyAsElementId
        {
            get { return ReportElementId.GetElementId_ReportLevel(Key); }
        }

        [NotMapped]
        public Guid ReportGuid
        {
            get { return this.Key.ReportGuid; }
        }

        [NotMapped]
        [PropertyDisplayData("Name", "Text that identifies the Report in the system.", true, EditorType.Input, OrderNumber_NonNull = 1, SupportedViewTypes = PropertyHost_ViewType.StructuralMap | PropertyHost_ViewType.DependencyMap | PropertyHost_ViewType.ReportDesigner)]
        public string Name
        {
            get { return SpecialCharacterConverter.ApplyEscapeCodesIfNecessary(m_Name); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                { throw new InvalidOperationException("The specified Report Name is not valid."); }

                m_Name = value;
            }
        }

        [NotMapped]
        [PropertyDisplayData("Description", "Text about the nature of the Report.", true, EditorType.Input, OrderNumber_NonNull = 2, SupportedViewTypes = PropertyHost_ViewType.StructuralMap | PropertyHost_ViewType.DependencyMap | PropertyHost_ViewType.ReportDesigner)]
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [NotMapped]
        [PropertyDisplayData("Sorting Number", "If specified, determines value on which to sort.", true, EditorType.Input, OrderNumber_NonNull = 3, SupportedViewTypes = PropertyHost_ViewType.StructuralMap | PropertyHost_ViewType.DependencyMap | PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public new Nullable<long> OrderNumber
        {
            get { return base.OrderNumber; }
            set { base.OrderNumber = value; }
        }

        [NotMapped]
        public ModelObjectReference ReportTemplateRef
        {
            get { return new ModelObjectReference(ModelObjectType.ReportTemplate, m_Key.ReportGuid); }
        }

        [NotMapped]
        [PropertyDisplayData("Is Locked", "Is the ReportTemplate locked for edits.", true, EditorType.Input, OrderNumber_NonNull = 4, SupportedViewTypes = PropertyHost_ViewType.StructuralMap | PropertyHost_ViewType.DependencyMap | PropertyHost_ViewType.ReportDesigner)]
        public bool IsLocked
        {
            get
            {
                var thisAsReportElement = (this as IReportElement);
                var isLocked = (m_IsLocked.HasValue) ? m_IsLocked.Value : thisAsReportElement.ReportElementType.IsLocked_Default();
                return isLocked;
            }
            set
            {
                var thisAsReportElement = (this as IReportElement);
                thisAsReportElement.ReportElementType.AssertIsUnlockable();
                m_IsLocked = value;
            }
        }

        [NotMapped]
        public bool IsContainer
        {
            get { return ReportElementType_New.Container.IsContainer(); }
        }

        [NotMapped]
        public bool AreContentsEditable
        {
            get { return ReportElementType_New.Container.AreContentsEditable(); }
        }

        [NotMapped]
        public ICollection<ReportElementType_New> AcceptableContentTypes
        {
            get { return ReportElementType_New.Container.GetAcceptableContentTypes(); }
        }

        [NotMapped]
        public ModelObjectReference ModelTemplateRef
        {
            get { return new ModelObjectReference(ModelObjectType.ModelTemplate, m_Key.ModelTemplateNumber); }
        }

        [NotMapped]
        [PropertyDisplayData("Structural Type", "The StructuralType that determines the number of Reports to Render .", true, EditorType.Select, OrderNumber_NonNull = 5, SupportedViewTypes = PropertyHost_ViewType.StructuralMap | PropertyHost_ViewType.DependencyMap | PropertyHost_ViewType.ReportDesigner)]
        public string StructuralTypeComplexId
        {
            get
            {
                var objRef = StructuralTypeRef;
                var complexId = ConversionUtils.ConvertComplexIdToString(objRef.ModelObjectType, objRef.ModelObjectId, ConversionUtils.ItemPartSeparator_Override);
                return complexId;
            }
            set
            {
                var complexId = value;
                var objRef = ConversionUtils.ConvertStringToModelObjectRef(complexId, ConversionUtils.ItemPartSeparator_Override);
                StructuralTypeRef = objRef;
            }
        }

        [NotMapped]
        public ModelObjectReference StructuralTypeRef
        {
            get { return m_StructuralTypeRef; }
            set
            {
                if (!value.IsStructuralType())
                { throw new InvalidOperationException("The specified reference is not a Structural Type."); }

                m_StructuralTypeRef = value;
            }
        }

        [NotMapped]
        public IDictionary<TimeDimensionType, Nullable<TimePeriodType>> TimeDimensionUsages
        {
            get
            {
                var timeDimensionUsages = new Dictionary<TimeDimensionType, Nullable<TimePeriodType>>();
                timeDimensionUsages.Add(TimeDimensionType.Primary, null);
                timeDimensionUsages.Add(TimeDimensionType.Secondary, null);
                return timeDimensionUsages;
            }
        }

        public bool HasTimePeriodType(int timeDimensionNumber)
        {
            return false;
        }

        public TimePeriodType GetTimePeriodType(int timeDimensionNumber)
        {
            return GetNullableTimePeriodType(timeDimensionNumber).Value;
        }

        public Nullable<TimePeriodType> GetNullableTimePeriodType(int timeDimensionNumber)
        {
            return null;
        }

        [NotMapped]
        [PropertyDisplayData("Zoom Factor", "How zoomed-in should the ReportDesigner show as?", false, EditorType.Input, OrderNumber_NonNull = 6, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public double ZoomFactor
        {
            get { return m_ZoomFactor; }
            set
            {
                VisualizationAssertions.AssertZoomFactorIsValid(value);
                m_ZoomFactor = value;
            }
        }

        [NotMapped]
        [PropertyDisplayData("Report Area Layout", "The flexible layout values that determine the outside and content areas of the Report.", true, PopupType.FlexibleLayout, OrderNumber_NonNull = 7, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public string ReportAreaBoxPosition
        {
            get
            {
                var leftMargin = m_ReportArea_DimensionLayout_X.Margin_Value.LesserSide;
                var rightMargin = m_ReportArea_DimensionLayout_X.Margin_Value.GreaterSide;
                var desiredWidth = m_ReportArea_DimensionLayout_X.RangeSize_Design_Value;

                var topMargin = m_ReportArea_DimensionLayout_Y.Margin_Value.LesserSide;
                var bottomMargin = m_ReportArea_DimensionLayout_Y.Margin_Value.GreaterSide;
                var desiredHeight = m_ReportArea_DimensionLayout_Y.RangeSize_Design_Value;

                var margins = new BoxStyleValue<int>(leftMargin, topMargin, rightMargin, bottomMargin);
                var size = new SizeT<int>(desiredWidth, desiredHeight);

                var layoutAsText = margins.ToString() + ConversionUtils.ListItemSeparator + size.ToString();
                return layoutAsText;
            }
        }

        [NotMapped]
        public IElementLayout ReportAreaLayout
        {
            get { return new ElementLayout(IsContainer, m_ReportArea_DimensionLayout_X, m_ReportArea_DimensionLayout_Y); }
        }

        [NotMapped]
        [PropertyDisplayData("Report Area Style", "The style for the content area of the Report.", true, PopupType.Style, OrderNumber_NonNull = 8, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public IElementStyle ReportAreaStyle
        {
            get { return m_ReportAreaStyle; }
        }

        [NotMapped]
        [PropertyDisplayData("Outside Area Style", "The style for the unused border area of the Report.", true, PopupType.Style, OrderNumber_NonNull = 9, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public IElementStyle OutsideAreaStyle
        {
            get { return m_OutsideAreaStyle; }
        }

        [NotMapped]
        [PropertyDisplayData("Title Range Style", "The style for Title Ranges in the Report.", true, PopupType.Style, OrderNumber_NonNull = 10, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public IElementStyle DefaultTitleStyle
        {
            get { return m_DefaultTitleStyle; }
        }

        [NotMapped]
        [PropertyDisplayData("Header Range Style", "The style for Header Ranges in the Report.", true, PopupType.Style, OrderNumber_NonNull = 11, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public IElementStyle DefaultHeaderStyle
        {
            get { return m_DefaultHeaderStyle; }
        }

        [NotMapped]
        [PropertyDisplayData("Data Range Style", "The style for Data Ranges in the Report.", true, PopupType.Style, OrderNumber_NonNull = 12, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public IElementStyle DefaultDataStyle
        {
            get { return m_DefaultDataStyle; }
        }

        IReport IReport.Copy()
        {
            return this.Copy();
        }

        bool IReport.Equals(IReport otherReport)
        {
            return this.Equals(otherReport);
        }

        #endregion

        #region IReportElement Implementation

        [NotMapped]
        ReportElementId IReportElement.Key
        {
            get { return this.KeyAsElementId; }
        }

        [NotMapped]
        int IReportElement.ElementNumber
        {
            get { return this.KeyAsElementId.ReportElementNumber; }
        }

        [NotMapped]
        string IReportElement.Name
        {
            get { return Name; }
            set { Name = value; }
        }

        [NotMapped]
        ModelObjectReference IReportElement.ReportElementTypeRef
        {
            get { return this.ReportTemplateRef; }
        }

        [NotMapped]
        ReportElementType_New IReportElement.ReportElementType
        {
            get { return ReportElementType_New.Report; }
        }

        [NotMapped]
        bool IReportElement.HasDataBoundMultiplicity
        {
            get { return (this as IReportElement).ReportElementType.HasDataBoundMultiplicity(); }
        }

        [NotMapped]
        ReportId IReportElement.ParentReportId
        {
            get { return this.Key; }
        }

        [NotMapped]
        Guid IReportElement.ParentReportGuid
        {
            get { return this.Key.ReportGuid; }
        }

        [NotMapped]
        ReportElementId? IReportElement.ParentElementId
        {
            get { return null; }
            set { throw new InvalidOperationException("Cannot set the Parent Element of a Report."); }
        }

        [NotMapped]
        Nullable<int> IReportElement.ParentElementNumber
        {
            get { return null; }
            set { throw new InvalidOperationException("Cannot set the Parent Element of a Report."); }
        }

        [NotMapped]
        ReportElementId IReportElement.ParentElementOrReportId
        {
            get { return this.KeyAsElementId; }
        }

        [NotMapped]
        int IReportElement.ParentElementOrReportNumber
        {
            get { return this.KeyAsElementId.ReportElementNumber; }
        }

        [NotMapped]
        bool IReportElement.IsLocked
        {
            get { return this.IsLocked; }
            set { this.IsLocked = value; }
        }

        [NotMapped]
        int IReportElement.ZOrder
        {
            get { return Default_ZOrder; }
            set { throw new InvalidOperationException("Cannot set the Z-Order of a Report."); }
        }

        [NotMapped]
        bool IReportElement.IsParentEditable
        {
            get { return (this as IReportElement).ReportElementType.IsParentEditable_Default(); }
            set { throw new InvalidOperationException("Cannot set the Parent Editability of a Report."); }
        }

        [NotMapped]
        bool IReportElement.IsDirectlyDeletable
        {
            get { return (this as IReportElement).ReportElementType.IsDirectlyDeletable_Default(); }
            set { throw new InvalidOperationException("Cannot set the Deltability of a Report."); }
        }

        [NotMapped]
        bool IReportElement.IsContainer
        {
            get { return (this as IReportElement).ReportElementType.IsContainer(); }
        }

        [NotMapped]
        public bool AutoDeletesChildren
        {
            get { return (this as IReportElement).ReportElementType.AutoDeletesChildren(); }
        }

        [NotMapped]
        bool IReportElement.AreContentsEditable
        {
            get { return (this as IReportElement).ReportElementType.AreContentsEditable(); }
        }

        [NotMapped]
        ICollection<ReportElementType_New> IReportElement.AcceptableContentTypes
        {
            get { return (this as IReportElement).ReportElementType.GetAcceptableContentTypes(); }
        }

        [NotMapped]
        bool IReportElement.IsPositionEditable
        {
            get { return (this as IReportElement).ReportElementType.IsEditable_Positioning(); }
        }

        [NotMapped]
        bool IReportElement.IsSizeEditable
        {
            get { return (this as IReportElement).ReportElementType.IsEditable_Sizing(); }
        }

        [NotMapped]
        bool IReportElement.RequiresDelayedSizing
        {
            get { return (this as IReportElement).ReportElementType.RequiresDelayedSizing(); }
        }

        [NotMapped]
        bool IReportElement.IsDirectlyTransposable
        {
            get { return (this as IReportElement).ReportElementType.IsDirectlyTransposable(); }
        }

        [NotMapped]
        bool IReportElement.IsStyleEditable
        {
            get { return (this as IReportElement).ReportElementType.IsEditable_Style(); }
        }

        [NotMapped]
        IElementLayout IReportElement.ElementLayout
        {
            get { return ReportAreaLayout; }
        }

        [NotMapped]
        KnownStyleType IReportElement.DefaultStyleType
        {
            get { return KnownStyleType.None; }
            set { throw new InvalidOperationException("Cannot set Default Style Type of Report."); }
        }

        [NotMapped]
        ReportElementId? IReportElement.StyleInheritanceElementId
        {
            get { return null; }
            set { throw new InvalidOperationException("Cannot set Style Inheritance of Report."); }
        }

        [NotMapped]
        int? IReportElement.StyleInheritanceElementNumber
        {
            get { return null; }
            set { throw new InvalidOperationException("Cannot set Style Inheritance of Report."); }
        }

        [NotMapped]
        IElementStyle IReportElement.ElementStyle
        {
            get { return ReportAreaStyle; }
        }

        IReportElement IReportElement.Copy()
        {
            return this.Copy();
        }

        bool IReportElement.Equals(IReportElement otherReportElement)
        {
            return this.Equals(otherReportElement);
        }

        RenderingResult IReportElement.Initialize(IReportingDataProvider dataProvider, IRenderingState renderingState)
        {
            throw new NotImplementedException();
        }

        RenderingResult IReportElement.Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree)
        {
            throw new NotImplementedException();
        }

        RenderingResult IReportElement.RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            throw new NotImplementedException();
        }

        RenderingResult IReportElement.RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IProjectMember<Report> Implementation

        public Report CopyForRevision(long newRevisionNumber)
        {
            if (this.Key.RevisionNumber >= newRevisionNumber)
            { throw new InvalidOperationException("A Report created for a new revision must have a greater revision number."); }

            return (this as IProjectMember<Report>).CopyForProject(this.Key.ProjectGuid, newRevisionNumber);
        }

        Report IProjectMember<Report>.CopyForProject(Guid projectGuid, long revisionNumber)
        {
            var newValue = this.Copy();

            newValue.EF_ProjectGuid = projectGuid;
            newValue.EF_RevisionNumber = revisionNumber;

            foreach (var newLayout in newValue.EF_DimensionLayouts)
            {
                newLayout.EF_ProjectGuid = projectGuid;
                newLayout.EF_RevisionNumber = revisionNumber;
            }
            foreach (var newStyle in newValue.EF_ElementStyles)
            {
                newStyle.EF_ProjectGuid = projectGuid;
                newStyle.EF_RevisionNumber = revisionNumber;
            }
            return newValue;
        }

        #endregion

        #region IPropertyDisplayDataOverrider Implementation

        bool IPropertyDisplayDataOverrider.Get_IsVisible_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.IsVisible;
        }

        bool IPropertyDisplayDataOverrider.Get_IsEditable_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.IsEditable;
        }

        Dictionary<string, string> IPropertyDisplayDataOverrider.Get_AvailableOptions_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.StructuralTypeComplexId))
            {
                var getAllStructuralTypes_Method = (Func<ICollection<KeyedOrderable<ModelObjectReference>>>)args[(int)PropertyDisplayData_ArgType.GetAllStructuralTypes_Method];
                var options = KeyedOrderableUtils.ConvertToOptionsList(getAllStructuralTypes_Method(), ConversionUtils.ItemPartSeparator_Override);
                return options;
            }
            else
            { return propertyAttribute.AvailableOptions; }
        }

        string IPropertyDisplayDataOverrider.Get_PopupData_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.PopupData;
        }

        #endregion

        #region Helper Methods

        protected IEditabilitySpecification GetEditabilitySpecForLayout(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return EditabilitySpec; }
            else if (dimension == Dimension.Y)
            { return EditabilitySpec; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        protected IEditabilitySpecification GetEditabilitySpecForStyle()
        {
            return null;
        }

        protected IDimensionLayout GetDefaultLayout(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return DefaultLayout_X; }
            else if (dimension == Dimension.Y)
            { return DefaultLayout_Y; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        protected IElementStyle GetDefaultStyle()
        {
            return null;
        }

        #endregion
    }
}