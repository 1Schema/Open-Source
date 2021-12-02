using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public static class ExcelExportUtils
    {
        public static readonly ExcelBorderStyle[] OrderedBorderStyles = new ExcelBorderStyle[] { ExcelBorderStyle.Double, ExcelBorderStyle.Thick, ExcelBorderStyle.Medium, ExcelBorderStyle.Thin, ExcelBorderStyle.None };

        public static ExcelBorderStyle MergeBorderStyles(this ExcelBorderStyle original, ExcelBorderStyle updated)
        {
            foreach (var borderStyle in OrderedBorderStyles)
            {
                if ((original == borderStyle) || (updated == borderStyle))
                { return borderStyle; }
            }
            return ExcelBorderStyle.None;
        }

        public static void MergeCellsInRange(this ExcelRange fullRange, bool mergeRowCells, bool mergeColumnCells)
        {
            if ((fullRange.Start.Row == fullRange.End.Row) && (fullRange.Start.Column == fullRange.End.Column))
            {
                mergeRowCells = false;
                mergeColumnCells = false;
            }

            if (mergeRowCells && mergeColumnCells)
            {
                fullRange.Merge = true;
            }
            else if (mergeRowCells)
            {
                int startRowIndex = fullRange.Start.Row;
                int endRowIndex = fullRange.End.Row;

                for (int i = startRowIndex; i <= endRowIndex; i++)
                {
                    var rowCells = fullRange.Worksheet.Cells[i, fullRange.Start.Column, i, fullRange.End.Column];
                    rowCells.Merge = true;
                }
            }
            else if (mergeColumnCells)
            {
                int startColIndex = fullRange.Start.Column;
                int endColIndex = fullRange.End.Column;

                for (int j = startColIndex; j <= endColIndex; j++)
                {
                    var colCells = fullRange.Worksheet.Cells[fullRange.Start.Row, j, fullRange.End.Row, j];
                    colCells.Merge = true;
                }
            }
            else
            {
                // do nothing
            }
        }

    }
}