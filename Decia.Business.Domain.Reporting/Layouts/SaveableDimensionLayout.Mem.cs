using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Domain.Reporting.Layouts
{
    public partial class SaveableDimensionLayout : IValueObject<SaveableDimensionLayout>
    {
        internal bool Default_CheckDefaultLayout_Setting = true;

        public static IEqualityComparer<SaveableDimensionLayout> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<SaveableDimensionLayout>(); }
        }

        IEqualityComparer<SaveableDimensionLayout> IValueObject<SaveableDimensionLayout>.ValueWiseComparer
        {
            get { return SaveableDimensionLayout.ValueWiseComparer; }
        }

        public virtual SaveableDimensionLayout Copy()
        {
            SaveableDimensionLayout otherObject = new SaveableDimensionLayout();
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual SaveableDimensionLayout CopyNew()
        {
            SaveableDimensionLayout otherObject = new SaveableDimensionLayout();
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public virtual void CopyTo(SaveableDimensionLayout otherObject)
        {
            this.CopyTo(otherObject, Default_CheckDefaultLayout_Setting);
        }

        public virtual void CopyTo(SaveableDimensionLayout otherObject, bool checkDefaultLayout)
        {
            if (otherObject.m_ParentReportElementId != m_ParentReportElementId)
            { otherObject.m_ParentReportElementId = m_ParentReportElementId; }

            this.CopyValuesTo(otherObject, checkDefaultLayout);
        }

        public virtual void CopyValuesTo(SaveableDimensionLayout otherObject)
        {
            this.CopyValuesTo(otherObject, Default_CheckDefaultLayout_Setting);
        }

        public virtual void CopyValuesTo(SaveableDimensionLayout otherObject, bool checkDefaultLayout)
        {
            if (otherObject.m_Dimension != m_Dimension)
            { otherObject.m_Dimension = m_Dimension; }
            if (otherObject.m_RenderingMode != m_RenderingMode)
            { otherObject.m_RenderingMode = m_RenderingMode; }
            if (otherObject.m_EditabilitySpec != m_EditabilitySpec)
            { otherObject.m_EditabilitySpec = m_EditabilitySpec; }

            if (otherObject.m_AlignmentMode != m_AlignmentMode)
            { otherObject.m_AlignmentMode = m_AlignmentMode; }
            if (otherObject.m_SizeMode != m_SizeMode)
            { otherObject.m_SizeMode = m_SizeMode; }
            if (otherObject.m_RangeSize_CombineInputValues != m_RangeSize_CombineInputValues)
            { otherObject.m_RangeSize_CombineInputValues = m_RangeSize_CombineInputValues; }
            if (otherObject.m_RangeSize_Design != m_RangeSize_Design)
            { otherObject.m_RangeSize_Design = m_RangeSize_Design; }
            if (otherObject.m_RangeSize_Production != m_RangeSize_Production)
            { otherObject.m_RangeSize_Production = m_RangeSize_Production; }
            if (otherObject.m_ContainerMode != m_ContainerMode)
            { otherObject.m_ContainerMode = m_ContainerMode; }
            if (otherObject.m_ContentGroup != m_ContentGroup)
            { otherObject.m_ContentGroup = m_ContentGroup; }
            if (otherObject.m_MinRangeSizeInCells != m_MinRangeSizeInCells)
            { otherObject.m_MinRangeSizeInCells = m_MinRangeSizeInCells; }
            if (otherObject.m_MaxRangeSizeInCells != m_MaxRangeSizeInCells)
            { otherObject.m_MaxRangeSizeInCells = m_MaxRangeSizeInCells; }
            if (otherObject.m_Margin != m_Margin)
            { otherObject.m_Margin = m_Margin; }
            if (otherObject.m_Padding != m_Padding)
            { otherObject.m_Padding = m_Padding; }
            if (otherObject.m_CellSize_CombineInputValues != m_CellSize_CombineInputValues)
            { otherObject.m_CellSize_CombineInputValues = m_CellSize_CombineInputValues; }
            if (otherObject.m_CellSize_Design != m_CellSize_Design)
            { otherObject.m_CellSize_Design = m_CellSize_Design; }
            if (otherObject.m_CellSize_Production != m_CellSize_Production)
            { otherObject.m_CellSize_Production = m_CellSize_Production; }
            if (otherObject.m_OverridePaddingCellSize != m_OverridePaddingCellSize)
            { otherObject.m_OverridePaddingCellSize = m_OverridePaddingCellSize; }
            if (otherObject.m_MergeInteriorAreaCells != m_MergeInteriorAreaCells)
            { otherObject.m_MergeInteriorAreaCells = m_MergeInteriorAreaCells; }

            if (!checkDefaultLayout)
            { return; }

            if (otherObject.Dimension != Dimension)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.AlignmentMode_Value != AlignmentMode_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.SizeMode_Value != SizeMode_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.RangeSize_Value != RangeSize_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.ContainerMode_Value != ContainerMode_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.ContentGroup_Value != ContentGroup_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.MinRangeSizeInCells_Value != MinRangeSizeInCells_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.MaxRangeSizeInCells_Value != MaxRangeSizeInCells_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.Margin_Value != Margin_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.Padding_Value != Padding_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.CellSize_Value != CellSize_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.OverridePaddingCellSize_Value != OverridePaddingCellSize_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
            if (otherObject.MergeInteriorAreaCells_Value != MergeInteriorAreaCells_Value)
            { otherObject.DefaultLayout_Value = DefaultLayout_Value; }
        }

        public virtual bool Equals(SaveableDimensionLayout otherObject)
        {
            return this.Equals(otherObject, Default_CheckDefaultLayout_Setting);
        }

        public virtual bool Equals(SaveableDimensionLayout otherObject, bool checkDefaultLayout)
        {
            if (otherObject.m_ParentReportElementId != m_ParentReportElementId)
            { return false; }

            return this.EqualsValues(otherObject, checkDefaultLayout);
        }

        public virtual bool EqualsValues(SaveableDimensionLayout otherObject)
        {
            return this.EqualsValues(otherObject, Default_CheckDefaultLayout_Setting);
        }

        public virtual bool EqualsValues(SaveableDimensionLayout otherObject, bool checkDefaultLayout)
        {
            if (otherObject.m_Dimension != m_Dimension)
            { return false; }
            if (otherObject.m_RenderingMode != m_RenderingMode)
            { return false; }
            if (otherObject.m_EditabilitySpec != m_EditabilitySpec)
            { return false; }

            if (otherObject.m_AlignmentMode != m_AlignmentMode)
            { return false; }
            if (otherObject.m_SizeMode != m_SizeMode)
            { return false; }
            if (otherObject.m_RangeSize_CombineInputValues != m_RangeSize_CombineInputValues)
            { return false; }
            if (otherObject.m_RangeSize_Design != m_RangeSize_Design)
            { return false; }
            if (otherObject.m_RangeSize_Production != m_RangeSize_Production)
            { return false; }
            if (otherObject.m_ContainerMode != m_ContainerMode)
            { return false; }
            if (otherObject.m_ContentGroup != m_ContentGroup)
            { return false; }
            if (otherObject.m_MinRangeSizeInCells != m_MinRangeSizeInCells)
            { return false; }
            if (otherObject.m_MaxRangeSizeInCells != m_MaxRangeSizeInCells)
            { return false; }
            if (otherObject.m_Margin != m_Margin)
            { return false; }
            if (otherObject.m_Padding != m_Padding)
            { return false; }
            if (otherObject.m_CellSize_CombineInputValues != m_CellSize_CombineInputValues)
            { return false; }
            if (otherObject.m_CellSize_Design != m_CellSize_Design)
            { return false; }
            if (otherObject.m_CellSize_Production != m_CellSize_Production)
            { return false; }
            if (otherObject.m_OverridePaddingCellSize != m_OverridePaddingCellSize)
            { return false; }
            if (otherObject.m_MergeInteriorAreaCells != m_MergeInteriorAreaCells)
            { return false; }

            if (!checkDefaultLayout)
            { return true; }

            if (otherObject.Dimension != Dimension)
            { return false; }
            if (otherObject.AlignmentMode_Value != AlignmentMode_Value)
            { return false; }
            if (otherObject.SizeMode_Value != SizeMode_Value)
            { return false; }
            if (otherObject.RangeSize_Value != RangeSize_Value)
            { return false; }
            if (otherObject.ContainerMode_Value != ContainerMode_Value)
            { return false; }
            if (otherObject.ContentGroup_Value != ContentGroup_Value)
            { return false; }
            if (otherObject.MinRangeSizeInCells_Value != MinRangeSizeInCells_Value)
            { return false; }
            if (otherObject.MaxRangeSizeInCells_Value != MaxRangeSizeInCells_Value)
            { return false; }
            if (otherObject.Margin_Value != Margin_Value)
            { return false; }
            if (otherObject.Padding_Value != Padding_Value)
            { return false; }
            if (otherObject.CellSize_Value != CellSize_Value)
            { return false; }
            if (otherObject.OverridePaddingCellSize_Value != OverridePaddingCellSize_Value)
            { return false; }
            if (otherObject.MergeInteriorAreaCells_Value != MergeInteriorAreaCells_Value)
            { return false; }
            return true;
        }

        public string GetPropertyName(Expression<Func<SaveableDimensionLayout, object>> propertyGetter)
        {
            return ClassReflector.GetPropertyName<SaveableDimensionLayout, object>(propertyGetter);
        }

        #region Object Overrides

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type objType = obj.GetType();
            Type thisType = typeof(SaveableDimensionLayout);

            if (!thisType.Equals(objType))
            { return false; }

            SaveableDimensionLayout typedObject = (SaveableDimensionLayout)obj;
            return Equals(typedObject);
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        #endregion
    }
}