using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Layouts
{
    public class CellLayoutManager<O, R>
    {
        #region Members

        protected O m_OrginalContainer;
        protected R m_RenderedContainer;
        protected List<double> m_ColumnsWidths;
        protected List<double> m_RowHeights;
        protected List<double> m_AggrPrevColumnsWidthsAtIndex;
        protected List<double> m_AggrPrevRowHeightsAtIndex;

        #endregion

        #region Constructors

        public CellLayoutManager(O orginalContainer, R renderedContainer, Func<R, List<double>> columnWidthsGetter, Func<R, List<double>> rowHeightsGetter)
        {
            m_OrginalContainer = orginalContainer;
            m_RenderedContainer = renderedContainer;

            m_ColumnsWidths = columnWidthsGetter(m_RenderedContainer);
            m_RowHeights = rowHeightsGetter(m_RenderedContainer);

            m_AggrPrevColumnsWidthsAtIndex = CreateAggregatePreviousSizeList(m_ColumnsWidths);
            m_AggrPrevRowHeightsAtIndex = CreateAggregatePreviousSizeList(m_RowHeights);
        }

        protected List<double> CreateAggregatePreviousSizeList(List<double> displaySizes)
        {
            var result = new List<double>();
            double previousSize = 0;

            for (int i = 0; i < displaySizes.Count; i++)
            {
                result.Add(previousSize);
                previousSize += displaySizes[i];
            }
            return result;
        }

        #endregion

        #region Properties

        public O OrginalContainer { get { return m_OrginalContainer; } }
        public R RenderedContainer { get { return m_RenderedContainer; } }
        public List<double> ColumnsWidths { get { return m_ColumnsWidths; } }
        public List<double> RowHeights { get { return m_RowHeights; } }

        #endregion

        #region Methods

        public double GetDefaultColumnWidth()
        { return CellLayoutManagerUtils.GetDefaultColumnWidth(); }

        public double GetDefaultRowHeight()
        { return CellLayoutManagerUtils.GetDefaultRowHeight(); }

        public double GetDefaultDisplaySize(Dimension dimension)
        { return CellLayoutManagerUtils.GetDefaultDisplaySize(dimension); }

        public double GetRangeWidth(int startColumnIndex, int widthInColumns)
        {
            double totalWidth = 0;
            for (int i = startColumnIndex; (i - startColumnIndex) < widthInColumns; i++)
            { totalWidth += m_ColumnsWidths[i]; }
            return totalWidth;
        }

        public double GetRangeHeight(int startRowIndex, int heightInRows)
        {
            double totalHeight = 0;
            for (int i = startRowIndex; (i - startRowIndex) < heightInRows; i++)
            { totalHeight += m_RowHeights[i]; }
            return totalHeight;
        }

        public double GetX_Abs(int columnIndex)
        {
            if (columnIndex <= 0)
            { return 0; }
            return m_AggrPrevColumnsWidthsAtIndex[columnIndex];
        }

        public double GetY_Abs(int rowIndex)
        {
            if (rowIndex <= 0)
            { return 0; }
            return m_AggrPrevRowHeightsAtIndex[rowIndex];
        }

        #endregion
    }

    public static class CellLayoutManagerUtils
    {
        #region Static Members

        public static readonly double DefaultZoomFactor = 1.0;
        public static readonly double DefaultSizingMultiplier = 1.2;

        public static double GetDefaultColumnWidth()
        { return GetDefaultDisplaySize(Dimension.X); }

        public static double GetDefaultRowHeight()
        { return GetDefaultDisplaySize(Dimension.Y); }

        public static double GetDefaultDisplaySize(Dimension dimension)
        {
            var sizeValue = (dimension == Dimension.X) ? IDimensionLayoutUtils.CellSize_DefaultWidth : IDimensionLayoutUtils.CellSize_DefaultHeight;
            sizeValue *= DefaultSizingMultiplier;
            return sizeValue;
        }

        #endregion
    }
}