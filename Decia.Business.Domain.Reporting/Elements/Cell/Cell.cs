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
    public partial class Cell : ValueRangeBase<Cell>, ICell
    {
        #region Constructors

        public Cell()
            : this(ReportId.DefaultId)
        { }

        public Cell(ReportId reportId)
            : this(reportId.ProjectGuid, reportId.RevisionNumber_NonNull, reportId.ModelTemplateNumber, reportId.ReportGuid)
        { }

        public Cell(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid reportGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, reportGuid)
        { }

        #endregion

        #region Abstract Method Implementations

        protected override ReportElementType_New GetReportElementType()
        {
            return ReportElementType_New.Cell;
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
        { return false; }

        protected override string GetCustom_RefName(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        { return null; }

        protected override bool UsesCustom_RefValue()
        { return false; }

        protected override object GetCustom_RefValue(IReportingDataProvider dataProvider, IRenderingState renderingState, RenderedLayout resultingLayout)
        { return null; }

        #endregion

        #region Properties

        [NotMapped]
        [PropertyDisplayData("Reference", "What is the Reference used for the Cell contents?", true, EditorType.Select, OrderNumber_NonNull = 14, SupportedViewTypes = PropertyHost_ViewType.ReportDesigner, RefreshOnUpdate = true)]
        public string ObjectRefAsComplexId
        {
            get
            {
                if (!HasRefValue)
                { return string.Empty; }

                var complexId = ConversionUtils.ConvertComplexIdToString(RefValue.ModelObjectType, RefValue.ModelObjectId, ConversionUtils.ItemPartSeparator_Override);
                return complexId;
            }
            set
            {
                var complexId = value;
                var objectRef = ConversionUtils.ConvertStringToModelObjectRef(complexId, ConversionUtils.ItemPartSeparator_Override);
                SetToRefValue(objectRef);
            }
        }

        #endregion

        #region IPropertyDisplayDataOverrider_WithValue Overrides

        public override bool Get_IsVisible_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ObjectRefAsComplexId))
            { return (propertyAttribute.IsVisible && (this.OutputValueType == OutputValueType.ReferencedId)); }
            else
            { return base.Get_IsVisible_ForProperty(propertyAttribute, modelDataProviderAsObj, reportDataObjectAsObj, args); }
        }

        public override Dictionary<string, string> Get_AvailableOptions_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ObjectRefAsComplexId))
            {
                var modelDataProvider = (modelDataProviderAsObj as IFormulaDataProvider);
                var reportDataObject = (reportDataObjectAsObj as IReportDataObject);
                var getAllStructuralTypes_Method = (Func<ICollection<KeyedOrderable<ModelObjectReference>>>)args[(int)PropertyDisplayData_ArgType.GetAllStructuralTypes_Method];

                var rootReport = reportDataObject.Report;
                var rootStructuralTypeRef = rootReport.StructuralTypeRef;
                var structuralTypes = getAllStructuralTypes_Method();
                var optionsList = new List<KeyedOrderable<ModelObjectReference>>();

                var reportUsesPrimaryTimeDim = (rootReport.TimeDimensionUsages.ContainsKey(TimeDimensionType.Primary) && rootReport.TimeDimensionUsages[TimeDimensionType.Primary].HasValue);
                var reportUsesSecondaryTimeDim = (rootReport.TimeDimensionUsages.ContainsKey(TimeDimensionType.Secondary) && rootReport.TimeDimensionUsages[TimeDimensionType.Secondary].HasValue);

                foreach (var structuralType in structuralTypes.OrderBy(x => x.OrderNumber))
                {
                    if (!modelDataProvider.StructuralMap.IsUnique(rootStructuralTypeRef, structuralType.Key, true))
                    { continue; }

                    var variableTemplateRefsForTypeRef = modelDataProvider.DependencyMap.GetVariableTemplates(structuralType.Key);
                    var variableTemplateRefsForTypeAndTime = new List<ModelObjectReference>();

                    foreach (var variableTemplateRef in variableTemplateRefsForTypeRef)
                    {
                        var timeDimensionality = modelDataProvider.GetAssessedTimeDimensions(variableTemplateRef);

                        if (timeDimensionality.PrimaryTimeDimension.HasTimeValue && !reportUsesPrimaryTimeDim)
                        { continue; }
                        if (timeDimensionality.SecondaryTimeDimension.HasTimeValue && !reportUsesSecondaryTimeDim)
                        { continue; }

                        variableTemplateRefsForTypeAndTime.Add(variableTemplateRef);
                    }

                    var variableTemplateDescs = variableTemplateRefsForTypeAndTime.Select(x => modelDataProvider.GetObjectDescriptor(x)).OrderBy(x => x.OrderNumber).ToList();
                    variableTemplateDescs = variableTemplateDescs.Select(x => new ModelObjectDescriptor(x.Key, x.OrderNumber, string.Format(VariableNameDisplayFormat, structuralType.Name, x.Name), x.Tag)).ToList();

                    optionsList.AddRange(variableTemplateDescs);
                }

                var options = KeyedOrderableUtils.ConvertToOptionsList(optionsList, ConversionUtils.ItemPartSeparator_Override);
                return options;
            }
            else
            { return base.Get_AvailableOptions_ForProperty(propertyAttribute, modelDataProviderAsObj, reportDataObjectAsObj, args); }
        }

        public override object Get_Value_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (propertyAttribute.PropertyInfo.Name == ClassReflector.GetPropertyName(() => this.ObjectRefAsComplexId))
            {
                var modelDataProvider = (modelDataProviderAsObj as IFormulaDataProvider);
                ModelObjectReference structuralTypeRef, variableTemplateRef;
                bool areRefsValid;

                var isVarTempRef = IsObjectRef_VariableTemplate(this, modelDataProvider, out structuralTypeRef, out variableTemplateRef, out areRefsValid);
                if (!isVarTempRef)
                { throw new InvalidOperationException("The specified reference is not supported."); }

                var variableTemplateComplexId = ConversionUtils.ConvertComplexIdToString(variableTemplateRef.ModelObjectType, variableTemplateRef.ModelObjectId, ConversionUtils.ItemPartSeparator_Override);
                return variableTemplateComplexId;
            }
            else
            { return base.Get_Value_ForProperty(propertyAttribute, modelDataProviderAsObj, reportDataObjectAsObj, args); }
        }

        #endregion
    }
}