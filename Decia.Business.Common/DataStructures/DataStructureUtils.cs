using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.DataStructures
{
    public static class DataStructureUtils
    {
        #region Node Methods

        public static ITimeNode<T> AsForeverNode<T>(this T node)
            where T : struct, IComparable
        {
            return new TimeNode<T>(node);
        }

        public static ITimeNode<T> AsPeriodNode<T>(this T node, TimePeriod period)
            where T : struct, IComparable
        {
            return new TimeNode<T>(period, node);
        }

        public static ICollection<ITimeNode<T>> AsForeverNodes<T>(this IEnumerable<T> nodes)
            where T : struct, IComparable
        {
            return nodes.Select(n => n.AsForeverNode()).ToList();
        }

        public static ICollection<ITimeNode<T>> AsPeriodNodes<T>(this IEnumerable<T> nodes, TimePeriod period)
            where T : struct, IComparable
        {
            return nodes.Select(n => n.AsPeriodNode(period)).ToList();
        }

        public static ICollection<T> CombineNodes<T>(this IEnumerable<T> nodes, IEnumerable<T> otherNodes)
             where T : struct, IComparable
        {
            if (nodes == null)
            { throw new InvalidOperationException("The \"nodes\" list must not be null."); }
            if (otherNodes == null)
            { otherNodes = new List<T>(); }

            List<T> allNodes = nodes.Union(otherNodes).Distinct().ToList();
            return allNodes;
        }

        public static ICollection<ITimeNode<T>> CombineNodes<T>(this IEnumerable<ITimeNode<T>> nodes, IEnumerable<ITimeNode<T>> otherNodes)
             where T : struct, IComparable
        {
            if (nodes == null)
            { throw new InvalidOperationException("The \"nodes\" list must not be null."); }
            if (otherNodes == null)
            { otherNodes = new List<ITimeNode<T>>(); }

            List<ITimeNode<T>> allNodes = nodes.Union(otherNodes).Distinct().ToList();
            return allNodes;
        }

        #endregion

        #region Edge Methods

        public static ITimeEdge<T> AsForeverEdge<T>(this IEdge<T> edge)
            where T : struct, IComparable
        {
            return new TimeEdge<T>(edge);
        }

        public static ITimeEdge<T> AsPeriodEdge<T>(this IEdge<T> edge, TimePeriod period)
            where T : struct, IComparable
        {
            return new TimeEdge<T>(period, edge);
        }

        public static ICollection<ITimeEdge<T>> AsForeverEdges<T>(this IEnumerable<IEdge<T>> edges)
            where T : struct, IComparable
        {
            return edges.Select(e => e.AsForeverEdge()).ToList();
        }

        public static ICollection<ITimeEdge<T>> AsPeriodEdges<T>(this IEnumerable<IEdge<T>> edges, TimePeriod period)
            where T : struct, IComparable
        {
            return edges.Select(e => e.AsPeriodEdge(period)).ToList();
        }

        public static ICollection<IEdge<T>> CombineEdges<T>(this IEnumerable<IEdge<T>> edges, IEnumerable<IEdge<T>> otherEdges)
             where T : struct, IComparable
        {
            if (edges == null)
            { throw new InvalidOperationException("The \"edges\" list must not be null."); }
            if (otherEdges == null)
            { otherEdges = new List<ITimeEdge<T>>(); }

            List<IEdge<T>> allEdges = edges.Union(otherEdges).Distinct().ToList();
            return allEdges;
        }

        public static ICollection<ITimeEdge<T>> CombineEdges<T>(this IEnumerable<ITimeEdge<T>> edges, IEnumerable<ITimeEdge<T>> otherEdges)
             where T : struct, IComparable
        {
            if (edges == null)
            { throw new InvalidOperationException("The \"edges\" list must not be null."); }
            if (otherEdges == null)
            { otherEdges = new List<ITimeEdge<T>>(); }

            List<ITimeEdge<T>> allEdges = edges.Union(otherEdges).Distinct().ToList();
            return allEdges;
        }

        #endregion
    }
}