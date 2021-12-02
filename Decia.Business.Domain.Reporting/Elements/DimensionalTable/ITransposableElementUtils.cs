using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public static class ITransposableElementUtils
    {
        public static readonly Dimension RowOriented_StackingDimension = Dimension.Y;
        public static readonly Dimension ColumnOriented_StackingDimension = Invert(RowOriented_StackingDimension);

        public const bool Default_IsTransposed = false;
        public static readonly Dimension Default_StackingDimension = GetStackingDimension(Default_IsTransposed);
        public static readonly Dimension Default_CommonDimension = Invert(Default_StackingDimension);

        public static readonly IEnumerable<Dimension> Valid_TableDimensions = DimensionUtils.ValidDimensions_2D;

        public static bool GetIsValidTableDimension(this Dimension dimension)
        {
            return DimensionUtils.GetIsValidDimension_2D(dimension);
        }

        public static void AssertIsValidTableDimension(this Dimension dimension)
        {
            if (!GetIsValidTableDimension(dimension))
            { throw new InvalidOperationException("The specified Dimension is not valid for DimensionalTables."); }
        }

        public static Dimension Invert(this Dimension dimension)
        {
            AssertIsValidTableDimension(dimension);
            return DimensionUtils.Invert_2D(dimension);
        }

        public static Dimension GetStackingDimension(this bool isTransposed)
        {
            return (!isTransposed) ? RowOriented_StackingDimension : ColumnOriented_StackingDimension;
        }

        public static Dimension GetCommonDimension(this bool isTransposed)
        {
            var stackingDimension = GetStackingDimension(isTransposed);
            return Invert(stackingDimension);
        }

        public static bool IsStackingDimensionTransposed(this Dimension stackingDimension)
        {
            AssertIsValidTableDimension(stackingDimension);
            var expectedIsTransposed = false;
            return (stackingDimension == GetStackingDimension(expectedIsTransposed)) ? expectedIsTransposed : !expectedIsTransposed;
        }

        public static bool IsCommonDimensionTransposed(this Dimension commonDimension)
        {
            AssertIsValidTableDimension(commonDimension);
            var expectedIsTransposed = false;
            return (commonDimension == GetCommonDimension(expectedIsTransposed)) ? expectedIsTransposed : !expectedIsTransposed;
        }

        internal static void SetStackingDimension(this ITransposableElement transposableElement, Dimension stackingDimension)
        {
            if (transposableElement is CommonTitleContainer)
            { (transposableElement as CommonTitleContainer).StackingDimension = stackingDimension; }
            else if (transposableElement is VariableTitleContainer)
            { (transposableElement as VariableTitleContainer).StackingDimension = stackingDimension; }
            else if (transposableElement is VariableDataContainer)
            { (transposableElement as VariableDataContainer).StackingDimension = stackingDimension; }
            else if (transposableElement is CommonTitleBox)
            { (transposableElement as CommonTitleBox).StackingDimension = stackingDimension; }
            else if (transposableElement is VariableTitleBox)
            { (transposableElement as VariableTitleBox).StackingDimension = stackingDimension; }
            else if (transposableElement is VariableDataBox)
            { (transposableElement as VariableDataBox).StackingDimension = stackingDimension; }
            else if (transposableElement is StructuralTitleRange)
            { (transposableElement as StructuralTitleRange).StackingDimension = stackingDimension; }
            else if (transposableElement is TimeTitleRange)
            { (transposableElement as TimeTitleRange).StackingDimension = stackingDimension; }
            else if (transposableElement is VariableTitleRange)
            { (transposableElement as VariableTitleRange).StackingDimension = stackingDimension; }
            else if (transposableElement is VariableDataRange)
            { (transposableElement as VariableDataRange).StackingDimension = stackingDimension; }
            else
            { throw new InvalidOperationException("Unexpected ITransposableElement encountered."); }
        }

        public static void TransposeElementLayoutAndStyle(this ITransposableElement transposableElement)
        {
            var elementLayout = transposableElement.ElementLayout;
            var elementStyle = transposableElement.ElementStyle;

            var xDimLayout = elementLayout.DimensionLayout_X;
            var yDimLayout = elementLayout.DimensionLayout_Y;
            var xDimEditSpec = xDimLayout.EditabilitySpec;
            var yDimEditSpec = yDimLayout.EditabilitySpec;

            var xContainerMode = xDimLayout.ContainerMode_Value;
            var yContainerMode = yDimLayout.ContainerMode_Value;
            var xContentGroup = xDimLayout.ContentGroup_Value;
            var yContentGroup = yDimLayout.ContentGroup_Value;
            var xMargin = xDimLayout.Margin_Value;
            var yMargin = yDimLayout.Margin_Value;
            var xPadding = xDimLayout.Padding_Value;
            var yPadding = yDimLayout.Padding_Value;
            var xBorderColor = new DimensionStyleValue<ColorSpecification>(elementStyle.BorderColor_Value.Left, elementStyle.BorderColor_Value.Right);
            var yBorderColor = new DimensionStyleValue<ColorSpecification>(elementStyle.BorderColor_Value.Top, elementStyle.BorderColor_Value.Bottom);
            var xBorderStyle = new DimensionStyleValue<BorderLineStyle>(elementStyle.BorderStyle_Value.Left, elementStyle.BorderStyle_Value.Right);
            var yBorderStyle = new DimensionStyleValue<BorderLineStyle>(elementStyle.BorderStyle_Value.Top, elementStyle.BorderStyle_Value.Bottom);

            if (xDimEditSpec is IOverridableEditabilitySpecification)
            { (xDimEditSpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.System; }
            if (yDimEditSpec is IOverridableEditabilitySpecification)
            { (yDimEditSpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.System; }

            if (xDimEditSpec.IsEditable(IDimensionLayoutUtils.ContainerMode_PropName))
            { xDimLayout.ContainerMode_Value = yContainerMode; }
            if (yDimEditSpec.IsEditable(IDimensionLayoutUtils.ContainerMode_PropName))
            { yDimLayout.ContainerMode_Value = xContainerMode; }

            xDimLayout.ContentGroup_Value = yContentGroup;
            yDimLayout.ContentGroup_Value = xContentGroup;
            xDimLayout.Margin_Value = yMargin;
            yDimLayout.Margin_Value = xMargin;
            xDimLayout.Padding_Value = yPadding;
            yDimLayout.Padding_Value = xPadding;
            elementStyle.BorderColor_Value = new BoxStyleValue<ColorSpecification>(yBorderColor.LesserSide, xBorderColor.LesserSide, yBorderColor.GreaterSide, xBorderColor.GreaterSide);
            elementStyle.BorderStyle_Value = new BoxStyleValue<BorderLineStyle>(yBorderStyle.LesserSide, xBorderStyle.LesserSide, yBorderStyle.GreaterSide, xBorderStyle.GreaterSide);

            if (xDimEditSpec is IOverridableEditabilitySpecification)
            { (xDimEditSpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.User; }
            if (yDimEditSpec is IOverridableEditabilitySpecification)
            { (yDimEditSpec as IOverridableEditabilitySpecification).CurrentMode = EditMode.User; }
        }
    }
}