using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public partial class RowHeader
    {
        #region Constants

        internal const int Default_MinimumWidth = 3;
        internal const int Default_MinimumHeight = 1;

        public static readonly IEditabilitySpecification EditabilitySpec = new RowHeaderEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Stretch,
            SizeMode_Value = SizeMode.Auto,
            ContainerMode_Value = ContainerMode.Single,
            ContentGroup_Value = DimensionalTable.LeftGroupNumber,
            MinRangeSizeInCells_Value = Default_MinimumWidth,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly DimensionLayout DefaultLayout_Y = new DimensionLayout(Dimension.Y)
        {
            AlignmentMode_Value = AlignmentMode.Stretch,
            SizeMode_Value = SizeMode.Auto,
            ContainerMode_Value = ContainerMode.Single,
            ContentGroup_Value = DimensionalTable.BottomGroupNumber,
            MinRangeSizeInCells_Value = Default_MinimumHeight,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region RowHeaderEditabilitySpecification Inner Class

        public class RowHeaderEditabilitySpecification : IEditabilitySpecification
        {
            public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

            public bool IsEditable(string propertyName)
            {
                if (propertyName == IDimensionLayoutUtils.AlignmentMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.SizeMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.RangeSize_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.ContainerMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.ContentGroup_PropName)
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