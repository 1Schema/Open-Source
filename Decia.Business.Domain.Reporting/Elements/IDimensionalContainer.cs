using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public interface IDimensionalContainer : IReadOnlyContainer
    {
        bool UseDynamicDimensionSorting { get; }
        ICollection<ModelObjectReference> Dimensions { get; }
        IDictionary<ModelObjectReference, int> DimensionSortOrders { get; }
        IDictionary<int, ModelObjectReference> SortOrderedDimensions { get; }
        int MinSortOrder { get; }
        int MaxSortOrder { get; }

        bool HasSortOrder(ModelObjectReference dimensionTypeRef);
        void SetSortOrder(ModelObjectReference dimensionTypeRef, int sortOrder);
        void SetToMaxSortOrder(ModelObjectReference dimensionTypeRef);
        void RemoveSortOrder(ModelObjectReference dimensionTypeRef);
        void ClearSortOrders();
    }
}