using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public partial class TimeTitleRange
    {
        #region Constants

        public static readonly TimeTitleRangeEditabilitySpecification EditabilitySpec = new TimeTitleRangeEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Cell,
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
            ContainerMode_Value = ContainerMode.Single,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region TimeTitleRangeEditabilitySpecification Inner Class

        public class TimeTitleRangeEditabilitySpecification : IEditabilitySpecification
        {
            public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

            public bool IsEditable(string propertyName)
            {
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