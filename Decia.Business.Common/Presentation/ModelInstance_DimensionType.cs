using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Presentation
{
    public enum ModelInstance_DimensionType
    {
        [Description("Model Instance")]
        ModelInstance,
        [Description("Structural Type")]
        StructuralType,
        [Description("Variable")]
        VariableTemplate,
        [Description("Structural")]
        StructuralInstance,
        [Description("Time (Primary)")]
        Time_D1,
        [Description("Time (Secondary)")]
        Time_D2,
        [Description("Placeholder")]
        Placeholder
    }

    public static class ModelInstance_DimensionTypeUtils
    {
        public const string Default_PlaceholderName = "";

        public static readonly IEnumerable<ModelInstance_DimensionType> Fixed_DimensionTypes_ForSheet_UsingRefs = new ModelInstance_DimensionType[] { ModelInstance_DimensionType.ModelInstance, ModelInstance_DimensionType.StructuralType };
        public static readonly IEnumerable<ModelInstance_DimensionType> Fixed_DimensionTypes_ForSheet_UsingPeriods = new ModelInstance_DimensionType[] { };
        public static readonly IEnumerable<ModelInstance_DimensionType> Floating_DimensionTypes_ForSheet_UsingRefs = new ModelInstance_DimensionType[] { ModelInstance_DimensionType.StructuralInstance, ModelInstance_DimensionType.VariableTemplate };
        public static readonly IEnumerable<ModelInstance_DimensionType> Floating_DimensionTypes_ForSheet_UsingPeriods = new ModelInstance_DimensionType[] { ModelInstance_DimensionType.Time_D1, ModelInstance_DimensionType.Time_D2 };

        public static readonly IEnumerable<ModelInstance_DimensionType> DimensionTypes_UsingRefs = Fixed_DimensionTypes_ForSheet_UsingRefs.Union(Floating_DimensionTypes_ForSheet_UsingRefs).ToList();
        public static readonly IEnumerable<ModelInstance_DimensionType> DimensionTypes_UsingPeriods = Fixed_DimensionTypes_ForSheet_UsingPeriods.Union(Floating_DimensionTypes_ForSheet_UsingPeriods).ToList();
        public static readonly IEnumerable<ModelInstance_DimensionType> Fixed_DimensionTypes_ForSheet = Fixed_DimensionTypes_ForSheet_UsingRefs.Union(Fixed_DimensionTypes_ForSheet_UsingPeriods).ToList();
        public static readonly IEnumerable<ModelInstance_DimensionType> Floating_DimensionTypes_ForSheet = Floating_DimensionTypes_ForSheet_UsingRefs.Union(Floating_DimensionTypes_ForSheet_UsingPeriods).ToList();

        public static bool GetIsEditable_OrderNumber(this ModelInstance_DimensionType dimensionType)
        {
            if (Fixed_DimensionTypes_ForSheet.Contains(dimensionType))
            { return false; }
            return true;
        }

        public static void AssertOrderNumberIsEditable(this ModelInstance_DimensionType dimensionType)
        {
            if (!GetIsEditable_OrderNumber(dimensionType))
            { throw new InvalidOperationException("Cannot edit the order for the given DimensionType."); }
        }

        public static int GetDefault_OrderNumber(this ModelInstance_DimensionType dimensionType)
        {
            if (Fixed_DimensionTypes_ForSheet.Contains(dimensionType))
            { return int.MinValue; }
            return (int)dimensionType;
        }

        public static bool GetDefault_IsConstrained(this ModelInstance_DimensionType dimensionType)
        {
            if (Fixed_DimensionTypes_ForSheet.Contains(dimensionType))
            { return true; }
            return false;
        }

        public static bool IsSheetDimension(this ModelInstance_DimensionType dimensionType)
        {
            return Floating_DimensionTypes_ForSheet.Contains(dimensionType);
        }

        public static void AssertIsSheetDimension(this ModelInstance_DimensionType dimensionType)
        {
            if (!IsSheetDimension(dimensionType))
            { throw new InvalidOperationException("The specified DimensionType is not a Worksheet Dimension."); }
        }

        public static bool IsRefBased(this ModelInstance_DimensionType dimensionType)
        {
            return DimensionTypes_UsingRefs.Contains(dimensionType);
        }

        public static void AssertIsRefBased(this ModelInstance_DimensionType dimensionType)
        {
            if (!IsRefBased(dimensionType))
            { throw new InvalidOperationException("The specified DimensionType is not Reference-based."); }
        }

        public static bool IsTimeBased(this ModelInstance_DimensionType dimensionType)
        {
            return DimensionTypes_UsingPeriods.Contains(dimensionType);
        }

        public static void AssertIsTimeBased(this ModelInstance_DimensionType dimensionType)
        {
            if (!IsTimeBased(dimensionType))
            { throw new InvalidOperationException("The specified DimensionType is not Time-based."); }
        }
    }
}