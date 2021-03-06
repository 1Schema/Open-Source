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
    public partial class CommonTitleContainer
    {
        #region Constants

        public static readonly CommonTitleContainerEditabilitySpecification EditabilitySpec = new CommonTitleContainerEditabilitySpecification();

        public static readonly DimensionLayout DefaultLayout_X = new DimensionLayout(Dimension.X)
        {
            AlignmentMode_Value = AlignmentMode.Stretch,
            SizeMode_Value = SizeMode.Auto,
            ContainerMode_Value = (ITransposableElementUtils.Default_CommonDimension == Dimension.X) ? ContainerMode.Grid : ContainerMode.Single,
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
            ContainerMode_Value = (ITransposableElementUtils.Default_CommonDimension == Dimension.Y) ? ContainerMode.Grid : ContainerMode.Single,
            MinRangeSizeInCells_Value = 1,
            Margin_Value = new DimensionStyleValue<int>(0, 0),
            Padding_Value = new DimensionStyleValue<int>(0, 0),

            OverridePaddingCellSize_Value = true,
            MergeInteriorAreaCells_Value = false,

            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        public static readonly ElementStyle DefaultStyle = new ElementStyle()
        {
            BackColor_Value = new ColorSpecification(Color.MistyRose),

            DefaultStyle_Value = Container.DefaultStyle,
            EditabilitySpec = new ReadOnlyEditabilitySpecification()
        };

        #endregion

        #region CommonTitleContainerEditabilitySpecification Inner Class

        public class CommonTitleContainerEditabilitySpecification : IOverridableEditabilitySpecification
        {
            internal const EditMode DefaultMode = IEditabilitySpecificationUtils.DefaultEditMode;

            public CommonTitleContainerEditabilitySpecification()
            { (this as IOverridableEditabilitySpecification).CurrentMode = DefaultMode; }

            EditMode IOverridableEditabilitySpecification.CurrentMode { get; set; }

            public EditMode CurrentMode { get { return (this as IOverridableEditabilitySpecification).CurrentMode; } }

            public bool IsEditable(string propertyName)
            {
                if (propertyName == IDimensionLayoutUtils.AlignmentMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.SizeMode_PropName)
                { return false; }
                if (propertyName == IDimensionLayoutUtils.RangeSize_PropName)
                { return false; }
                if ((this as IOverridableEditabilitySpecification).CurrentMode != EditMode.System)
                {
                    if (propertyName == IDimensionLayoutUtils.ContainerMode_PropName)
                    { return false; }
                    if (propertyName == IDimensionLayoutUtils.ContentGroup_PropName)
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