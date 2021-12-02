using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    public static class ElementOrderingUtils
    {
        public static SortedDictionary<int, T> ProduceOrderedElements<T>(this IEnumerable<T> elements, Dimension orderingDimension)
            where T : IReportElement
        {
            var orderedElements = new SortedDictionary<int, T>();

            foreach (var element in elements.OrderBy(x => GetOrder(x, orderingDimension)).ToList())
            {
                orderedElements.Add(orderedElements.Count + 1, element);
            }
            return orderedElements;
        }

        private static string GetOrder<T>(this T element, Dimension orderingDimension)
              where T : IReportElement
        {
            var layout = element.ElementLayout.GetDimensionLayout(orderingDimension);
            var contentGroup = layout.ContentGroup_Value;
            var location = layout.Margin_Value.LesserSide;

            var orderString = string.Format("{0}, {1}", contentGroup.ToString("D11"), location.ToString("D11"));
            return orderString;
        }

        public static void MoveOrderedDimensionRefs<VALUE>(ref SortedDictionary<int, VALUE> elementsByIndex, int lowerIndex, int upperIndex, int changeInIndex)
            where VALUE : struct
        {
            if (changeInIndex == 0)
            { return; }

            if (changeInIndex > 0)
            {
                for (int keyToMove = upperIndex; keyToMove >= lowerIndex; keyToMove--)
                {
                    if (!elementsByIndex.ContainsKey(keyToMove))
                    { continue; }

                    var valueToMove = elementsByIndex[keyToMove];

                    elementsByIndex.Remove(keyToMove);
                    elementsByIndex.Add(keyToMove + changeInIndex, valueToMove);
                }
            }
            else if (changeInIndex < 0)
            {
                for (int keyToMove = lowerIndex; keyToMove <= upperIndex; keyToMove++)
                {
                    if (!elementsByIndex.ContainsKey(keyToMove))
                    { continue; }

                    var valueToMove = elementsByIndex[keyToMove];

                    elementsByIndex.Remove(keyToMove);
                    elementsByIndex.Add(keyToMove + changeInIndex, valueToMove);
                }
            }
            else
            { throw new InvalidOperationException("The specified change in index is not supported."); }
        }
    }
}