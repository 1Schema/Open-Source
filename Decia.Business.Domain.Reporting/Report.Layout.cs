using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting
{
    public partial class Report
    {
        #region Constants

        public static readonly IEditabilitySpecification EditabilitySpec = new ReportEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Auto,
            RangeSize_Design_Value = 1,
            ContainerMode_Value = ContainerMode.Single,
            MinRangeSizeInCells_Value = 10,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = true,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly DimensionLayout DefaultLayout_Y = new DimensionLayout(Dimension.Y)
        {
            AlignmentMode_Value = AlignmentMode.Lesser,
            SizeMode_Value = SizeMode.Auto,
            RangeSize_Design_Value = 1,
            ContainerMode_Value = ContainerMode.Grid,
            MinRangeSizeInCells_Value = 10,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = true,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region ReportEditabilitySpecification Inner Class

        public class ReportEditabilitySpecification : IEditabilitySpecification
        {
            public EditMode CurrentMode { get { return IEditabilitySpecificationUtils.DefaultEditMode; } }

            public bool IsEditable(string propertyName)
            {
                if (propertyName == IDimensionLayoutUtils.ContentGroup_PropName)
                { return false; }
                return true;
            }

            public bool IsValueValid(string propertyName, object proposedValue, IDictionary<string, object> currentValues)
            {
                if (!IsEditable(propertyName))
                { return false; }

                if (propertyName == IDimensionLayoutUtils.SizeMode_PropName)
                {
                    if (proposedValue == null)
                    { return false; }
                    if ((SizeMode)proposedValue == SizeMode.Ratio)
                    { return false; }
                }
                return true;
            }
        }

        #endregion
    }
}