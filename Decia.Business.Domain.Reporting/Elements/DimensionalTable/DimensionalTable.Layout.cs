using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;

namespace Decia.Business.Domain.Reporting
{
    public partial class DimensionalTable
    {
        #region Constants

        internal const int LeftGroupNumber = 1;
        internal const int RightGroupNumber = 2;
        internal const int TopGroupNumber = 1;
        internal const int BottomGroupNumber = 2;

        public static readonly IEditabilitySpecification EditabilitySpec = new DimensionalTableEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Auto,
            ContainerMode_Value = ContainerMode.Grid,
            MinRangeSizeInCells_Value = 2,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly DimensionLayout DefaultLayout_Y = new DimensionLayout(Dimension.Y)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Auto,
            ContainerMode_Value = ContainerMode.Grid,
            MinRangeSizeInCells_Value = 2,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly ElementStyle DefaultStyle = new ElementStyle()
        {
            BorderColor_Value = new BoxStyleValue<ColorSpecification>(new ColorSpecification(Color.Black)),
            BorderStyle_Value = new BoxStyleValue<BorderLineStyle>(BorderLineStyle.Medium),

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region DimensionalTableEditabilitySpecification Inner Class

        public class DimensionalTableEditabilitySpecification : IEditabilitySpecification
        {
            public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

            public bool IsEditable(string propertyName)
            {
                if (propertyName == IDimensionLayoutUtils.SizeMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.RangeSize_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.ContainerMode_PropName)
                { return false; }
                return true;
            }

            public bool IsValueValid(string propertyName, object proposedValue, IDictionary<string, object> currentValues)
            {
                if (!IsEditable(propertyName))
                { return false; }

                return true;
            }
        }

        #endregion
    }
}