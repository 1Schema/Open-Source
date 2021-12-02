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
    public partial class Container
    {
        #region Constants

        public static readonly IEditabilitySpecification EditabilitySpec = new NoOpEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Cell,
            RangeSize_Design_Value = 4,
            ContainerMode_Value = ContainerMode.Single,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly DimensionLayout DefaultLayout_Y = new DimensionLayout(Dimension.Y)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Cell,
            RangeSize_Design_Value = 4,
            ContainerMode_Value = ContainerMode.Grid,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly ElementStyle DefaultStyle = new ElementStyle()
        {
            BorderColor_Value = new BoxStyleValue<ColorSpecification>(new ColorSpecification(Color.Black)),
            BorderStyle_Value = new BoxStyleValue<BorderLineStyle>(BorderLineStyle.Thin),

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion
    }
}