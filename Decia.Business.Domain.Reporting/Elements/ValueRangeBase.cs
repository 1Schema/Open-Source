using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Presentation;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Styling;
using Decia.Business.Common.Time;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Layouts;
using Decia.Business.Domain.Reporting.Rendering;
using Decia.Business.Domain.Reporting.Styling;

namespace Decia.Business.Domain.Reporting
{
    public abstract partial class ValueRangeBase<KDO> : ReportElementBase<KDO>, IValueRange_Configurable
        where KDO : class, IKeyedDomainObject<ReportElementId, KDO>, IReportElement<KDO>, IPermissionable, IValueRange_Configurable
    {
        public const string VariableNameDisplayFormat = "{0} - {1}";
        public static readonly OutputValueType Default_OutputValueType = OutputValueType.DirectValue;
        public static readonly DeciaDataType Default_DataType = DeciaDataType.Text;

        #region Members

        protected OutputValueType m_OutputValueType;
        protected DynamicValue m_DirectValue;
        protected Nullable<ModelObjectReference> m_RefValue;
        protected Nullable<Guid> m_ValueFormulaGuid;

        #endregion

        #region Constructors

        public ValueRangeBase()
            : this(ReportId.DefaultId)
        { }

        public ValueRangeBase(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public ValueRangeBase(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        {
            m_OutputValueType = Default_OutputValueType;
            m_DirectValue = new DynamicValue(Default_DataType);
            m_RefValue = null;
            m_ValueFormulaGuid = null;
        }

        #endregion

        #region Abstract Methods

        protected abstract bool UsesCustom_RefName();
        protected abstract string GetCustom_RefName(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout);
        protected abstract bool UsesCustom_RefValue();
        protected abstract object GetCustom_RefValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout);

        #endregion

        #region IValueRange_Configurable Implementation

        [NotMapped]
        [PropertyDisplayData("Value Type", "The source of the rendered value of the Range.", true, EditorType.Select, typeof(OutputValueType), OrderNumber_NonNull = 12, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public OutputValueType OutputValueType
        {
            get { return m_OutputValueType; }
        }

        [NotMapped]
        [PropertyDisplayData("Text Value", "What is the text contained in the Cell?", true, EditorType.Input, OrderNumber_NonNull = 13, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public DynamicValue DirectValue
        {
            get
            {
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { throw new InvalidOperationException("The specified Report Element value is in an invalid state."); }

                return m_DirectValue;
            }
        }

        [NotMapped]
        public virtual bool HasRefValue
        {
            get { return m_RefValue.HasValue; }
        }

        [NotMapped]
        public virtual ModelObjectReference RefValue
        {
            get
            {
                if (!m_RefValue.HasValue)
                { throw new InvalidOperationException("The specified Report Element value is in an invalid state."); }

                return m_RefValue.Value;
            }
        }

        [NotMapped]
        public bool HasValueFormula
        {
            get { return m_ValueFormulaGuid.HasValue; }
        }

        [NotMapped]
        public FormulaId ValueFormulaId
        {
            get
            {
                if (!m_ValueFormulaGuid.HasValue)
                { throw new InvalidOperationException("The specified Report Element value is in an invalid state."); }

                return new FormulaId(m_Key.ProjectGuid, m_Key.RevisionNumber_NonNull, m_ValueFormulaGuid.Value);
            }
        }

        [NotMapped]
        [PropertyDisplayData("Formula", "What is the Formula for the Cell contents?", true, PopupType.Formula, OrderNumber_NonNull = 15, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public Guid ValueFormulaGuid
        {
            get { return ValueFormulaId.FormulaGuid; }
        }

        public void SetToDirectValue()
        {
            m_OutputValueType = OutputValueType.DirectValue;
            if (m_DirectValue == (object)null)
            { m_DirectValue = new DynamicValue(Default_DataType); }
        }

        public void SetToDirectValue(object value)
        {
            m_OutputValueType = OutputValueType.DirectValue;
            m_DirectValue.SetValue(Default_DataType, value);
        }

        public void SetToRefValue()
        {
            if (!HasRefValue)
            { SetToRefValue(ModelObjectReference.GlobalTypeReference); }
            else
            { m_OutputValueType = OutputValueType.ReferencedId; }
        }

        public virtual void SetToRefValue(ModelObjectReference reference)
        {
            m_OutputValueType = OutputValueType.ReferencedId;
            m_RefValue = reference;
        }

        public void SetToRefValue(ModelObjectReference reference, Nullable<int> alternateDimensionNumber)
        {
            ModelObjectReference alternateRef = new ModelObjectReference(reference, alternateDimensionNumber);
            SetToRefValue(alternateRef);
        }

        public void SetToRefValue(ModelObjectType referencedType, int referencedNumber)
        {
            ModelObjectReference reference = new ModelObjectReference(referencedType, referencedNumber);
            SetToRefValue(reference);
        }

        public void SetToRefValue(ModelObjectType referencedType, int referencedNumber, Nullable<int> alternateDimensionNumber)
        {
            ModelObjectReference reference = new ModelObjectReference(referencedType, referencedNumber, alternateDimensionNumber);
            SetToRefValue(reference);
        }

        public void SetToValueFormula()
        {
            if (!HasValueFormula)
            { throw new InvalidOperationException("Cannot set to use null Formula"); }

            m_OutputValueType = OutputValueType.AnonymousFormula;
        }

        public void SetToValueFormula(FormulaId formulaId)
        {
            if (!formulaId.ProjectMemberId.Equals_Revisionless(Key.ProjectMemberId))
            { throw new InvalidOperationException("The Formula provided does not match the Project and Revision of the Cell."); }

            m_OutputValueType = OutputValueType.AnonymousFormula;
            m_ValueFormulaGuid = formulaId.FormulaGuid;
        }

        public void ClearValueFormula()
        {
            if (m_OutputValueType == OutputValueType.AnonymousFormula)
            { throw new InvalidOperationException("Cannot clear Formula while it is in use."); }
            m_ValueFormulaGuid = null;
        }

        [NotMapped]
        public ICollection<FormulaId> ContainedFormulaIds
        {
            get { return ContainedFormulaGuids.Select(x => new FormulaId(Key.ProjectGuid, Key.RevisionNumber_NonNull, x)).ToHashSet(); }
        }

        [NotMapped]
        public ICollection<Guid> ContainedFormulaGuids
        {
            get
            {
                var formulaGuids = new HashSet<Guid>();

                if (m_ValueFormulaGuid.HasValue)
                { formulaGuids.Add(m_ValueFormulaGuid.Value); }

                return formulaGuids;
            }
        }

        [NotMapped]
        public ICollection<FormulaId> ActiveFormulaIds
        {
            get { return ActiveFormulaGuids.Select(x => new FormulaId(Key.ProjectGuid, Key.RevisionNumber_NonNull, x)).ToHashSet(); }
        }

        [NotMapped]
        public ICollection<Guid> ActiveFormulaGuids
        {
            get
            {
                var formulaGuids = new HashSet<Guid>();

                if (OutputValueType == OutputValueType.AnonymousFormula)
                { formulaGuids.Add(m_ValueFormulaGuid.Value); }

                return formulaGuids;
            }
        }

        public bool TryGetDesignValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        {
            var reportState = renderingState.ReportState;
            var repeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];

            if (this.OutputValueType == OutputValueType.DirectValue)
            {
                resultingLayout.Value = this.DirectValue.GetValue();
            }
            else if (this.OutputValueType == OutputValueType.ReferencedId)
            {
                if (UsesCustom_RefName())
                {
                    var objectName = GetCustom_RefName(dataProvider, renderingState, resultingLayout);
                    resultingLayout.Value = objectName;
                }
                else
                {
                    ModelObjectReference structuralTypeRef, variableTemplateRef;
                    bool areRefsValid;

                    var isVarTempRef = IsObjectRef_VariableTemplate(this, dataProvider, out structuralTypeRef, out variableTemplateRef, out areRefsValid);
                    if (!isVarTempRef)
                    { throw new InvalidOperationException("The specified reference is not supported."); }

                    var isValid = dataProvider.GetValueRange_IsValid(renderingState, variableTemplateRef, resultingLayout.StructuralSpace);
                    if (!isValid)
                    { return false; }

                    var objectName = string.Format(VariableNameDisplayFormat, dataProvider.GetObjectName(structuralTypeRef), dataProvider.GetObjectName(variableTemplateRef));
                    resultingLayout.Value = objectName;
                }
            }
            else if (this.OutputValueType == OutputValueType.AnonymousFormula)
            {
                var isValid = dataProvider.GetValueRange_IsValid(renderingState, this.ValueFormulaId, reportState.StructuralTypeRef, resultingLayout.StructuralSpace);
                if (!isValid)
                { return false; }

                var anonymousFormula = dataProvider.GetAnonymousFormula(this.ValueFormulaId);
                var anonymousFormulaAsText = anonymousFormula.RenderAsString(dataProvider, reportState);
                resultingLayout.Value = anonymousFormulaAsText;
            }
            else
            { throw new InvalidOperationException("Unrecognized OutputValueType encountered."); }
            return true;
        }

        public bool TryGetProductionValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        {
            var reportState = renderingState.ReportState;
            var repeatGroup = renderingState.GroupingState.ElementRepeatGroups[this.Key];

            if (this.OutputValueType == OutputValueType.DirectValue)
            {
                resultingLayout.Value = this.DirectValue.GetValue();
            }
            else if (this.OutputValueType == OutputValueType.ReferencedId)
            {
                if (UsesCustom_RefValue())
                {
                    var objectValue = GetCustom_RefValue(dataProvider, renderingState, resultingLayout);
                    resultingLayout.Value = objectValue;
                }
                else
                {
                    ModelObjectReference structuralTypeRef, variableTemplateRef;
                    bool areRefsValid;

                    var isVarTempRef = IsObjectRef_VariableTemplate(this, dataProvider, out structuralTypeRef, out variableTemplateRef, out areRefsValid);
                    if (!isVarTempRef)
                    { throw new InvalidOperationException("The specified reference is not supported."); }

                    var modelInstanceRef = reportState.ModelInstanceRef;
                    var rootStructuralInstanceRef = reportState.StructuralInstanceRef;
                    var structuralPoint = resultingLayout.StructuralPoint.Value;
                    var timeKey = resultingLayout.TimeKey;

                    var objectValue = dataProvider.GetValueRange_Value(renderingState, variableTemplateRef, modelInstanceRef, structuralPoint, timeKey);
                    resultingLayout.Value = objectValue.GetValue();
                }
            }
            else if (this.OutputValueType == OutputValueType.AnonymousFormula)
            {
                var anonymousFormulaId = this.ValueFormulaId;
                var modelInstanceRef = reportState.ModelInstanceRef;
                var rootStructuralInstanceRef = reportState.StructuralInstanceRef;
                var structuralPoint = resultingLayout.StructuralPoint.Value;
                var timeKey = resultingLayout.TimeKey;

                var computedValue = dataProvider.GetValueRange_Value(renderingState, anonymousFormulaId, modelInstanceRef, rootStructuralInstanceRef, structuralPoint, timeKey);
                var computedText = computedValue.GetTypedValue<string>();

                var resultText = computedText;
                resultingLayout.Value = resultText;
            }
            else
            { throw new InvalidOperationException("Unrecognized OutputValueType encountered."); }
            return true;
        }

        #endregion

        #region IPropertyDisplayDataOverrider_WithValue Overrides

        public override bool Get_IsVisible_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.DirectValue))
            { return (propertyAttribute.IsVisible && (this.OutputValueType == OutputValueType.DirectValue)); }
            else if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ValueFormulaGuid))
            { return (propertyAttribute.IsVisible && (this.OutputValueType == OutputValueType.AnonymousFormula)); }
            else
            { return base.Get_IsVisible_ForProperty(propertyAttribute, modelDataProviderAsObj, reportDataObjectAsObj, args); }
        }

        #endregion

        #region Helper Methods

        public static bool IsObjectRef_VariableTemplate(IValueRange_Configurable valueRange, IFormulaDataProvider dataProvider, out ModelObjectReference structuralTypeRef, out ModelObjectReference variableTemplateRef, out bool areRefsValid)
        {
            if (!valueRange.HasRefValue || valueRange.RefValue.IsStructuralType())
            {
                structuralTypeRef = valueRange.HasRefValue ? valueRange.RefValue : ModelObjectReference.GlobalTypeReference;
                areRefsValid = dataProvider.StructuralMap.StructuralTypeRefs.Contains(structuralTypeRef);
                variableTemplateRef = areRefsValid ? dataProvider.DependencyMap.GetNameVariableTemplate(structuralTypeRef) : ModelObjectReference.EmptyReference;
                return true;
            }
            else if (valueRange.RefValue.ModelObjectType == ModelObjectType.VariableTemplate)
            {
                variableTemplateRef = valueRange.RefValue;
                areRefsValid = dataProvider.DependencyMap.VariableTemplateRefs.Contains(variableTemplateRef);
                structuralTypeRef = areRefsValid ? dataProvider.DependencyMap.GetStructuralType(variableTemplateRef) : ModelObjectReference.EmptyReference;
                return true;
            }

            structuralTypeRef = ModelObjectReference.EmptyReference;
            variableTemplateRef = ModelObjectReference.EmptyReference;
            areRefsValid = false;
            return false;
        }

        #endregion
    }
}