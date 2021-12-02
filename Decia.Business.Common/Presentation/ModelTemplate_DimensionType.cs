using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Presentation
{
    public enum ModelTemplate_DimensionType
    {
        [Description("Model Template")]
        ModelTemplate,
        [Description("Base-Unit Type")]
        BaseUnitType,
        [Description("Structural Type")]
        StructuralType,
        [Description("Variable Template")]
        VariableTemplate,
        [Description("Output Template")]
        OutputTemplate,
        [Description("Output Element Template")]
        OutputElementTemplate,
        [Description("Property Information")]
        PropertyInfo
    }

    public static class ModelTemplate_DimensionTypeUtils
    {
        public const string Default_PropertyInfoName = "";

        public static readonly IEnumerable<ModelTemplate_DimensionType> Fixed_DimensionTypes_ForSheet_UsingRefs = new ModelTemplate_DimensionType[] { ModelTemplate_DimensionType.ModelTemplate, ModelTemplate_DimensionType.BaseUnitType, ModelTemplate_DimensionType.StructuralType, ModelTemplate_DimensionType.VariableTemplate, ModelTemplate_DimensionType.OutputTemplate, ModelTemplate_DimensionType.OutputElementTemplate };
        public static readonly IEnumerable<ModelTemplate_DimensionType> Fixed_DimensionTypes_ForSheet_UsingNames = new ModelTemplate_DimensionType[] { };
        public static readonly IEnumerable<ModelTemplate_DimensionType> Floating_DimensionTypes_ForSheet_UsingRefs = new ModelTemplate_DimensionType[] { };
        public static readonly IEnumerable<ModelTemplate_DimensionType> Floating_DimensionTypes_ForSheet_UsingNames = new ModelTemplate_DimensionType[] { ModelTemplate_DimensionType.PropertyInfo };

        public static readonly IEnumerable<ModelTemplate_DimensionType> DimensionTypes_UsingRefs = Fixed_DimensionTypes_ForSheet_UsingRefs.Union(Floating_DimensionTypes_ForSheet_UsingRefs).ToList();
        public static readonly IEnumerable<ModelTemplate_DimensionType> DimensionTypes_UsingNames = Fixed_DimensionTypes_ForSheet_UsingNames.Union(Floating_DimensionTypes_ForSheet_UsingNames).ToList();
        public static readonly IEnumerable<ModelTemplate_DimensionType> Fixed_DimensionTypes_ForSheet = Fixed_DimensionTypes_ForSheet_UsingRefs.Union(Fixed_DimensionTypes_ForSheet_UsingNames).ToList();
        public static readonly IEnumerable<ModelTemplate_DimensionType> Floating_DimensionTypes_ForSheet = Floating_DimensionTypes_ForSheet_UsingRefs.Union(Floating_DimensionTypes_ForSheet_UsingNames).ToList();

        public static bool GetIsEditable_OrderNumber(this ModelTemplate_DimensionType dimensionType)
        {
            if (Fixed_DimensionTypes_ForSheet.Contains(dimensionType))
            { return false; }
            return true;
        }

        public static void AssertOrderNumberIsEditable(this ModelTemplate_DimensionType dimensionType)
        {
            if (!GetIsEditable_OrderNumber(dimensionType))
            { throw new InvalidOperationException("Cannot edit the order for the given DimensionType."); }
        }

        public static int GetDefault_OrderNumber(this ModelTemplate_DimensionType dimensionType)
        {
            if (Fixed_DimensionTypes_ForSheet.Contains(dimensionType))
            { return int.MinValue; }
            return (int)dimensionType;
        }

        public static bool GetDefault_IsConstrained(this ModelTemplate_DimensionType dimensionType)
        {
            if (Fixed_DimensionTypes_ForSheet.Contains(dimensionType))
            { return true; }
            return false;
        }

        public static bool IsSheetDimension(this ModelTemplate_DimensionType dimensionType)
        {
            return Floating_DimensionTypes_ForSheet.Contains(dimensionType);
        }

        public static void AssertIsSheetDimension(this ModelTemplate_DimensionType dimensionType)
        {
            if (!IsSheetDimension(dimensionType))
            { throw new InvalidOperationException("The specified DimensionType is not a Worksheet Dimension."); }
        }

        public static bool IsRefBased(this ModelTemplate_DimensionType dimensionType)
        {
            return DimensionTypes_UsingRefs.Contains(dimensionType);
        }

        public static void AssertIsRefBased(this ModelTemplate_DimensionType dimensionType)
        {
            if (!IsRefBased(dimensionType))
            { throw new InvalidOperationException("The specified DimensionType is not Reference-based."); }
        }

        public static ModelTemplate_DimensionType GetDimensionType_ForObjectType(this ModelObjectType modelObjectType)
        {
            if (modelObjectType == ModelObjectType.ModelTemplate)
            { return ModelTemplate_DimensionType.ModelTemplate; }
            else if (modelObjectType == ModelObjectType.BaseUnitType)
            { return ModelTemplate_DimensionType.BaseUnitType; }
            else if (modelObjectType.IsStructuralType())
            { return ModelTemplate_DimensionType.StructuralType; }
            else if (modelObjectType == ModelObjectType.VariableTemplate)
            { return ModelTemplate_DimensionType.VariableTemplate; }
            else if (modelObjectType == ModelObjectType.ReportTemplate)
            { return ModelTemplate_DimensionType.OutputTemplate; }
            else if (modelObjectType == ModelObjectType.ReportElementTemplate)
            { return ModelTemplate_DimensionType.OutputElementTemplate; }
            else
            { throw new InvalidOperationException("The specified ModelObjectType does not correspond to a ModelTemplate_DimensionType."); }
        }

        public static bool IsNameBased(this ModelTemplate_DimensionType dimensionType)
        {
            return DimensionTypes_UsingNames.Contains(dimensionType);
        }

        public static void AssertIsNameBased(this ModelTemplate_DimensionType dimensionType)
        {
            if (!IsNameBased(dimensionType))
            { throw new InvalidOperationException("The specified DimensionType is not Time-based."); }
        }
    }
}