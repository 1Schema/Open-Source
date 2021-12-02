using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Presentation;
using Decia.Business.Common.Styling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Rendering;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public abstract partial class ReportElementBase<KDO> : ModelDomainObjectBase_DeleteableOrderable<ReportElementId, KDO>, IReportElement<KDO>, IPropertyDisplayDataOverrider_WithValue
        where KDO : class, IKeyedDomainObject<ReportElementId, KDO>, IReportElement<KDO>, IPermissionable
    {
        #region Static Members

        public static readonly bool ShowDebugText = Report.ShowDebugText;

        public static readonly string NameFormat = "New {0}";
        public static readonly int Default_ZOrder = 0;

        #endregion

        #region Members

        protected string m_Name;

        protected Nullable<int> m_ParentElementNumber;
        protected bool? m_IsLocked;
        protected int m_ZOrder;

        protected bool? m_IsParentEditable;
        protected bool? m_IsDirectlyDeletable;

        protected SaveableDimensionLayout m_DimensionLayout_X;
        protected SaveableDimensionLayout m_DimensionLayout_Y;

        protected KnownStyleType m_DefaultStyleType;
        protected Nullable<int> m_StyleInheritanceElementNumber;
        protected SaveableElementStyle m_ElementStyle;

        #endregion

        #region Constructors

        public ReportElementBase()
            : this(ReportId.DefaultId)
        { }

        public ReportElementBase(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public ReportElementBase(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, null)
        {
            int elementNumber = Guid.NewGuid().GetHashCode();
            while (Report.Report_AllElementNumbers.Contains(elementNumber))
            { elementNumber = Guid.NewGuid().GetHashCode(); ;}

            m_Key = new ReportElementId(projectGuid, revisionNumber, modelTemplateNumber, reportGuid, elementNumber);
            m_Name = string.Format(NameFormat, ReportElementType.GetNameForEnumValue());

            m_ParentElementNumber = null;
            m_IsLocked = ReportElementType.IsLocked_Default();
            m_ZOrder = Default_ZOrder;

            m_IsParentEditable = ReportElementType.IsParentEditable_Default();
            m_IsDirectlyDeletable = ReportElementType.IsDirectlyDeletable_Default();

            m_DimensionLayout_X = new SaveableDimensionLayout(m_Key, Dimension.X, GetEditabilitySpecForLayout(Dimension.X));
            m_DimensionLayout_Y = new SaveableDimensionLayout(m_Key, Dimension.Y, GetEditabilitySpecForLayout(Dimension.Y));
            m_DimensionLayout_X.DefaultLayout_Value = GetDefaultLayout(Dimension.X);
            m_DimensionLayout_Y.DefaultLayout_Value = GetDefaultLayout(Dimension.Y);

            m_DefaultStyleType = KnownStyleType.None;
            m_StyleInheritanceElementNumber = null;

            m_ElementStyle = new SaveableElementStyle(m_Key, GetEditabilitySpecForStyle());
            m_ElementStyle.DefaultStyle_Value = GetDefaultStyle();
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

        #region Abstract Methods

        protected abstract ReportElementType_New GetReportElementType();

        protected abstract IEditabilitySpecification GetEditabilitySpecForLayout(Dimension dimension);
        protected abstract IEditabilitySpecification GetEditabilitySpecForStyle();

        protected abstract IDimensionLayout GetDefaultLayout(Dimension dimension);
        protected abstract IElementStyle GetDefaultStyle();

        #endregion

        #region IReportElement Implementation

        public override string ToString()
        {
            if (!ShowDebugText)
            { return base.ToString(); }
            else
            { return Name; }
        }

        [NotMapped]
        public int ElementNumber
        {
            get { return this.Key.ReportElementNumber; }
        }

        [NotMapped]
        [PropertyDisplayData("Name", "Text that identifies the ReportElement in the system.", true, EditorType.Input, OrderNumber_NonNull = 1, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public string Name
        {
            get { return SpecialCharacterConverter.ApplyEscapeCodesIfNecessary(m_Name); }
            set
            {
                ReportElementType.AssertNameIsValid(value);
                m_Name = value;
            }
        }

        [NotMapped]
        [PropertyDisplayData("Sorting Number", "If specified, determines value on which to sort.", true, EditorType.Input, OrderNumber_NonNull = 3, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public new Nullable<long> OrderNumber
        {
            get { return base.OrderNumber; }
            set { base.OrderNumber = value; }
        }

        [NotMapped]
        public ModelObjectReference ReportElementTypeRef
        {
            get { return new ModelObjectReference(ModelObjectType.ReportElementTemplate, m_Key.ReportElementNumber); }
        }

        [NotMapped]
        [PropertyDisplayData("Element Type", "What type of Report Element is the current selection?", false, EditorType.Input, OrderNumber_NonNull = 5, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public ReportElementType_New ReportElementType
        {
            get { return GetReportElementType(); }
        }

        [NotMapped]
        public bool HasDataBoundMultiplicity
        {
            get { return ReportElementType.HasDataBoundMultiplicity(); }
        }

        [NotMapped]
        [PropertyDisplayData("Parent Report", "What Report does the current selection belong to?", false, EditorType.Input, OrderNumber_NonNull = 6, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public ReportId ParentReportId
        {
            get { return m_Key.ReportId; }
        }

        [NotMapped]
        public Guid ParentReportGuid
        {
            get { return m_Key.ReportGuid; }
        }

        [NotMapped]
        [PropertyDisplayData("Parent Element", "What Report does the current selection belong to?", false, EditorType.Input, OrderNumber_NonNull = 7, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public Nullable<ReportElementId> ParentElementId
        {
            get
            {
                if (!m_ParentElementNumber.HasValue)
                { return null; }

                return new ReportElementId(m_Key.ReportId, m_ParentElementNumber.Value, false);
            }
            set
            {
                if (!value.HasValue)
                { m_ParentElementNumber = null; }
                else
                {
                    if (m_Key.ReportGuid != value.Value.ReportGuid)
                    { throw new InvalidOperationException("Cannot set Report Element to reference parent in a different Report."); }

                    m_ParentElementNumber = value.Value.ReportElementNumber;
                }
            }
        }

        [NotMapped]
        public Nullable<int> ParentElementNumber
        {
            get { return m_ParentElementNumber; }
            set { m_ParentElementNumber = value; }
        }

        [NotMapped]
        public ReportElementId ParentElementOrReportId
        {
            get
            {
                if (!ParentElementId.HasValue)
                { return ReportElementId.GetElementId_ReportLevel(ParentReportId); }
                return ParentElementId.Value;
            }
        }

        [NotMapped]
        public int ParentElementOrReportNumber
        {
            get { return ParentElementOrReportId.ReportElementNumber; }
        }

        [NotMapped]
        [PropertyDisplayData("Is Locked", "Is the ReportTemplate locked for edits.", true, EditorType.Input, OrderNumber_NonNull = 8, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public bool IsLocked
        {
            get
            {
                var isLocked = (m_IsLocked.HasValue) ? m_IsLocked.Value : ReportElementType.IsLocked_Default();
                return isLocked;
            }
            set
            {
                ReportElementType.AssertIsUnlockable();
                m_IsLocked = value;
            }
        }

        [NotMapped]
        [PropertyDisplayData("Z-Order", "What is the stacking order of this element relative to its siblings?", true, EditorType.Input, OrderNumber_NonNull = 19, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public int ZOrder
        {
            get { return m_ZOrder; }
            set { m_ZOrder = value; }
        }

        [NotMapped]
        public bool IsParentEditable
        {
            get
            {
                if (IsLocked)
                { return false; }

                var isParentEditable = (m_IsParentEditable.HasValue) ? m_IsParentEditable.Value : ReportElementType.IsParentEditable_Default();
                return isParentEditable;
            }
            set
            {
                ReportElementType.AssertCanEditParentEditability();
                m_IsParentEditable = value;
            }
        }

        [NotMapped]
        public bool IsDirectlyDeletable
        {
            get
            {
                if (IsLocked)
                { return false; }

                var isDirectlyDeletable = (m_IsDirectlyDeletable.HasValue) ? m_IsDirectlyDeletable.Value : ReportElementType.IsDirectlyDeletable_Default();
                return isDirectlyDeletable;
            }
            set
            {
                ReportElementType.AssertCanEditDeletability();
                m_IsDirectlyDeletable = value;
            }
        }

        [NotMapped]
        public bool IsContainer
        {
            get { return ReportElementType.IsContainer(); }
        }

        [NotMapped]
        public bool AutoDeletesChildren
        {
            get { return ReportElementType.AutoDeletesChildren(); }
        }

        [NotMapped]
        public bool AreContentsEditable
        {
            get { return ReportElementType.AreContentsEditable(); }
        }

        [NotMapped]
        public ICollection<ReportElementType_New> AcceptableContentTypes
        {
            get { return ReportElementType.GetAcceptableContentTypes(); }
        }

        [NotMapped]
        public bool IsPositionEditable
        {
            get { return ReportElementType.IsEditable_Positioning(); }
        }

        [NotMapped]
        bool IReportElement.IsSizeEditable
        {
            get { return ReportElementType.IsEditable_Sizing(); }
        }

        [NotMapped]
        public bool RequiresDelayedSizing
        {
            get { return ReportElementType.RequiresDelayedSizing(); }
        }

        [NotMapped]
        bool IReportElement.IsDirectlyTransposable
        {
            get { return ReportElementType.IsDirectlyTransposable(); }
        }

        [NotMapped]
        public bool IsStyleEditable
        {
            get { return ReportElementType.IsEditable_Style(); }
        }

        [NotMapped]
        [PropertyDisplayData("Element Layout", "The flexible layout values that determine the position and size of the Element.", true, PopupType.FlexibleLayout, OrderNumber_NonNull = 20, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public string ReportAreaBoxPosition
        {
            get
            {
                var leftMargin = m_DimensionLayout_X.Margin_Value.LesserSide;
                var rightMargin = m_DimensionLayout_X.Margin_Value.GreaterSide;
                var desiredWidth = m_DimensionLayout_X.RangeSize_Design_Value;

                var topMargin = m_DimensionLayout_Y.Margin_Value.LesserSide;
                var bottomMargin = m_DimensionLayout_Y.Margin_Value.GreaterSide;
                var desiredHeight = m_DimensionLayout_Y.RangeSize_Design_Value;

                var margins = new BoxStyleValue<int>(leftMargin, topMargin, rightMargin, bottomMargin);
                var size = new SizeT<int>(desiredWidth, desiredHeight);

                var layoutAsText = margins.ToString() + ConversionUtils.ListItemSeparator + size.ToString();
                return layoutAsText;
            }
        }

        [NotMapped]
        public virtual IElementLayout ElementLayout
        {
            get { return new ElementLayout(IsContainer, m_DimensionLayout_X, m_DimensionLayout_Y); }
        }

        [NotMapped]
        [PropertyDisplayData("Default Style", "Should this element use a default Style?", true, EditorType.Select, typeof(KnownStyleType), OrderNumber_NonNull = 21, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public KnownStyleType DefaultStyleType
        {
            get { return m_DefaultStyleType; }
            set { m_DefaultStyleType = value; }
        }

        [NotMapped]
        public Nullable<ReportElementId> StyleInheritanceElementId
        {
            get
            {
                if (!m_StyleInheritanceElementNumber.HasValue)
                { return null; }

                return new ReportElementId(m_Key.ReportId, m_StyleInheritanceElementNumber.Value, false);
            }
            set
            {
                if (!value.HasValue)
                { m_StyleInheritanceElementNumber = null; }
                else
                {
                    if (m_Key.ReportGuid != value.Value.ReportGuid)
                    { throw new InvalidOperationException("Cannot set Report Element to reference Style of Element in a different Report."); }

                    m_StyleInheritanceElementNumber = value.Value.ReportElementNumber;
                }
            }
        }

        [NotMapped]
        public Nullable<int> StyleInheritanceElementNumber
        {
            get { return m_StyleInheritanceElementNumber; }
            set { m_StyleInheritanceElementNumber = value; }
        }

        [NotMapped]
        [PropertyDisplayData("Element Style", "The style for the Element.", true, PopupType.Style, OrderNumber_NonNull = 22, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner)]
        public IElementStyle ElementStyle
        {
            get { return m_ElementStyle; }
        }

        IReportElement IReportElement.Copy()
        {
            return this.Copy();
        }

        bool IReportElement.Equals(IReportElement otherReportElement)
        {
            return this.Equals(otherReportElement);
        }

        public virtual RenderingResult Initialize(IReportingDataProvider dataProvider, IRenderingState renderingState)
        {
            ICurrentState reportState = renderingState.ReportState;
            RenderingResult result = new RenderingResult(reportState.ModelTemplateRef, reportState.NullableModelInstanceRef, this.Key, ProcessingAcivityType.Initialization);

            if (!renderingState.ReportElements.ContainsKey(this.Key))
            {
                result.SetErrorState(RenderingResultType.ReportElementNotFound, "The current Report Element does not exist in the Report being rendered.");
                return result;
            }
            if (renderingState.ReportElements[this.Key] != this)
            {
                result.SetErrorState(RenderingResultType.ReportElementNotFound, "The current Report Element does not exist in the Report being rendered.");
                return result;
            }
            if (!renderingState.Report.Key.Equals_Revisionless(this.ParentReportId))
            {
                result.SetErrorState(RenderingResultType.ParentReportDoesNotMatch, "The current Report Element does not exist in the Report being rendered.");
                return result;
            }
            if (this.ParentElementId.HasValue)
            {
                if (!renderingState.ReportElements.ContainsKey(this.ParentElementId.Value))
                {
                    result.SetErrorState(RenderingResultType.ParentReportElementNotFound, "The Report being rendered does not contain the current Report Element's parent Element.");
                    return result;
                }
            }

            result.SetInitializedState();
            return result;
        }

        public abstract RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree);
        public abstract RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey);
        public abstract RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey);

        #endregion

        #region IProjectMember<KDO> Implementation

        public KDO CopyForRevision(long newRevisionNumber)
        {
            if (this.Key.RevisionNumber >= newRevisionNumber)
            { throw new InvalidOperationException("A ReportElement created for a new revision must have a greater revision number."); }

            return (this as IProjectMember<KDO>).CopyForProject(this.Key.ProjectGuid, newRevisionNumber);
        }

        KDO IProjectMember<KDO>.CopyForProject(Guid projectGuid, long revisionNumber)
        {
            var newValue = (this.Copy() as ReportElementBase<KDO>);

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
            return (newValue as KDO);
        }

        #endregion

        #region IProjectMember_Cloneable<T> Implementation

        public virtual KDO CopyAsNew(long revisionNumber)
        {
            var newValue = (this.Copy() as ReportElementBase<KDO>);
            int elementNumber = Guid.NewGuid().GetHashCode();

            newValue.EF_ReportElementNumber = elementNumber;
            newValue.EF_RevisionNumber = revisionNumber;

            foreach (var newLayout in newValue.EF_DimensionLayouts)
            {
                newLayout.EF_ReportElementNumber = elementNumber;
                newLayout.EF_RevisionNumber = revisionNumber;
            }
            foreach (var newStyle in newValue.EF_ElementStyles)
            {
                newStyle.EF_ReportElementNumber = elementNumber;
                newStyle.EF_RevisionNumber = revisionNumber;
            }
            return (newValue as KDO);
        }

        #endregion

        #region IPropertyDisplayDataOverrider_WithValue Implementation

        public virtual bool Get_IsVisible_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.IsVisible;
        }

        public virtual bool Get_IsEditable_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.IsEditable;
        }

        public virtual object Get_Value_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ReportElementType))
            {
                return EnumUtils.GetDescription<ReportElementType_New>(this.ReportElementType);
            }
            else if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ParentReportId))
            {
                var reportDataObject = (reportDataObjectAsObj as IReportDataObject);

                if (reportDataObject.ReportGuid != this.ParentReportGuid)
                { throw new InvalidOperationException("An incorrent Report Data Object was provided."); }

                return reportDataObject.Report.Name;
            }
            else if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ParentElementId))
            {
                if (!this.ParentElementId.HasValue)
                { return string.Empty; }

                var reportDataObject = (reportDataObjectAsObj as IReportDataObject);
                return reportDataObject.ReportElementsByRef[this.ParentElementId.Value.ModelObjectRef].Name;
            }
            else
            {
                object rawValue = propertyAttribute.PropertyInfo.GetValue(this, null);
                return rawValue;
            }
        }

        public virtual Dictionary<string, string> Get_AvailableOptions_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.AvailableOptions;
        }

        public virtual string Get_PopupData_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            return propertyAttribute.PopupData;
        }

        #endregion
    }
}