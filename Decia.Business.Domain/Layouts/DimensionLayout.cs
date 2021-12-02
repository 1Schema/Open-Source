using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using DLU = Decia.Business.Domain.Layouts.IDimensionLayoutUtils;

namespace Decia.Business.Domain.Layouts
{
    public class DimensionLayout : IDimensionLayout
    {
        public const string NotEditableExceptionFormat = "The {0} property is not editable.";
        public const string InvalidValueExceptionFormat = "The {0} value provided is not valid.";
        public const string InvalidRenderingModeMessage = "The specified RenderingMode is not valid.";

        protected Dimension m_Dimension;
        protected RenderingMode m_RenderingMode;

        protected IEditabilitySpecification m_EditabilitySpec;
        protected IDimensionLayout m_DefaultLayout;

        protected Nullable<AlignmentMode> m_AlignmentMode;
        protected Nullable<SizeMode> m_SizeMode;
        protected bool m_RangeSize_CombineInputValues;
        protected Nullable<int> m_RangeSize_Design;
        protected Nullable<int> m_RangeSize_Production;
        protected Nullable<ContainerMode> m_ContainerMode;
        protected Nullable<int> m_ContentGroup;
        protected Nullable<int> m_MinRangeSizeInCells;
        protected Nullable<int> m_MaxRangeSizeInCells;
        protected DimensionStyleValue<Nullable<int>> m_Margin;
        protected DimensionStyleValue<Nullable<int>> m_Padding;

        protected bool m_CellSize_CombineInputValues;
        protected Nullable<double> m_CellSize_Design;
        protected Nullable<double> m_CellSize_Production;
        protected Nullable<bool> m_OverridePaddingCellSize;
        protected Nullable<bool> m_MergeInteriorAreaCells;

        public DimensionLayout(Dimension dimension)
            : this(dimension, new NoOpEditabilitySpecification())
        { }

        public DimensionLayout(Dimension dimension, IEditabilitySpecification editabilitySpec)
        {
            dimension.Dimension_AssertIsValid();

            m_Dimension = dimension;
            m_RenderingMode = IDimensionLayoutUtils.RenderingMode_Default;

            m_EditabilitySpec = (editabilitySpec != null) ? editabilitySpec : new NoOpEditabilitySpecification();
            m_DefaultLayout = null;

            ResetTo_DefaultLayout();
        }

        public DimensionLayout(DimensionLayout layoutToCopy)
            : this(layoutToCopy, layoutToCopy.m_EditabilitySpec)
        { }

        public DimensionLayout(DimensionLayout layoutToCopy, IEditabilitySpecification editabilitySpec)
        {
            layoutToCopy.m_Dimension.Dimension_AssertIsValid();

            m_Dimension = layoutToCopy.m_Dimension;
            m_RenderingMode = layoutToCopy.m_RenderingMode;

            m_EditabilitySpec = (editabilitySpec != null) ? editabilitySpec : new NoOpEditabilitySpecification();
            m_DefaultLayout = layoutToCopy.m_DefaultLayout;

            m_AlignmentMode = layoutToCopy.m_AlignmentMode;
            m_SizeMode = layoutToCopy.m_SizeMode;
            m_RangeSize_CombineInputValues = layoutToCopy.m_RangeSize_CombineInputValues;
            m_RangeSize_Design = layoutToCopy.m_RangeSize_Design;
            m_RangeSize_Production = layoutToCopy.m_RangeSize_Production;
            m_ContainerMode = layoutToCopy.m_ContainerMode;
            m_ContentGroup = layoutToCopy.m_ContentGroup;
            m_MinRangeSizeInCells = layoutToCopy.m_MinRangeSizeInCells;
            m_MaxRangeSizeInCells = layoutToCopy.m_MaxRangeSizeInCells;
            m_Margin = layoutToCopy.m_Margin;
            m_Padding = layoutToCopy.m_Padding;

            m_CellSize_CombineInputValues = layoutToCopy.m_CellSize_CombineInputValues;
            m_CellSize_Design = layoutToCopy.m_CellSize_Design;
            m_CellSize_Production = layoutToCopy.m_CellSize_Production;
            m_OverridePaddingCellSize = layoutToCopy.m_OverridePaddingCellSize;
            m_MergeInteriorAreaCells = layoutToCopy.m_MergeInteriorAreaCells;
        }

        #region State Methods

        public virtual SortedDictionary<string, object> GetCurrentPropertyValues()
        {
            SortedDictionary<string, object> propertyValues = new SortedDictionary<string, object>();

            propertyValues.Add(DLU.Dimension_PropName, Dimension);
            propertyValues.Add(DLU.RenderingMode_PropName, RenderingMode);
            propertyValues.Add(DLU.DefaultLayout_PropName, DefaultLayout_Value);

            propertyValues.Add(DLU.AlignmentMode_PropName, AlignmentMode_Value);
            propertyValues.Add(DLU.SizeMode_PropName, SizeMode_Value);
            propertyValues.Add(DLU.RangeSize_PropName, RangeSize_Value);
            propertyValues.Add(DLU.ContainerMode_PropName, ContainerMode_Value);
            propertyValues.Add(DLU.ContentGroup_PropName, ContentGroup_Value);
            propertyValues.Add(DLU.MinRangeSizeInCells_PropName, MinRangeSizeInCells_Value);
            propertyValues.Add(DLU.MaxRangeSizeInCells_PropName, MaxRangeSizeInCells_Value);
            propertyValues.Add(DLU.Margin_PropName, Margin_Value);
            propertyValues.Add(DLU.Padding_PropName, Padding_Value);

            propertyValues.Add(DLU.CellSize_PropName, CellSize_Value);
            propertyValues.Add(DLU.OverridePaddingCellSize_PropName, OverridePaddingCellSize_Value);
            propertyValues.Add(DLU.MergeInteriorAreaCells_PropName, MergeInteriorAreaCells_Value);

            return propertyValues;
        }

        public void ResetTo_DefaultLayout()
        {
            m_AlignmentMode = null;
            m_SizeMode = null;
            m_RangeSize_CombineInputValues = IDimensionLayoutUtils.RangeSize_CombineInputValues_Default;
            m_RangeSize_Design = null;
            m_RangeSize_Production = null;
            m_ContainerMode = null;
            m_ContentGroup = null;
            m_MinRangeSizeInCells = null;
            m_MaxRangeSizeInCells = null;
            m_Margin = new DimensionStyleValue<Nullable<int>>(null, null);
            m_Padding = new DimensionStyleValue<Nullable<int>>(null, null);

            m_CellSize_CombineInputValues = IDimensionLayoutUtils.CellSize_CombineInputValues_Default;
            m_CellSize_Design = null;
            m_CellSize_Production = null;
            m_OverridePaddingCellSize = null;
            m_MergeInteriorAreaCells = null;
        }

        #endregion

        #region State Properties

        [NotMapped]
        public Dimension Dimension
        {
            get { return m_Dimension; }
        }

        [NotMapped]
        public RenderingMode RenderingMode
        {
            get { return m_RenderingMode; }
            set { m_RenderingMode = value; }
        }

        [NotMapped]
        public IEditabilitySpecification EditabilitySpec
        {
            get { return m_EditabilitySpec; }
            set
            {
                if (value == null)
                { value = new NoOpEditabilitySpecification(); }

                m_EditabilitySpec = value;
            }
        }

        [NotMapped]
        public bool DefaultLayout_HasValue
        {
            get { return (m_DefaultLayout != null); }
        }

        [NotMapped]
        public IDimensionLayout DefaultLayout_Value
        {
            get { return m_DefaultLayout; }
            set { m_DefaultLayout = value; }
        }

        #endregion

        #region Report Object Positioning Properties

        [NotMapped]
        public bool AlignmentMode_IsEditable
        {
            get
            {
                var propertyName = DLU.AlignmentMode_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool AlignmentMode_IsValid(AlignmentMode proposedValue)
        {
            var propertyName = DLU.AlignmentMode_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool AlignmentMode_HasValue
        {
            get { return m_AlignmentMode.HasValue; }
        }

        [NotMapped]
        public AlignmentMode AlignmentMode_Value
        {
            get
            {
                if (!AlignmentMode_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.AlignmentMode_Default; }
                    else
                    { return DefaultLayout_Value.AlignmentMode_Value; }
                }

                return m_AlignmentMode.Value;
            }
            set
            {
                if (!AlignmentMode_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.AlignmentMode_PropName)); }
                if (!AlignmentMode_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.AlignmentMode_PropName)); }

                m_AlignmentMode = value;
            }
        }

        public void AlignmentMode_ResetValue()
        { m_AlignmentMode = null; }

        [NotMapped]
        public bool SizeMode_IsEditable
        {
            get
            {
                var propertyName = DLU.SizeMode_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool SizeMode_IsValid(SizeMode proposedValue)
        {
            var propertyName = DLU.SizeMode_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool SizeMode_HasValue
        {
            get { return m_SizeMode.HasValue; }
        }

        [NotMapped]
        public SizeMode SizeMode_Value
        {
            get
            {
                if (!SizeMode_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.SizeMode_Default; }
                    else
                    { return DefaultLayout_Value.SizeMode_Value; }
                }

                return m_SizeMode.Value;
            }
            set
            {
                if (!SizeMode_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.SizeMode_PropName)); }
                if (!SizeMode_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.SizeMode_PropName)); }

                m_SizeMode = value;
                ReValidate_RangeSize_Design();
                ReValidate_RangeSize_Production();
            }
        }

        public void SizeMode_ResetValue()
        {
            m_SizeMode = null;
            ReValidate_RangeSize_Design();
            ReValidate_RangeSize_Production();
        }

        [NotMapped]
        public int RangeSize_Value
        {
            get
            {
                if (RenderingMode == RenderingMode.Design)
                { return RangeSize_Design_Value; }
                else if (RenderingMode == RenderingMode.Production)
                { return RangeSize_Production_Value; }
                else
                { throw new InvalidOperationException(InvalidRenderingModeMessage); }
            }
        }

        [NotMapped]
        public bool RangeSize_HasValue
        {
            get
            {
                if (RenderingMode == RenderingMode.Design)
                { return RangeSize_Design_HasValue; }
                else if (RenderingMode == RenderingMode.Production)
                { return RangeSize_Production_HasValue; }
                else
                { throw new InvalidOperationException(InvalidRenderingModeMessage); }
            }
        }

        [NotMapped]
        public bool RangeSize_CombineInputValues
        {
            get { return m_RangeSize_CombineInputValues; }
            set { m_RangeSize_CombineInputValues = value; }
        }

        [NotMapped]
        public bool RangeSize_Design_IsEditable
        {
            get
            {
                var propertyName = DLU.RangeSize_Design_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool RangeSize_Design_IsValid(double proposedValue)
        {
            var propertyName = DLU.RangeSize_Design_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool RangeSize_Design_HasValue
        {
            get { return m_RangeSize_Design.HasValue; }
        }

        [NotMapped]
        public int RangeSize_Design_Value
        {
            get
            {
                if (!RangeSize_Design_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.RangeSize_Design_Default; }
                    else
                    { return DefaultLayout_Value.RangeSize_Design_Value; }
                }

                return m_RangeSize_Design.Value;
            }
            set
            {
                if (!RangeSize_Design_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.RangeSize_Design_PropName)); }
                if (!RangeSize_Design_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.RangeSize_Design_PropName)); }

                value.RangeSize_AssertIsValid(SizeMode_Value, MinRangeSizeInCells_Value, MaxRangeSizeInCells_Value);

                m_RangeSize_Design = value;
                ReValidate_RangeSize_Design();
            }
        }

        public void RangeSize_Design_ResetValue()
        {
            m_RangeSize_Design = null;
            ReValidate_RangeSize_Design();
        }

        protected virtual void ReValidate_RangeSize_Design()
        {
            int curr = RangeSize_Design_Value;
            SizeMode sizeMode = SizeMode_Value;
            int min = MinRangeSizeInCells_Value;
            int max = MaxRangeSizeInCells_Value;

            if (!curr.RangeSize_IsValid(sizeMode, min, max))
            {
                if (curr < min)
                { RangeSize_Design_Value = min; }
                else
                { RangeSize_Design_Value = max; }
            }
        }

        [NotMapped]
        public bool RangeSize_Production_IsEditable
        {
            get
            {
                var propertyName = DLU.RangeSize_Production_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool RangeSize_Production_IsValid(double proposedValue)
        {
            var propertyName = DLU.RangeSize_Production_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool RangeSize_Production_HasValue
        {
            get { return m_RangeSize_Production.HasValue; }
        }

        [NotMapped]
        public int RangeSize_Production_Value
        {
            get
            {
                if (RangeSize_CombineInputValues)
                { return RangeSize_Design_Value; }

                if (!RangeSize_Production_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.RangeSize_Production_Default; }
                    else
                    { return DefaultLayout_Value.RangeSize_Production_Value; }
                }

                return m_RangeSize_Production.Value;
            }
            set
            {
                if (!RangeSize_Production_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.RangeSize_Production_PropName)); }
                if (!RangeSize_Production_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.RangeSize_Production_PropName)); }

                value.RangeSize_AssertIsValid(SizeMode_Value, MinRangeSizeInCells_Value, MaxRangeSizeInCells_Value);

                m_RangeSize_Production = value;
                ReValidate_RangeSize_Production();
            }
        }

        public void RangeSize_Production_ResetValue()
        {
            m_RangeSize_Production = null;
            ReValidate_RangeSize_Production();
        }

        protected virtual void ReValidate_RangeSize_Production()
        {
            int curr = RangeSize_Production_Value;
            SizeMode sizeMode = SizeMode_Value;
            int min = MinRangeSizeInCells_Value;
            int max = MaxRangeSizeInCells_Value;

            if (!curr.RangeSize_IsValid(sizeMode, min, max))
            {
                if (curr < min)
                { RangeSize_Production_Value = min; }
                else
                { RangeSize_Production_Value = max; }
            }
        }

        [NotMapped]
        public bool ContainerMode_IsEditable
        {
            get
            {
                var propertyName = DLU.ContainerMode_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool ContainerMode_IsValid(ContainerMode proposedValue)
        {
            var propertyName = DLU.ContainerMode_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool ContainerMode_HasValue
        {
            get { return m_ContainerMode.HasValue; }
        }

        [NotMapped]
        public ContainerMode ContainerMode_Value
        {
            get
            {
                if (!ContainerMode_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.ContainerMode_Default; }
                    else
                    { return DefaultLayout_Value.ContainerMode_Value; }
                }

                return m_ContainerMode.Value;
            }
            set
            {
                if (!ContainerMode_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.ContainerMode_PropName)); }
                if (!ContainerMode_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.ContainerMode_PropName)); }

                m_ContainerMode = value;
            }
        }

        public void ContainerMode_ResetValue()
        { m_ContainerMode = null; }

        [NotMapped]
        public bool ContentGroup_IsEditable
        {
            get
            {
                var propertyName = DLU.ContentGroup_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool ContentGroup_IsValid(int proposedValue)
        {
            var propertyName = DLU.ContentGroup_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool ContentGroup_HasValue
        {
            get { return m_ContentGroup.HasValue; }
        }

        [NotMapped]
        public int ContentGroup_Value
        {
            get
            {
                if (!ContentGroup_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.ContentGroup_Default; }
                    else
                    { return DefaultLayout_Value.ContentGroup_Value; }
                }

                return m_ContentGroup.Value;
            }
            set
            {
                if (!ContentGroup_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.ContentGroup_PropName)); }
                if (!ContentGroup_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.ContentGroup_PropName)); }

                m_ContentGroup = value;
            }
        }

        public void ContentGroup_ResetValue()
        { m_ContentGroup = null; }

        [NotMapped]
        public bool MinRangeSizeInCells_IsEditable
        {
            get
            {
                var propertyName = DLU.MinRangeSizeInCells_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool MinRangeSizeInCells_IsValid(int proposedValue)
        {
            var propertyName = DLU.MinRangeSizeInCells_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool MinRangeSizeInCells_HasValue
        {
            get { return m_MinRangeSizeInCells.HasValue; }
        }

        [NotMapped]
        public int MinRangeSizeInCells_Value
        {
            get
            {
                if (!MinRangeSizeInCells_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.MinRangeSizeInCells_Default; }
                    else
                    { return DefaultLayout_Value.MinRangeSizeInCells_Value; }
                }

                return m_MinRangeSizeInCells.Value;
            }
            set
            {
                if (!MinRangeSizeInCells_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.MinRangeSizeInCells_PropName)); }
                if (!MinRangeSizeInCells_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.MinRangeSizeInCells_PropName)); }

                value.RangeSize_AssertIsValid(SizeMode_Value, value, MaxRangeSizeInCells_Value);

                m_MinRangeSizeInCells = value;
                ReValidate_RangeSize_Design();
                ReValidate_RangeSize_Production();
            }
        }

        public void MinRangeSizeInCells_ResetValue()
        {
            m_MinRangeSizeInCells = null;
            ReValidate_RangeSize_Design();
            ReValidate_RangeSize_Production();
        }

        [NotMapped]
        public bool MaxRangeSizeInCells_IsEditable
        {
            get
            {
                var propertyName = DLU.MaxRangeSizeInCells_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool MaxRangeSizeInCells_IsValid(int proposedValue)
        {
            var propertyName = DLU.MaxRangeSizeInCells_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool MaxRangeSizeInCells_HasValue
        {
            get { return m_MaxRangeSizeInCells.HasValue; }
        }

        [NotMapped]
        public bool MaxRangeSizeInCells_IsUnconstrained
        {
            get { return MaxRangeSizeInCells_Value.MaxRangeSize_IsUnconstrained(); }
        }

        [NotMapped]
        public int MaxRangeSizeInCells_Value
        {
            get
            {
                if (!MaxRangeSizeInCells_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return DLU.MaxRangeSizeInCells_Default; }
                    else
                    { return DefaultLayout_Value.MaxRangeSizeInCells_Value; }
                }

                return m_MaxRangeSizeInCells.Value;
            }
            set
            {
                if (!MaxRangeSizeInCells_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.MaxRangeSizeInCells_PropName)); }
                if (!MaxRangeSizeInCells_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.MaxRangeSizeInCells_PropName)); }

                value.RangeSize_AssertIsValid(SizeMode_Value, MinRangeSizeInCells_Value, value);

                m_MaxRangeSizeInCells = value;
                ReValidate_RangeSize_Design();
                ReValidate_RangeSize_Production();
            }
        }

        public void MaxRangeSizeInCells_SetToUnconstrained()
        {
            MaxRangeSizeInCells_Value = DLU.RangeSize_Unconstrained;
        }

        public void MaxRangeSizeInCells_ResetValue()
        {
            m_MaxRangeSizeInCells = null;
            ReValidate_RangeSize_Design();
            ReValidate_RangeSize_Production();
        }

        [NotMapped]
        public bool Margin_IsEditable
        {
            get
            {
                var propertyName = DLU.Margin_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool Margin_IsValid(DimensionStyleValue<int> proposedValue)
        {
            var propertyName = DLU.Margin_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public DimensionStyleValue<bool> Margin_HasValue
        {
            get
            {
                bool hasLesser = m_Margin.LesserSide.HasValue;
                bool hasGreater = m_Margin.GreaterSide.HasValue;

                return new DimensionStyleValue<bool>(hasLesser, hasGreater);
            }
        }

        [NotMapped]
        public DimensionStyleValue<int> Margin_Value
        {
            get
            {
                Func<int?> baseDefaultLesserGetter = (() => (DefaultLayout_Value != null) ? (int?)DefaultLayout_Value.Margin_Value.LesserSide : (int?)null);
                Func<int?> baseDefaultGreaterGetter = (() => (DefaultLayout_Value != null) ? (int?)DefaultLayout_Value.Margin_Value.GreaterSide : (int?)null);

                int lesser = StylingUtils.GetValueToUse(m_Margin.LesserSide, baseDefaultLesserGetter, DLU.Margin_Default);
                int greater = StylingUtils.GetValueToUse(m_Margin.GreaterSide, baseDefaultGreaterGetter, DLU.Margin_Default);

                return new DimensionStyleValue<int>(lesser, greater);
            }
            set
            {
                if (!Margin_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.Margin_PropName)); }
                if (!Margin_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.Margin_PropName)); }

                value.LesserSide.Margin_AssertIsValid();
                value.GreaterSide.Margin_AssertIsValid();

                m_Margin = new DimensionStyleValue<int?>(value.LesserSide, value.GreaterSide);
                ReValidate_Margin();
            }
        }

        public void Margin_ResetValue()
        {
            m_Margin = new DimensionStyleValue<int?>(null, null);
            ReValidate_Margin();
        }

        protected virtual void ReValidate_Margin()
        {
            int min = DLU.Margin_Min;
            int max = DLU.Margin_Max;
            Func<int, bool> isValidFunc = DLU.Margin_IsValid;

            DimensionStyleValue<int>.ReSetValuesWithinBounds(Margin_Value, min, max, isValidFunc, ref m_Margin);
        }

        [NotMapped]
        public bool Padding_IsEditable
        {
            get
            {
                var propertyName = DLU.Padding_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool Padding_IsValid(DimensionStyleValue<int> proposedValue)
        {
            var propertyName = DLU.Padding_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public DimensionStyleValue<bool> Padding_HasValue
        {
            get
            {
                bool hasLeft = m_Padding.LesserSide.HasValue;
                bool hasTop = m_Padding.GreaterSide.HasValue;

                return new DimensionStyleValue<bool>(hasLeft, hasTop);
            }
        }

        [NotMapped]
        public DimensionStyleValue<int> Padding_Value
        {
            get
            {
                Func<int?> baseDefaultLesserGetter = (() => (DefaultLayout_Value != null) ? (int?)DefaultLayout_Value.Padding_Value.LesserSide : (int?)null);
                Func<int?> baseDefaultGreaterGetter = (() => (DefaultLayout_Value != null) ? (int?)DefaultLayout_Value.Padding_Value.GreaterSide : (int?)null);

                int greater = StylingUtils.GetValueToUse(m_Padding.LesserSide, baseDefaultLesserGetter, DLU.Padding_Default);
                int lesser = StylingUtils.GetValueToUse(m_Padding.GreaterSide, baseDefaultGreaterGetter, DLU.Padding_Default);

                return new DimensionStyleValue<int>(greater, lesser);
            }
            set
            {
                if (!Padding_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.Padding_PropName)); }
                if (!Padding_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.Padding_PropName)); }

                value.LesserSide.Padding_AssertIsValid();
                value.GreaterSide.Padding_AssertIsValid();

                m_Padding = new DimensionStyleValue<int?>(value.LesserSide, value.GreaterSide);
                ReValidate_Padding();
            }
        }

        public void Padding_ResetValue()
        {
            m_Padding = new DimensionStyleValue<int?>(null, null);
            ReValidate_Padding();
        }

        protected virtual void ReValidate_Padding()
        {
            int min = DLU.Padding_Min;
            int max = DLU.Padding_Max;
            Func<int, bool> isValidFunc = DLU.Padding_IsValid;

            DimensionStyleValue<int>.ReSetValuesWithinBounds(Padding_Value, min, max, isValidFunc, ref m_Padding);
        }

        #endregion

        #region Excel Cell Rendering Properties

        [NotMapped]
        public double CellSize_Value
        {
            get
            {
                if (RenderingMode == RenderingMode.Design)
                { return CellSize_Design_Value; }
                else if (RenderingMode == RenderingMode.Production)
                { return CellSize_Production_Value; }
                else
                { throw new InvalidOperationException(InvalidRenderingModeMessage); }
            }
        }

        [NotMapped]
        public bool CellSize_HasValue
        {
            get
            {
                if (RenderingMode == RenderingMode.Design)
                { return CellSize_Design_HasValue; }
                else if (RenderingMode == RenderingMode.Production)
                { return CellSize_Production_HasValue; }
                else
                { throw new InvalidOperationException(InvalidRenderingModeMessage); }
            }
        }

        [NotMapped]
        public bool CellSize_CombineInputValues
        {
            get { return m_CellSize_CombineInputValues; }
            set { m_CellSize_CombineInputValues = value; }
        }

        [NotMapped]
        public bool CellSize_Design_IsEditable
        {
            get
            {
                var propertyName = DLU.CellSize_Design_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool CellSize_Design_IsValid(double proposedValue)
        {
            var propertyName = DLU.CellSize_Design_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool CellSize_Design_HasValue
        {
            get { return m_CellSize_Design.HasValue; }
        }

        [NotMapped]
        public double CellSize_Design_Value
        {
            get
            {
                if (!CellSize_Design_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return m_Dimension.CellSize_GetDefaultForDimension(); }
                    else
                    { return DefaultLayout_Value.CellSize_Design_Value; }
                }

                return m_CellSize_Design.Value;
            }
            set
            {
                if (!CellSize_Design_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.CellSize_Design_PropName)); }
                if (!CellSize_Design_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.CellSize_Design_PropName)); }

                value.CellSize_AssertIsValid();

                m_CellSize_Design = value;
                ReValidate_CellSize_Design();
            }
        }

        public void CellSize_Design_ResetValue()
        {
            m_CellSize_Design = null;
            ReValidate_CellSize_Design();
        }

        protected virtual void ReValidate_CellSize_Design()
        {
            double curr = CellSize_Design_Value;
            double min = DLU.CellSize_Min;
            double max = DLU.CellSize_Max;

            if (!curr.CellSize_IsValid())
            {
                if (curr < min)
                { CellSize_Design_Value = min; }
                else
                { CellSize_Design_Value = max; }
            }
        }

        [NotMapped]
        public bool CellSize_Production_IsEditable
        {
            get
            {
                var propertyName = DLU.CellSize_Production_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool CellSize_Production_IsValid(double proposedValue)
        {
            var propertyName = DLU.CellSize_Production_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool CellSize_Production_HasValue
        {
            get { return m_CellSize_Production.HasValue; }
        }

        [NotMapped]
        public double CellSize_Production_Value
        {
            get
            {
                if (CellSize_CombineInputValues)
                { return CellSize_Design_Value; }

                if (!CellSize_Production_HasValue)
                {
                    if (!DefaultLayout_HasValue)
                    { return m_Dimension.CellSize_GetDefaultForDimension(); }
                    else
                    { return DefaultLayout_Value.CellSize_Production_Value; }
                }

                return m_CellSize_Production.Value;
            }
            set
            {
                if (!CellSize_Production_IsEditable)
                { throw new InvalidOperationException(string.Format(NotEditableExceptionFormat, IDimensionLayoutUtils.CellSize_Production_PropName)); }
                if (!CellSize_Production_IsValid(value))
                { throw new InvalidOperationException(string.Format(InvalidValueExceptionFormat, IDimensionLayoutUtils.CellSize_Production_PropName)); }

                value.CellSize_AssertIsValid();

                m_CellSize_Production = value;
                ReValidate_CellSize_Production();
            }
        }

        public void CellSize_Production_ResetValue()
        {
            m_CellSize_Production = null;
            ReValidate_CellSize_Production();
        }

        protected virtual void ReValidate_CellSize_Production()
        {
            double curr = CellSize_Production_Value;
            double min = DLU.CellSize_Min;
            double max = DLU.CellSize_Max;

            if (!curr.CellSize_IsValid())
            {
                if (curr < min)
                { CellSize_Production_Value = min; }
                else
                { CellSize_Production_Value = max; }
            }
        }

        [NotMapped]
        public bool OverridePaddingCellSize_IsEditable
        {
            get
            {
                var propertyName = DLU.OverridePaddingCellSize_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool OverridePaddingCellSize_IsValid(bool proposedValue)
        {
            var propertyName = DLU.OverridePaddingCellSize_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool OverridePaddingCellSize_HasValue
        {
            get { return m_OverridePaddingCellSize.HasValue; }
        }

        [NotMapped]
        public bool OverridePaddingCellSize_Value
        {
            get
            {
                if (OverridePaddingCellSize_HasValue)
                { return m_OverridePaddingCellSize.Value; }

                if (DefaultLayout_HasValue)
                { return DefaultLayout_Value.OverridePaddingCellSize_Value; }

                return DLU.OverridePaddingCellSize_Default;
            }
            set
            {
                m_OverridePaddingCellSize = value;
            }
        }

        public void OverridePaddingCellSize_ResetValue()
        {
            m_OverridePaddingCellSize = null;
        }

        [NotMapped]
        public bool MergeInteriorAreaCells_IsEditable
        {
            get
            {
                var propertyName = DLU.MergeInteriorAreaCells_PropName;
                return m_EditabilitySpec.IsEditable(propertyName);
            }
        }

        public bool MergeInteriorAreaCells_IsValid(bool proposedValue)
        {
            var propertyName = DLU.MergeInteriorAreaCells_PropName;
            var propertyValues = GetCurrentPropertyValues();
            return m_EditabilitySpec.IsValueValid(propertyName, proposedValue, propertyValues);
        }

        [NotMapped]
        public bool MergeInteriorAreaCells_HasValue
        {
            get { return m_MergeInteriorAreaCells.HasValue; }
        }

        [NotMapped]
        public bool MergeInteriorAreaCells_Value
        {
            get
            {
                if (MergeInteriorAreaCells_HasValue)
                { return m_MergeInteriorAreaCells.Value; }

                if (DefaultLayout_HasValue)
                { return DefaultLayout_Value.MergeInteriorAreaCells_Value; }

                return DLU.MergeInteriorAreaCells_Default;
            }
            set
            {
                m_MergeInteriorAreaCells = value;
            }
        }

        public void MergeInteriorAreaCells_ResetValue()
        {
            m_MergeInteriorAreaCells = null;
        }

        #endregion
    }
}