using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Presentation
{
    public static class VisualizationAssertions
    {
        public const int MinCanvasWidth = 200;
        public const int MinCanvasHeight = 200;
        public const double MinZoomFactor = .1;
        public const double MaxZoomFactor = 20;
        public const int MinElementX = 0;
        public const int MinElementY = 0;
        public const int MinElementWidth = 100;
        public const int MinElementHeight = 50;

        public static void AssertCanvasSizeIsValid(int width, int height)
        {
            if ((width < MinCanvasWidth) || (height < MinCanvasHeight))
            { throw new InvalidOperationException(string.Format("The minimum canvas dimensions are {0}x{1}.", MinCanvasWidth, MinCanvasHeight)); }
        }

        public static void AssertZoomFactorIsValid(double zoomFactor)
        {
            if (zoomFactor < MinZoomFactor)
            { throw new InvalidOperationException(string.Format("The minimum zoom factor is {0}.", MinZoomFactor)); }
            if (zoomFactor > MaxZoomFactor)
            { throw new InvalidOperationException(string.Format("The maximum zoom factor is {0}.", MaxZoomFactor)); }
        }

        public static void AssertElementPositionIsValid(int left, int top)
        {
            if ((left < MinElementX) || (top < MinElementY))
            { throw new InvalidOperationException(string.Format("The minimum element position is ({0}, {1}).", MinElementX, MinElementY)); }
        }

        public static void AssertElementSizeIsValid(int width, int height)
        {
            if ((width < MinElementWidth) || (height < MinElementHeight))
            { throw new InvalidOperationException(string.Format("The minimum element dimensions are {0}x{1}.", MinElementWidth, MinElementHeight)); }
        }
    }
}