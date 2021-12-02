using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Layouts
{
    public interface IDimensionLayout
    {
        #region State Methods

        SortedDictionary<string, object> GetCurrentPropertyValues();
        void ResetTo_DefaultLayout();

        #endregion

        #region State Properties

        Dimension Dimension { get; }
        RenderingMode RenderingMode { get; set; }

        IEditabilitySpecification EditabilitySpec { get; set; }
        bool DefaultLayout_HasValue { get; }
        IDimensionLayout DefaultLayout_Value { get; set; }

        #endregion

        #region Report Object Positioning Properties

        AlignmentMode AlignmentMode_Value { get; set; }
        bool AlignmentMode_IsEditable { get; }
        bool AlignmentMode_IsValid(AlignmentMode proposedValue);
        bool AlignmentMode_HasValue { get; }
        void AlignmentMode_ResetValue();

        SizeMode SizeMode_Value { get; set; }
        bool SizeMode_IsEditable { get; }
        bool SizeMode_IsValid(SizeMode proposedValue);
        bool SizeMode_HasValue { get; }
        void SizeMode_ResetValue();

        int RangeSize_Value { get; }
        bool RangeSize_HasValue { get; }
        bool RangeSize_CombineInputValues { get; set; }

        int RangeSize_Design_Value { get; set; }
        bool RangeSize_Design_IsEditable { get; }
        bool RangeSize_Design_IsValid(double proposedValue);
        bool RangeSize_Design_HasValue { get; }
        void RangeSize_Design_ResetValue();

        int RangeSize_Production_Value { get; set; }
        bool RangeSize_Production_IsEditable { get; }
        bool RangeSize_Production_IsValid(double proposedValue);
        bool RangeSize_Production_HasValue { get; }
        void RangeSize_Production_ResetValue();

        ContainerMode ContainerMode_Value { get; set; }
        bool ContainerMode_IsEditable { get; }
        bool ContainerMode_IsValid(ContainerMode proposedValue);
        bool ContainerMode_HasValue { get; }
        void ContainerMode_ResetValue();

        int ContentGroup_Value { get; set; }
        bool ContentGroup_IsEditable { get; }
        bool ContentGroup_IsValid(int proposedValue);
        bool ContentGroup_HasValue { get; }
        void ContentGroup_ResetValue();

        int MinRangeSizeInCells_Value { get; set; }
        bool MinRangeSizeInCells_IsEditable { get; }
        bool MinRangeSizeInCells_IsValid(int proposedValue);
        bool MinRangeSizeInCells_HasValue { get; }
        void MinRangeSizeInCells_ResetValue();

        int MaxRangeSizeInCells_Value { get; set; }
        bool MaxRangeSizeInCells_IsEditable { get; }
        bool MaxRangeSizeInCells_IsValid(int proposedValue);
        bool MaxRangeSizeInCells_HasValue { get; }
        bool MaxRangeSizeInCells_IsUnconstrained { get; }
        void MaxRangeSizeInCells_SetToUnconstrained();
        void MaxRangeSizeInCells_ResetValue();

        DimensionStyleValue<int> Margin_Value { get; set; }
        bool Margin_IsEditable { get; }
        bool Margin_IsValid(DimensionStyleValue<int> proposedValue);
        DimensionStyleValue<bool> Margin_HasValue { get; }
        void Margin_ResetValue();

        DimensionStyleValue<int> Padding_Value { get; set; }
        bool Padding_IsEditable { get; }
        bool Padding_IsValid(DimensionStyleValue<int> proposedValue);
        DimensionStyleValue<bool> Padding_HasValue { get; }
        void Padding_ResetValue();

        #endregion

        #region Excel Cell Rendering Properties

        double CellSize_Value { get; }
        bool CellSize_HasValue { get; }
        bool CellSize_CombineInputValues { get; set; }

        double CellSize_Design_Value { get; set; }
        bool CellSize_Design_IsEditable { get; }
        bool CellSize_Design_IsValid(double proposedValue);
        bool CellSize_Design_HasValue { get; }
        void CellSize_Design_ResetValue();

        double CellSize_Production_Value { get; set; }
        bool CellSize_Production_IsEditable { get; }
        bool CellSize_Production_IsValid(double proposedValue);
        bool CellSize_Production_HasValue { get; }
        void CellSize_Production_ResetValue();

        bool OverridePaddingCellSize_Value { get; set; }
        bool OverridePaddingCellSize_IsEditable { get; }
        bool OverridePaddingCellSize_IsValid(bool proposedValue);
        bool OverridePaddingCellSize_HasValue { get; }
        void OverridePaddingCellSize_ResetValue();

        bool MergeInteriorAreaCells_Value { get; set; }
        bool MergeInteriorAreaCells_IsEditable { get; }
        bool MergeInteriorAreaCells_IsValid(bool proposedValue);
        bool MergeInteriorAreaCells_HasValue { get; }
        void MergeInteriorAreaCells_ResetValue();

        #endregion
    }
}