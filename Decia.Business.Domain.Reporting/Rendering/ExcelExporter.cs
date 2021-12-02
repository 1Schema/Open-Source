using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public delegate void AlterRangeDelegate(ExcelRange range);

    public class ExcelExporter : IExcelExporter
    {
        public const bool RepeatCellValues = false;
        public const ExcelFillStyle FillStyle_ShowAsHidden = ExcelFillStyle.DarkGrid;
        public const double CellWidthFactor = (8.43 / (15.0 * (63.0 / 19.25)));
        public const double CellHeightFactor = 1.0;

        private IFormulaDataProvider m_DataProvider;
        private Nullable<RenderingMode> m_ExportMode;
        private double m_DefaultCellWidth;
        private double m_DefaultCellHeight;

        public ExcelExporter(IFormulaDataProvider dataProvider)
        {
            m_DataProvider = dataProvider;
            m_ExportMode = null;
        }

        public IFormulaDataProvider DataProvider
        {
            get { return m_DataProvider; }
        }

        public RenderingMode ExportMode
        {
            get { return m_ExportMode.Value; }
        }

        public bool ShowHiddenElements
        {
            get { return (ExportMode != RenderingMode.Production); }
        }

        public ExcelPackage ExportReport(RenderedReport report)
        {
            ExcelPackage excelPackage = new ExcelPackage();

            ExportReport(report, excelPackage);
            return excelPackage;
        }

        public void ExportReport(RenderedReport report, ExcelPackage excelPackage)
        {
            int worksheetCount = excelPackage.Workbook.Worksheets.Count;

            ExportReport(report, excelPackage, worksheetCount + 1);
        }

        public void ExportReport(RenderedReport report, ExcelPackage excelPackage, int worksheetPostion)
        {
            m_ExportMode = report.RenderingMode;

            var workbook = excelPackage.Workbook;

            var ws = workbook.Worksheets.Add(report.Name);
            workbook.Worksheets.MoveAfter(ws.Index, worksheetPostion);

            var viewArea = report.Abs_ViewArea;
            var interiorArea = report.Abs_InteriorArea;

            m_DefaultCellWidth = ws.Column(1).Width;
            m_DefaultCellHeight = ws.Row(1).Height;

            var allCells = ws.Cells;
            SetFill(allCells.Style.Fill, report.OutsideArea_Color, false);

            if (report.OverrideCellSizeInPadding.LesserSide)
            { OverrideCellWidth(ws, viewArea.Excel_Left, viewArea.Excel_Right, report.OverriddenCellSize.LesserSide); }
            else
            { OverrideCellWidth(ws, interiorArea.Excel_Left, interiorArea.Excel_Right, report.OverriddenCellSize.LesserSide); }

            if (report.OverrideCellSizeInPadding.GreaterSide)
            { OverrideCellHeight(ws, viewArea.Excel_Top, viewArea.Excel_Bottom, report.OverriddenCellSize.GreaterSide); }
            else
            { OverrideCellHeight(ws, interiorArea.Excel_Top, interiorArea.Excel_Bottom, report.OverriddenCellSize.GreaterSide); }

            ExportRange(report, ws, report);

            var children = report.Children;
            foreach (var rangeId in children.Keys)
            {
                var range = children[rangeId];
                ExportRange(range, ws, report);
            }
        }

        protected void ExportRange(IRenderedObject renderedObject, ExcelWorksheet ws, RenderedReport parentReport)
        {
            var isReport = (renderedObject is RenderedReport);
            var isRange = !isReport;
            var report = (renderedObject as RenderedReport);
            var range = (renderedObject as RenderedRange);
            var isHidden = (isRange && range.IsHidden);

            if (isHidden && !ShowHiddenElements)
            { return; }

            var viewArea = renderedObject.Abs_ViewArea;
            var interiorArea = renderedObject.Abs_InteriorArea;
            var rangeCells = ws.Cells[viewArea.Excel_Top, viewArea.Excel_Left, viewArea.Excel_Bottom, viewArea.Excel_Right];

            SetFill(rangeCells.Style.Fill, renderedObject.BackColor, isHidden);

            rangeCells.Style.Font.Color.SetColor(renderedObject.ForeColor);
            rangeCells.Style.Font.Name = renderedObject.FontName;
            rangeCells.Style.Font.Size = (float)renderedObject.FontSize;
            rangeCells.Style.Font.Bold = renderedObject.FontStyle.IsBold();
            rangeCells.Style.Font.Italic = renderedObject.FontStyle.IsItalic();
            rangeCells.Style.Font.UnderLine = renderedObject.FontStyle.IsUnderline();
            rangeCells.Style.Font.Strike = renderedObject.FontStyle.IsStrikeout();
            rangeCells.Style.HorizontalAlignment = renderedObject.H_Align.GetExcelValue();
            rangeCells.Style.VerticalAlignment = renderedObject.V_Align.GetExcelValue();

            if (renderedObject.H_Align != HAlignment.Center)
            { rangeCells.Style.Indent = renderedObject.Indent; }

            var leftRangeCells = ws.Cells[viewArea.Excel_Top, viewArea.Excel_Left, viewArea.Excel_Bottom, viewArea.Excel_Left];
            SetLeftBorder(leftRangeCells.Style.Border, renderedObject.BorderStyle.Left, renderedObject.BorderColor_Left);
            var topRangeCells = ws.Cells[viewArea.Excel_Top, viewArea.Excel_Left, viewArea.Excel_Top, viewArea.Excel_Right];
            SetTopBorder(topRangeCells.Style.Border, renderedObject.BorderStyle.Top, renderedObject.BorderColor_Top);
            var rightRangeCells = ws.Cells[viewArea.Excel_Top, viewArea.Excel_Right, viewArea.Excel_Bottom, viewArea.Excel_Right];
            SetRightBorder(rightRangeCells.Style.Border, renderedObject.BorderStyle.Right, renderedObject.BorderColor_Right);
            var bottomRangeCells = ws.Cells[viewArea.Excel_Bottom, viewArea.Excel_Left, viewArea.Excel_Bottom, viewArea.Excel_Right];
            SetBottomBorder(bottomRangeCells.Style.Border, renderedObject.BorderStyle.Bottom, renderedObject.BorderColor_Bottom);

            if (renderedObject.OverriddenCellSize.LesserSide.HasValue)
            {
                if (renderedObject.OverrideCellSizeInPadding.LesserSide)
                { OverrideCellWidth(ws, viewArea.Excel_Left, viewArea.Excel_Right, renderedObject.OverriddenCellSize.LesserSide); }
                else
                { OverrideCellWidth(ws, interiorArea.Excel_Left, interiorArea.Excel_Right, renderedObject.OverriddenCellSize.LesserSide); }
            }

            if (renderedObject.OverriddenCellSize.GreaterSide.HasValue)
            {
                if (renderedObject.OverrideCellSizeInPadding.GreaterSide)
                { OverrideCellHeight(ws, viewArea.Excel_Top, viewArea.Excel_Bottom, renderedObject.OverriddenCellSize.GreaterSide); }
                else
                { OverrideCellHeight(ws, interiorArea.Excel_Top, interiorArea.Excel_Bottom, renderedObject.OverriddenCellSize.GreaterSide); }
            }

            if (isReport)
            { return; }

            var interiorCells = ws.Cells[interiorArea.Excel_Top, interiorArea.Excel_Left, interiorArea.Excel_Bottom, interiorArea.Excel_Right];
            interiorCells.MergeCellsInRange(renderedObject.MergeInteriorAreaCells.LesserSide, renderedObject.MergeInteriorAreaCells.GreaterSide);

            object cellValue = range.HasFormula ? range.ExcelFormula : range.DirectValue;
            if (RepeatCellValues)
            { interiorCells.Value = cellValue; }
            else
            { interiorCells.Worksheet.Cells[interiorCells.Start.Row, interiorCells.Start.Column].Value = cellValue; }

            var children = range.Children;
            foreach (var rangeId in children.Keys)
            {
                var nestedRange = children[rangeId];
                ExportRange(nestedRange, ws, parentReport);
            }
        }

        protected void AlterCellsInRange(ExcelRange fullRange, AlterRangeDelegate alterMethod)
        {
            int startRowIndex = fullRange.Start.Row;
            int startColIndex = fullRange.Start.Column;
            int endRowIndex = fullRange.End.Row;
            int endColIndex = fullRange.End.Column;

            for (int i = startRowIndex; i <= endRowIndex; i++)
            {
                for (int j = startColIndex; j <= endColIndex; j++)
                {
                    var cell = fullRange.Worksheet.Cells[i, j];
                    alterMethod(cell);
                }
            }
        }

        #region  Set Color Methods

        protected void SetFill(ExcelFill fill, Color colorValue, bool showAsHidden)
        {
            if (colorValue.A == 0)
            {
                if (showAsHidden)
                { fill.PatternType = FillStyle_ShowAsHidden; }
            }
            else
            {
                fill.PatternType = (!showAsHidden) ? ExcelFillStyle.Solid : FillStyle_ShowAsHidden;
                fill.BackgroundColor.SetColor(colorValue);
            }
        }

        protected void SetLeftBorder(Border border, BorderLineStyle lineStyle, Color colorValue)
        {
            var excelBorderStyle = border.Left.Style.MergeBorderStyles(lineStyle.GetExcelValue());

            border.Left.Style = excelBorderStyle;
            if (excelBorderStyle != ExcelBorderStyle.None)
            { border.Left.Color.SetColor(colorValue); }
        }

        protected void SetTopBorder(Border border, BorderLineStyle lineStyle, Color colorValue)
        {
            var excelBorderStyle = border.Top.Style.MergeBorderStyles(lineStyle.GetExcelValue());

            border.Top.Style = excelBorderStyle;
            if (excelBorderStyle != ExcelBorderStyle.None)
            { border.Top.Color.SetColor(colorValue); }
        }

        protected void SetRightBorder(Border border, BorderLineStyle lineStyle, Color colorValue)
        {
            var excelBorderStyle = border.Right.Style.MergeBorderStyles(lineStyle.GetExcelValue());

            border.Right.Style = excelBorderStyle;
            if (excelBorderStyle != ExcelBorderStyle.None)
            { border.Right.Color.SetColor(colorValue); }
        }

        protected void SetBottomBorder(Border border, BorderLineStyle lineStyle, Color colorValue)
        {
            var excelBorderStyle = border.Bottom.Style.MergeBorderStyles(lineStyle.GetExcelValue());

            border.Bottom.Style = excelBorderStyle;
            if (excelBorderStyle != ExcelBorderStyle.None)
            { border.Bottom.Color.SetColor(colorValue); }
        }

        #endregion

        #region Cell Size Override Methods

        private void OverrideCellWidth(ExcelWorksheet ws, int columnIndex, double? width)
        { OverrideCellWidth(ws, columnIndex, columnIndex, width); }

        private void OverrideCellWidth(ExcelWorksheet ws, int startColumnIndex, int endColumnIndex, double? width)
        {
            if (!width.HasValue)
            { return; }

            var adjustedWidth = (width.Value * CellWidthFactor);

            for (int j = startColumnIndex; j <= endColumnIndex; j++)
            {
                double existingWidth = ws.Column(j).Width;

                if ((existingWidth == m_DefaultCellWidth) || (existingWidth < adjustedWidth))
                { ws.Column(j).Width = adjustedWidth; }
            }
        }

        private void OverrideCellHeight(ExcelWorksheet ws, int rowIndex, double? height)
        { OverrideCellHeight(ws, rowIndex, rowIndex, height); }

        private void OverrideCellHeight(ExcelWorksheet ws, int startRowIndex, int endRowIndex, double? height)
        {
            if (!height.HasValue)
            { return; }

            var adjustedHeight = (height.Value * CellHeightFactor);

            for (int i = startRowIndex; i <= endRowIndex; i++)
            {
                double existingHeight = ws.Row(i).Height;

                if ((existingHeight == m_DefaultCellHeight) || (existingHeight < adjustedHeight))
                { ws.Row(i).Height = adjustedHeight; }
            }
        }

        #endregion
    }
}