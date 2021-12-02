using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Layouts
{
    public interface IElementLayout
    {
        bool IsContainer { get; }
        bool IsTransposed { get; }
        RenderingMode RenderingMode { get; set; }

        IDimensionLayout DimensionLayout_X { get; }
        IDimensionLayout DimensionLayout_Y { get; }
        ICollection<Dimension> Dimensions { get; }
        ICollection<int> DimensionNumbers { get; }
        int MinimumDimensionNumber { get; }
        int MaximumDimensionNumber { get; }

        IDimensionLayout GetDimensionLayout(Dimension dimension);
        IDimensionLayout GetDimensionLayout(int dimensionNumber);

        bool HasContentGroups { get; }
        ICollection<Dimension> GetContainerDimensions(ContainerMode desiredMode);
        ICollection<Dimension> GetContainerDimensions_Single();
        ICollection<Dimension> GetContainerDimensions_Grid();

        Size GetDesiredSize(Size defaultSize);
        Size GetDesiredSize(int defaultWidth, int defaultHeight);
    }
}