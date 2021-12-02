using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Styling;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public interface IRenderedObject
    {
        RenderingKey Key { get; }
        string Name { get; }
        RenderingMode RenderingMode { get; }
        RenderedLayout Layout { get; }

        object Template { get; }
        bool IsReport { get; }
        bool IsElement { get; }
        Nullable<ReportElementType_New> ElementType { get; }

        bool HasRenderedReport { get; }
        IRenderedObject RenderedReport { get; set; }
        bool HasParentRenderedRange { get; }
        IRenderedObject ParentRenderedRange { get; set; }

        void SetCachedLayoutValues();
        bool AreLayoutValuesCached { get; }
        void UpdateDimensionalDisplaySizes(Dimension dimension, List<double> displaySizes);

        List<GroupBox> GroupBoxes { get; }

        DimensionStyleValue<double?> OverriddenCellSize { get; set; }
        DimensionStyleValue<bool> OverrideCellSizeInPadding { get; set; }
        DimensionStyleValue<bool> MergeInteriorAreaCells { get; set; }

        BoxPosition Rel_ViewArea { get; }
        BoxPosition Abs_ViewArea { get; }
        BoxStyleValue<int> Padding { get; }
        BoxPosition Rel_InteriorArea { get; }
        BoxPosition Abs_InteriorArea { get; }

        ColorSpecification BackColorSpec { get; set; }
        Color BackColor { get; set; }
        ColorSpecification ForeColorSpec { get; set; }
        Color ForeColor { get; set; }

        string FontName { get; set; }
        double FontSize { get; set; }
        DeciaFontStyle FontStyle { get; set; }
        Font Font { get; }

        HAlignment H_Align { get; set; }
        VAlignment V_Align { get; set; }
        int Indent { get; set; }

        BoxStyleValue<BorderLineStyle> BorderStyle { get; set; }
        ColorSpecification BorderColorSpec_Left { get; set; }
        Color BorderColor_Left { get; set; }
        ColorSpecification BorderColorSpec_Top { get; set; }
        Color BorderColor_Top { get; set; }
        ColorSpecification BorderColorSpec_Right { get; set; }
        Color BorderColor_Right { get; set; }
        ColorSpecification BorderColorSpec_Bottom { get; set; }
        Color BorderColor_Bottom { get; set; }
    }
}