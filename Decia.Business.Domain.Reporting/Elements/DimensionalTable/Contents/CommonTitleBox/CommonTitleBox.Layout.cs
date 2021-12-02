using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public partial class CommonTitleBox
    {
        #region Constants

        public static readonly CommonTitleRangeEditabilitySpecification EditabilitySpec = new CommonTitleRangeEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Stretch,
            SizeMode_Value = SizeMode.Auto,
            ContainerMode_Value = ContainerMode.Grid,
            MinRangeSizeInCells_Value = 1,
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
            ContainerMode_Value = ContainerMode.Grid,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region CommonTitleRangeEditabilitySpecification Inner Class

        public class CommonTitleRangeEditabilitySpecification : IOverridableEditabilitySpecification
        {
            internal const EditMode DefaultMode = IEditabilitySpecificationUtils.DefaultEditMode;

            public CommonTitleRangeEditabilitySpecification()
            { (this as IOverridableEditabilitySpecification).CurrentMode = DefaultMode; }

            EditMode IOverridableEditabilitySpecification.CurrentMode { get; set; }

            public EditMode CurrentMode { get { return (this as IOverridableEditabilitySpecification).CurrentMode; } }

            public bool IsEditable(string propertyName)
            {
                if (propertyName == IDimensionLayoutUtils.AlignmentMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.SizeMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.ContainerMode_PropName)
                { return false; }
                if ((this as IOverridableEditabilitySpecification).CurrentMode != EditMode.System)
                {
                    if (propertyName == IDimensionLayoutUtils.ContentGroup_PropName)
                    { return false; }
                    if (propertyName == IDimensionLayoutUtils.Margin_PropName)
                    { return false; }
                    if (propertyName == IDimensionLayoutUtils.Padding_PropName)
                    { return false; }
                }
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