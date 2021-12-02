using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Presentation
{
    public interface IViewableComponent
    {
        Point ViewPosition { get; }
        Size ViewSize { get; }
        Color ViewColor { get; }

        bool IsViewPositionEditable();
        bool IsViewSizeEditable();
        bool IsViewColorEditable();

        void SetViewPosition(int left, int top);
        void SetViewSize(int width, int height);
        void SetViewColor(int alpha, int red, int green, int blue);
    }

    public static class IViewableComponentUtils
    {
        public static void AssertIsViewPositionEditable(this IViewableComponent view)
        {
            if (!view.IsViewPositionEditable())
            { throw new InvalidOperationException("The selected Viewable Component does now allow its Position to be changed."); }
        }

        public static void AssertIsViewSizeEditable(this IViewableComponent view)
        {
            if (!view.IsViewSizeEditable())
            { throw new InvalidOperationException("The selected Viewable Component does now allow its Size to be changed."); }
        }

        public static void AssertIsViewColorEditable(this IViewableComponent view)
        {
            if (!view.IsViewColorEditable())
            { throw new InvalidOperationException("The selected Viewable Component does now allow its Color to be changed."); }
        }

        public static void SetViewPosition(this IViewableComponent view, Point point)
        {
            view.SetViewPosition(point.X, point.Y);
        }

        public static void SetViewSize(this IViewableComponent view, Size size)
        {
            view.SetViewSize(size.Width, size.Height);
        }

        public static void SetViewColor(this IViewableComponent view, Color color)
        {
            view.SetViewColor(color.A, color.R, color.G, color.B);
        }
    }
}