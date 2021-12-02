using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public abstract class RenderedObjectBase<RT, ET> : IRenderedObject
        where RT : class, IRenderedObject
        where ET : class, IRenderedObject
    {
        #region Static Members

        public static readonly Color DefaultBackColor = Color.Transparent;
        public static readonly Color DefaultForeColor = Color.Black;
        public const string DefaultFontName = "";
        public const double DefaultFontSize = 11.0;
        public const DeciaFontStyle DefaultFontStyle = DeciaFontStyle.Regular;
        public const HAlignment Default_H_Align = HAlignment.Left;
        public const VAlignment Default_V_Align = VAlignment.Bottom;
        public const int DefaultIndent = 0;

        #endregion

        #region Members & Constructor

        protected bool m_AreLayoutValuesCached;
        protected Nullable<BoxPosition> m_Cached_Abs_ViewArea;
        protected Nullable<BoxPosition> m_Cached_Abs_InteriorArea;
        protected Nullable<BoxPosition> m_Cached_Rel_ViewArea;
        protected Nullable<BoxPosition> m_Cached_Rel_InteriorArea;

        protected RenderedLayout m_Layout;
        protected Dictionary<RenderingKey, ET> m_Children;

        public RenderedObjectBase(RenderedLayout layout)
            : this(layout, new Point(0, 0))
        { }

        public RenderedObjectBase(RenderedLayout layout, Point groupOffset)
        {
            m_AreLayoutValuesCached = false;
            m_Cached_Abs_ViewArea = null;
            m_Cached_Abs_InteriorArea = null;
            m_Cached_Rel_ViewArea = null;
            m_Cached_Rel_InteriorArea = null;

            m_Layout = layout;
            m_Children = new Dictionary<RenderingKey, ET>(ReportRenderingEngine.EqualityComparer_RenderingKey);

            GroupOffset = groupOffset;
            Size = layout.ActualSize;
            Rel_Location = layout.Location;

            Padding = new BoxStyleValue<int>(0);

            BackColorSpec = layout.TemplateStyle.BackColor_Value;
            ForeColorSpec = layout.TemplateStyle.ForeColor_Value;
            FontName = layout.TemplateStyle.FontName_Value;
            FontSize = layout.TemplateStyle.FontSize_Value;
            FontStyle = layout.TemplateStyle.FontStyle_Value;
            H_Align = layout.TemplateStyle.FontHAlign_Value;
            V_Align = layout.TemplateStyle.FontVAlign_Value;
            Indent = layout.TemplateStyle.Indent_Value;

            BorderStyle = layout.TemplateStyle.BorderStyle_Value;
            BorderColorSpec_Left = layout.TemplateStyle.BorderColor_Value.Left;
            BorderColorSpec_Top = layout.TemplateStyle.BorderColor_Value.Top;
            BorderColorSpec_Right = layout.TemplateStyle.BorderColor_Value.Right;
            BorderColorSpec_Bottom = layout.TemplateStyle.BorderColor_Value.Bottom;

            var xTemplateLayout = layout.TemplateLayout.GetDimensionLayout(Dimension.X);
            var yTemplateLayout = layout.TemplateLayout.GetDimensionLayout(Dimension.Y);

            if (layout.IsReport)
            {
                OverriddenCellSize = new DimensionStyleValue<double?>(xTemplateLayout.CellSize_Value, yTemplateLayout.CellSize_Value);
            }
            else
            {
                var xDefaultCellSize = IDimensionLayoutUtils.CellSize_GetDefaultForDimension(Dimension.X);
                var yDefaultCellSize = IDimensionLayoutUtils.CellSize_GetDefaultForDimension(Dimension.Y);

                var xHasCellSize = (xTemplateLayout.CellSize_HasValue || (xTemplateLayout.CellSize_Value != xDefaultCellSize));
                var yHasCellSize = (yTemplateLayout.CellSize_HasValue || (yTemplateLayout.CellSize_Value != yDefaultCellSize));

                var xCellSize = xHasCellSize ? xTemplateLayout.CellSize_Value : (double?)null;
                var yCellSize = yHasCellSize ? yTemplateLayout.CellSize_Value : (double?)null;

                OverriddenCellSize = new DimensionStyleValue<double?>(xCellSize, yCellSize);
            }
            OverrideCellSizeInPadding = new DimensionStyleValue<bool>(xTemplateLayout.OverridePaddingCellSize_Value, yTemplateLayout.OverridePaddingCellSize_Value);
            MergeInteriorAreaCells = new DimensionStyleValue<bool>(xTemplateLayout.MergeInteriorAreaCells_Value, yTemplateLayout.MergeInteriorAreaCells_Value);
        }

        #endregion

        #region Report Element Tree Properties and Methods

        public RenderingKey Key { get { return Layout.RenderingKey; } }
        public string Name { get { return (Layout.Element != null) ? Layout.Element.Name : Layout.Report.Name; } }
        public RenderingMode RenderingMode { get { return Layout.StructuralPoint.HasValue ? RenderingMode.Production : RenderingMode.Design; } }
        public RenderedLayout Layout { get { return m_Layout; } }

        public object Template { get { return (Layout.Element != null) ? (object)Layout.Element : (object)Layout.Report; } }
        public bool IsReport { get { return (Template is IReport); } }
        public bool IsElement { get { return (Template is IReportElement); } }
        public bool IsContainer { get { return Layout.IsContainer; } }
        public bool IsHidden { get { return Layout.IsHidden; } }

        public ReportElementId ElementId { get { return (Layout.Element != null) ? Layout.Element.Key : Layout.Report.KeyAsElementId; } }
        public Nullable<ReportElementType_New> ElementType { get { return IsElement ? (Nullable<ReportElementType_New>)Layout.Element.ReportElementType : null; } }

        public bool HasRenderedReport { get { return (RenderedReport != null); } }
        public IRenderedObject RenderedReport { get; set; }
        public bool HasParentRenderedRange { get { return (ParentRenderedRange != null); } }
        public IRenderedObject ParentRenderedRange { get; set; }

        public void SetCachedLayoutValues()
        {
            if (AreLayoutValuesCached)
            { throw new InvalidOperationException("The Layout values are already cached."); }

            m_Cached_Abs_ViewArea = Abs_ViewArea;
            m_Cached_Abs_InteriorArea = Abs_InteriorArea;
            m_Cached_Rel_ViewArea = Rel_ViewArea;
            m_Cached_Rel_InteriorArea = Rel_InteriorArea;

            m_AreLayoutValuesCached = true;

            foreach (var child in Children.Values)
            { child.SetCachedLayoutValues(); }
        }

        public bool AreLayoutValuesCached { get { return m_AreLayoutValuesCached; } }

        public List<GroupBox> GroupBoxes
        {
            get
            {
                List<GroupBox> groupBoxes = new List<GroupBox>();
                int xOffset = 0;
                foreach (int xGroupNumber in m_Layout.NestedGroups[Dimension.X].Keys)
                {
                    var xGroup = m_Layout.NestedGroups[Dimension.X][xGroupNumber];
                    int yOffset = 0;

                    foreach (int yGroupNumber in m_Layout.NestedGroups[Dimension.Y].Keys)
                    {
                        var yGroup = m_Layout.NestedGroups[Dimension.Y][yGroupNumber];
                        var groupRect = new Rectangle(xOffset, yOffset, xGroup.ActualSize.Width, yGroup.ActualSize.Height);

                        var groupBox = new GroupBox(xGroupNumber, yGroupNumber, groupRect);
                        groupBoxes.Add(groupBox);

                        yOffset += yGroup.ActualSize.Height;
                    }
                    xOffset += xGroup.ActualSize.Width;
                }
                return groupBoxes;
            }
        }

        public IDictionary<RenderingKey, ET> Children
        {
            get { return m_Children.ToDictionary(c => c.Key, c => c.Value, ReportRenderingEngine.EqualityComparer_RenderingKey); }
        }

        public virtual void AddChild(ET range)
        {
            range.RenderedReport = this.RenderedReport;
            range.ParentRenderedRange = this;

            m_Children.Add(range.Key, range);
        }

        void IRenderedObject.UpdateDimensionalDisplaySizes(Dimension dimension, List<double> displaySizes)
        {
            var defaultDisplaySize = (dimension == Dimension.X) ? IDimensionLayoutUtils.CellSize_DefaultWidth : IDimensionLayoutUtils.CellSize_DefaultHeight;
            var overrideCellSizeInPadding = (dimension == Dimension.X) ? this.OverrideCellSizeInPadding.LesserSide : this.OverrideCellSizeInPadding.GreaterSide;
            var overriddenCellSize = (dimension == Dimension.X) ? this.OverriddenCellSize.LesserSide : this.OverriddenCellSize.GreaterSide;

            if (overriddenCellSize.HasValue)
            {
                var outer_StartDisplayIndex = (dimension == Dimension.X) ? this.Abs_ViewArea.Left : this.Abs_ViewArea.Top;
                var outer_EndDisplayIndex = (dimension == Dimension.X) ? this.Abs_ViewArea.Right : this.Abs_ViewArea.Bottom;

                var inner_StartDisplayIndex = (dimension == Dimension.X) ? this.Abs_InteriorArea.Left : this.Abs_InteriorArea.Top;
                var inner_EndDisplayIndex = (dimension == Dimension.X) ? this.Abs_InteriorArea.Right : this.Abs_InteriorArea.Bottom;

                var startDisplayIndex = (overrideCellSizeInPadding) ? outer_StartDisplayIndex : inner_StartDisplayIndex;
                var endDisplayIndex = (overrideCellSizeInPadding) ? outer_EndDisplayIndex : inner_EndDisplayIndex;

                for (int i = startDisplayIndex; i < endDisplayIndex; i++)
                {
                    var existingDisplaySize = displaySizes[i];

                    if ((existingDisplaySize == defaultDisplaySize) || (existingDisplaySize < overriddenCellSize))
                    { displaySizes[i] = overriddenCellSize.Value; }
                }
            }

            var children = this.m_Children.Values;
            foreach (var child in children)
            {
                child.UpdateDimensionalDisplaySizes(dimension, displaySizes);
            }
        }

        #endregion

        #region CellSize Properties

        public DimensionStyleValue<double?> OverriddenCellSize { get; set; }
        public DimensionStyleValue<bool> OverrideCellSizeInPadding { get; set; }
        public DimensionStyleValue<bool> MergeInteriorAreaCells { get; set; }

        #endregion

        #region Layout Properties

        public Point GroupOffset { get; set; }

        public Size Size { get; set; }
        public int Width { get { return Size.Width; } }
        public int Height { get { return Size.Height; } }

        public Point Rel_Location { get; set; }
        public int Rel_Left { get { return Rel_ViewArea.Left; } }
        public int Rel_Top { get { return Rel_ViewArea.Top; } }
        public int Rel_Right { get { return Rel_ViewArea.Right; } }
        public int Rel_Bottom { get { return Rel_ViewArea.Bottom; } }

        public BoxPosition Rel_ViewArea
        {
            get { return (m_AreLayoutValuesCached) ? m_Cached_Rel_ViewArea.Value : new BoxPosition(Rel_Location.X, Rel_Location.Y, Size.Width, Size.Height); }
        }

        public BoxPosition Abs_ViewArea
        {
            get
            {
                if (m_AreLayoutValuesCached)
                { return m_Cached_Abs_ViewArea.Value; }

                int rAbsLeft = (HasRenderedReport && (this != RenderedReport)) ? RenderedReport.Abs_InteriorArea.Left : 0;
                int rAbsTop = (HasRenderedReport && (this != RenderedReport)) ? RenderedReport.Abs_InteriorArea.Top : 0;

                int pAbsLeft = (HasParentRenderedRange) ? ParentRenderedRange.Abs_InteriorArea.Left : rAbsLeft;
                int pAbsTop = (HasParentRenderedRange) ? ParentRenderedRange.Abs_InteriorArea.Top : rAbsTop;

                int absLeft = (pAbsLeft + GroupOffset.X + Rel_Location.X);
                int absTop = (pAbsTop + GroupOffset.Y + Rel_Location.Y);

                return new BoxPosition(absLeft, absTop, Size.Width, Size.Height);
            }
        }

        public Point Abs_Location { get { return new Point(Abs_ViewArea.Left, Abs_ViewArea.Top); } }
        public int Abs_Left { get { return Abs_ViewArea.Left; } }
        public int Abs_Top { get { return Abs_ViewArea.Top; } }
        public int Abs_Right { get { return Abs_ViewArea.Right; } }
        public int Abs_Bottom { get { return Abs_ViewArea.Bottom; } }

        #endregion

        #region Padding Properties

        public BoxStyleValue<int> Padding { get; set; }
        public int Padding_Left { get { return Padding.Left; } }
        public int Padding_Top { get { return Padding.Top; } }
        public int Padding_Right { get { return Padding.Right; } }
        public int Padding_Bottom { get { return Padding.Bottom; } }

        public BoxPosition Rel_InteriorArea
        {
            get { return (m_AreLayoutValuesCached) ? m_Cached_Rel_InteriorArea.Value : new BoxPosition(Rel_Left + Padding_Left, Rel_Top + Padding_Top, Width - (Padding_Left + Padding_Right), Height - (Padding_Top + Padding_Bottom)); }
        }

        public BoxPosition Abs_InteriorArea
        {
            get { return (m_AreLayoutValuesCached) ? m_Cached_Abs_InteriorArea.Value : new BoxPosition(Abs_Left + Padding_Left, Abs_Top + Padding_Top, Width - (Padding_Left + Padding_Right), Height - (Padding_Top + Padding_Bottom)); }
        }

        #endregion

        #region BackColor Properties

        public ColorSpecification BackColorSpec { get; set; }
        public int BackColor_Alpha { get { return BackColorSpec.Alpha; } }
        public int BackColor_Red { get { return BackColorSpec.Red; } }
        public int BackColor_Green { get { return BackColorSpec.Green; } }
        public int BackColor_Blue { get { return BackColorSpec.Blue; } }

        public Color BackColor
        {
            get { return BackColorSpec.GetColor(); }
            set { BackColorSpec = new ColorSpecification(value); }
        }

        #endregion

        #region ForeColor Properties

        public ColorSpecification ForeColorSpec { get; set; }
        public int ForeColor_Alpha { get { return ForeColorSpec.Alpha; } }
        public int ForeColor_Red { get { return ForeColorSpec.Red; } }
        public int ForeColor_Green { get { return ForeColorSpec.Green; } }
        public int ForeColor_Blue { get { return ForeColorSpec.Blue; } }

        public Color ForeColor
        {
            get { return ForeColorSpec.GetColor(); }
            set { ForeColorSpec = new ColorSpecification(value); }
        }

        #endregion

        #region Font Properties

        public string FontName { get; set; }
        public double FontSize { get; set; }
        public DeciaFontStyle FontStyle { get; set; }
        public bool IsBold { get { return FontStyle.IsBold(); } }
        public bool IsItalic { get { return FontStyle.IsItalic(); } }
        public bool IsUnderline { get { return FontStyle.IsUnderline(); } }
        public bool IsStrikeout { get { return FontStyle.IsStrikeout(); } }
        public bool IsDoubleUnderline { get { return FontStyle.IsDoubleUnderline(); } }
        public Font Font { get { return new Font((!string.IsNullOrWhiteSpace(FontName) ? FontName : DefaultFontName), (float)FontSize, FontStyle.GetNetFontStyle()); } }

        public HAlignment H_Align { get; set; }
        public int H_AlignMode { get { return (int)H_Align; } }
        public VAlignment V_Align { get; set; }
        public int V_AlignMode { get { return (int)V_Align; } }
        public int Indent { get; set; }

        #endregion

        #region BorderStyle Properties

        public BoxStyleValue<BorderLineStyle> BorderStyle { get; set; }
        public int BorderStyle_Left { get { return Padding.Left; } }
        public int BorderStyle_Top { get { return Padding.Top; } }
        public int BorderStyle_Right { get { return Padding.Right; } }
        public int BorderStyle_Bottom { get { return Padding.Bottom; } }

        #endregion

        #region BorderColor_Left Properties

        public ColorSpecification BorderColorSpec_Left { get; set; }
        public int BorderColor_Left_Alpha { get { return BorderColorSpec_Left.Alpha; } }
        public int BorderColor_Left_Red { get { return BorderColorSpec_Left.Red; } }
        public int BorderColor_Left_Green { get { return BorderColorSpec_Left.Green; } }
        public int BorderColor_Left_Blue { get { return BorderColorSpec_Left.Blue; } }

        public Color BorderColor_Left
        {
            get { return BorderColorSpec_Left.GetColor(); }
            set { BorderColorSpec_Left = new ColorSpecification(value); }
        }

        #endregion

        #region BorderColor_Top Properties

        public ColorSpecification BorderColorSpec_Top { get; set; }
        public int BorderColor_Top_Alpha { get { return BorderColorSpec_Top.Alpha; } }
        public int BorderColor_Top_Red { get { return BorderColorSpec_Top.Red; } }
        public int BorderColor_Top_Green { get { return BorderColorSpec_Top.Green; } }
        public int BorderColor_Top_Blue { get { return BorderColorSpec_Top.Blue; } }

        public Color BorderColor_Top
        {
            get { return BorderColorSpec_Top.GetColor(); }
            set { BorderColorSpec_Top = new ColorSpecification(value); }
        }

        #endregion

        #region BorderColor_Right Properties

        public ColorSpecification BorderColorSpec_Right { get; set; }
        public int BorderColor_Right_Alpha { get { return BorderColorSpec_Right.Alpha; } }
        public int BorderColor_Right_Red { get { return BorderColorSpec_Right.Red; } }
        public int BorderColor_Right_Green { get { return BorderColorSpec_Right.Green; } }
        public int BorderColor_Right_Blue { get { return BorderColorSpec_Right.Blue; } }

        public Color BorderColor_Right
        {
            get { return BorderColorSpec_Right.GetColor(); }
            set { BorderColorSpec_Right = new ColorSpecification(value); }
        }

        #endregion

        #region BorderColor_Bottom Properties

        public ColorSpecification BorderColorSpec_Bottom { get; set; }
        public int BorderColor_Bottom_Alpha { get { return BorderColorSpec_Bottom.Alpha; } }
        public int BorderColor_Bottom_Red { get { return BorderColorSpec_Bottom.Red; } }
        public int BorderColor_Bottom_Green { get { return BorderColorSpec_Bottom.Green; } }
        public int BorderColor_Bottom_Blue { get { return BorderColorSpec_Bottom.Blue; } }

        public Color BorderColor_Bottom
        {
            get { return BorderColorSpec_Bottom.GetColor(); }
            set { BorderColorSpec_Bottom = new ColorSpecification(value); }
        }

        #endregion
    }
}