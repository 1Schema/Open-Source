using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Layouts
{
    public static class IDimensionLayoutUtils
    {
        public static readonly string Dimension_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.Dimension);
        public static readonly string RenderingMode_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.RenderingMode);
        public static readonly string DefaultLayout_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.DefaultLayout_Value);
        public static readonly string AlignmentMode_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.AlignmentMode_Value);
        public static readonly string SizeMode_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.SizeMode_Value);
        public static readonly string RangeSize_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.RangeSize_Value);
        public static readonly string RangeSize_Design_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.RangeSize_Design_Value);
        public static readonly string RangeSize_Production_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.RangeSize_Production_Value);
        public static readonly string ContainerMode_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.ContainerMode_Value);
        public static readonly string ContentGroup_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.ContentGroup_Value);
        public static readonly string MinRangeSizeInCells_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.MinRangeSizeInCells_Value);
        public static readonly string MaxRangeSizeInCells_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.MaxRangeSizeInCells_Value);
        public static readonly string Margin_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.Margin_Value);
        public static readonly string Padding_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.Padding_Value);
        public static readonly string CellSize_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.CellSize_Value);
        public static readonly string CellSize_Design_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.CellSize_Design_Value);
        public static readonly string CellSize_Production_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.CellSize_Production_Value);
        public static readonly string OverridePaddingCellSize_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.OverridePaddingCellSize_Value);
        public static readonly string MergeInteriorAreaCells_PropName = ClassReflector.GetPropertyName((IDimensionLayout x) => x.MergeInteriorAreaCells_Value);

        public static readonly Dimension Dimension_Default = Dimension.X;
        public static readonly RenderingMode RenderingMode_Default = RenderingMode.Design;
        public static readonly AlignmentMode AlignmentMode_Default = AlignmentMode.Lesser;
        public static readonly SizeMode SizeMode_Default = SizeMode.Cell;
        public static readonly bool RangeSize_CombineInputValues_Default = true;
        public static readonly int RangeSize_Min = 1;
        public static readonly int RangeSize_Max = int.MaxValue;
        public static readonly int RangeSize_Unconstrained = RangeSize_Max;
        public static readonly int RangeSize_Default = RangeSize_Min;
        public static readonly int RangeSize_Design_Default = RangeSize_Default;
        public static readonly int RangeSize_Production_Default = RangeSize_Default;
        public static readonly ContainerMode ContainerMode_Default = ContainerMode.Single;
        public static readonly int ContentGroup_Default = 0;
        public static readonly int MinRangeSizeInCells_Default = 1;
        public static readonly int MaxRangeSizeInCells_Default = RangeSize_Unconstrained;
        public static readonly int Margin_Min = int.MinValue;
        public static readonly int Margin_Max = int.MaxValue;
        public static readonly int Margin_Default = 0;
        public static readonly int Padding_Min = 0;
        public static readonly int Padding_Max = int.MaxValue;
        public static readonly int Padding_Default = Padding_Min;
        public static readonly bool CellSize_CombineInputValues_Default = true;
        public static readonly double CellSize_Min = 1;
        public static readonly double CellSize_Max = 1000;
        public static readonly double CellSize_DefaultWidth = 49;
        public static readonly double CellSize_DefaultHeight = 15;
        public static readonly bool OverridePaddingCellSize_Default = false;
        public static readonly bool MergeInteriorAreaCells_Default = false;

        public static bool Dimension_IsValid(this Dimension dimension)
        {
            return (dimension != Dimension.None);
        }

        public static void Dimension_AssertIsValid(this Dimension dimension)
        {
            if (!dimension.Dimension_IsValid())
            { throw new InvalidOperationException("The specified Dimension is not valid."); }
        }

        public static bool RangeSize_IsValid(this int rangeSize, SizeMode sizeMode, int minRangeSize, int maxRangeSize)
        {
            if (sizeMode != SizeMode.Cell)
            { return true; }

            if (rangeSize < RangeSize_Min)
            { return false; }
            if (rangeSize < minRangeSize)
            { return false; }
            if (rangeSize > maxRangeSize)
            { return false; }
            return true;
        }

        public static void RangeSize_AssertIsValid(this int rangeSize, SizeMode sizeMode, int minRangeSize, int maxRangeSize)
        {
            if (!rangeSize.RangeSize_IsValid(sizeMode, minRangeSize, maxRangeSize))
            { throw new InvalidOperationException("The specified Range Size Value is not valid."); }
        }

        public static bool MaxRangeSize_IsUnconstrained(this int maxRangeSize)
        {
            return (maxRangeSize >= IDimensionLayoutUtils.RangeSize_Unconstrained);
        }

        public static void MaxRangeSize_AssertIsUnconstrained(this int maxRangeSize)
        {
            if (!maxRangeSize.MaxRangeSize_IsUnconstrained())
            { throw new InvalidOperationException("The specified Max Range Size Value is not unconstrainted."); }
        }

        public static void MaxRangeSize_AssertIsNotUnconstrained(this int maxRangeSize)
        {
            if (maxRangeSize.MaxRangeSize_IsUnconstrained())
            { throw new InvalidOperationException("The specified Max Range Size Value is unconstrainted."); }
        }

        public static bool Margin_IsValid(this int margin)
        {
            if (margin < Margin_Min)
            { return false; }
            if (margin > Margin_Max)
            { return false; }
            return true;
        }

        public static void Margin_AssertIsValid(this int margin)
        {
            if (!margin.Margin_IsValid())
            { throw new InvalidOperationException("The specified Margin is not valid."); }
        }

        public static bool Padding_IsValid(this int padding)
        {
            if (padding < Padding_Min)
            { return false; }
            if (padding > Padding_Max)
            { return false; }
            return true;
        }

        public static void Padding_AssertIsValid(this int padding)
        {
            if (!padding.Padding_IsValid())
            { throw new InvalidOperationException("The specified Padding is not valid."); }
        }

        public static double CellSize_GetDefaultForDimension(this Dimension dimension)
        {
            dimension.Dimension_AssertIsValid();

            if ((dimension == Dimension.X) || (dimension == Dimension.Z1))
            { return CellSize_DefaultWidth; }
            else if ((dimension == Dimension.Y) || (dimension == Dimension.Z2))
            { return CellSize_DefaultHeight; }
            else
            { throw new InvalidOperationException("The specified Dimension does not have a Default Cell Size."); }
        }

        public static bool CellSize_IsValid(this double cellSize)
        {
            if (cellSize < CellSize_Min)
            { return false; }
            if (cellSize > CellSize_Max)
            { return false; }
            return true;
        }

        public static void CellSize_AssertIsValid(this double cellSize)
        {
            if (!cellSize.CellSize_IsValid())
            { throw new InvalidOperationException("The specified Cell Size is not valid."); }
        }
    }
}