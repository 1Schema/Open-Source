using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.DataStructures
{
    public interface ICycleGroup<T> : IConvertible
        where T : struct, IComparable
    {
        ICollection<T> NodesIncluded { get; }
        ICollection<IEdge<T>> EdgesIncluded { get; }
    }
}