using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public partial class Cell
    {
        #region Constants

        public static readonly IEditabilitySpecification EditabilitySpec = new CellEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Cell,
            RangeSize_Design_Value = 1,
            ContainerMode_Value = ContainerMode.Single,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = true,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly DimensionLayout DefaultLayout_Y = new DimensionLayout(Dimension.Y)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Cell,
            RangeSize_Design_Value = 1,
            ContainerMode_Value = ContainerMode.Single,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = true,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region CellEditabilitySpecification Inner Class

        public class CellEditabilitySpecification : IEditabilitySpecification
        {
            public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

            public bool IsEditable(string propertyName)
            {
                return (propertyName != IDimensionLayoutUtils.ContainerMode_PropName);
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