using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class SpaceResultComparer : IComparer, IComparer<SpaceResult>
    {
        public int Compare(object x, object y)
        {
            return Compare(x as SpaceResult, y as SpaceResult);
        }

        public int Compare(SpaceResult x, SpaceResult y)
        {
            if ((x == null) || (y == null))
            { throw new InvalidOperationException("Null references are not allowed for ProductionSpaceResults."); }

            var xDimResults = x.DimensionResults;
            var yDimResults = y.DimensionResults;

            if (xDimResults.Count != xDimResults.Count)
            { throw new InvalidOperationException("The Dimensions results do not match."); }

            foreach (var xDimIndex in xDimResults.Keys)
            {
                var xDimResult = xDimResults[xDimIndex];
                var yDimResult = yDimResults[xDimIndex];

                if (xDimResult.ParentEnumerator != yDimResult.ParentEnumerator)
                { throw new InvalidOperationException("The Dimensions results do not match."); }

                var dimComp = xDimResult.ParentEnumerator.GetComparer().Compare(xDimResult.ObjectToOrderBy, yDimResult.ObjectToOrderBy);

                if (dimComp != 0)
                { return dimComp; }
            }
            return 0;
        }
    }
}