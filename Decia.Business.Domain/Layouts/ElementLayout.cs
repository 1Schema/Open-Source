using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Layouts
{
    public class ElementLayout : IElementLayout
    {
        public const bool DefaultIsTransposed = false;

        protected bool m_IsContainer;
        protected bool m_IsTransposed;
        protected SortedDictionary<Dimension, IDimensionLayout> m_DimensionLayouts;

        public ElementLayout(bool isContainer, IDimensionLayout dimensionLayout_X, IDimensionLayout dimensionLayout_Y)
            : this(isContainer, dimensionLayout_X, dimensionLayout_Y, DefaultIsTransposed)
        { }

        public ElementLayout(bool isContainer, IDimensionLayout dimensionLayout_X, IDimensionLayout dimensionLayout_Y, bool isTransposed)
        {
            if (dimensionLayout_X.Dimension != Dimension.X)
            { throw new InvalidOperationException("Dimension Layout X must be of type \"X\"."); }
            if (dimensionLayout_Y.Dimension != Dimension.Y)
            { throw new InvalidOperationException("Dimension Layout Y must be of type \"Y\"."); }

            m_IsContainer = isContainer;
            m_IsTransposed = isTransposed;
            m_DimensionLayouts = new SortedDictionary<Dimension, IDimensionLayout>();

            if (!m_IsTransposed)
            {
                m_DimensionLayouts.Add(Dimension.X, dimensionLayout_X);
                m_DimensionLayouts.Add(Dimension.Y, dimensionLayout_Y);
            }
            else
            {
                m_DimensionLayouts.Add(Dimension.Y, dimensionLayout_X);
                m_DimensionLayouts.Add(Dimension.X, dimensionLayout_Y);
            }

            RenderingMode = RenderingMode;
        }

        public bool IsContainer
        {
            get { return m_IsContainer; }
        }

        public bool IsTransposed
        {
            get { return m_IsTransposed; }
        }

        public RenderingMode RenderingMode
        {
            get { return DimensionLayout_X.RenderingMode; }
            set
            {
                foreach (var dimLayout in m_DimensionLayouts.Values)
                {
                    dimLayout.RenderingMode = value;
                }
            }
        }

        public IDimensionLayout DimensionLayout_X
        {
            get { return m_DimensionLayouts[Dimension.X]; }
        }

        public IDimensionLayout DimensionLayout_Y
        {
            get { return m_DimensionLayouts[Dimension.Y]; }
        }

        public ICollection<Dimension> Dimensions
        {
            get { return new HashSet<Dimension>(m_DimensionLayouts.Keys.OrderBy(d => (int)d).ToList()); }
        }

        public ICollection<int> DimensionNumbers
        {
            get { return new HashSet<int>(Dimensions.Select(d => (int)d).OrderBy(d => d).ToList()); }
        }

        public int MinimumDimensionNumber
        {
            get { return DimensionNumbers.Min(); }
        }

        public int MaximumDimensionNumber
        {
            get { return DimensionNumbers.Max(); }
        }

        public IDimensionLayout GetDimensionLayout(Dimension dimension)
        {
            if (!m_DimensionLayouts.ContainsKey(dimension))
            { throw new InvalidOperationException("The requested Dimension Layout does not exist."); }

            return m_DimensionLayouts[dimension];
        }

        public IDimensionLayout GetDimensionLayout(int dimensionNumber)
        {
            return GetDimensionLayout((Dimension)dimensionNumber);
        }

        public bool HasContentGroups
        {
            get { return (GetContainerDimensions_Grid().Count > 0); }
        }

        public ICollection<Dimension> GetContainerDimensions(ContainerMode desiredMode)
        {
            var dimensions = new HashSet<Dimension>();

            if (!IsContainer)
            { return dimensions; }

            foreach (var dimension in Dimensions)
            {
                if (GetDimensionLayout(dimension).ContainerMode_Value == desiredMode)
                { dimensions.Add(dimension); }
            }
            return dimensions;
        }

        public ICollection<Dimension> GetContainerDimensions_Single()
        {
            return GetContainerDimensions(ContainerMode.Single);
        }

        public ICollection<Dimension> GetContainerDimensions_Grid()
        {
            return GetContainerDimensions(ContainerMode.Grid);
        }

        public Size GetDesiredSize(Size defaultSize)
        {
            return GetDesiredSize(defaultSize.Width, defaultSize.Height);
        }

        public Size GetDesiredSize(int defaultWidth, int defaultHeight)
        {
            var xLayout = DimensionLayout_X;
            var yLayout = DimensionLayout_Y;

            bool isXFixedSize = (xLayout.SizeMode_Value == SizeMode.Cell);
            bool isYFixedSize = (yLayout.SizeMode_Value == SizeMode.Cell);

            int width = (isXFixedSize) ? xLayout.RangeSize_Value : defaultWidth;
            int height = (isYFixedSize) ? yLayout.RangeSize_Value : defaultHeight;

            return new Size(width, height);
        }
    }
}