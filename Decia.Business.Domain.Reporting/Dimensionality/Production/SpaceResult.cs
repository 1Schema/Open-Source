using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class SpaceResult
    {
        protected SpaceEnumerator m_ParentEnumerator;
        protected int m_OriginalResultIndex;
        protected bool m_IsGlobal;
        protected SortedDictionary<int, DimensionResult> m_DimensionResults;

        public SpaceResult(SpaceEnumerator parentEnumerator, int originalResultIndex)
        {
            m_ParentEnumerator = parentEnumerator;
            m_OriginalResultIndex = originalResultIndex;
            m_IsGlobal = true;
            m_DimensionResults = new SortedDictionary<int, DimensionResult>();
        }

        public SpaceResult(SpaceEnumerator parentEnumerator, int originalResultIndex, IEnumerable<DimensionResult> dimensionResults)
            : this(parentEnumerator, originalResultIndex, dimensionResults.ToDictionary(x => x.SortOrder, x => x))
        { }

        public SpaceResult(SpaceEnumerator parentEnumerator, int originalResultIndex, IDictionary<int, DimensionResult> dimensionResults)
        {
            m_ParentEnumerator = parentEnumerator;
            m_OriginalResultIndex = originalResultIndex;
            m_IsGlobal = false;
            m_DimensionResults = new SortedDictionary<int, DimensionResult>(dimensionResults);
        }

        public SpaceEnumerator ParentEnumerator
        {
            get { return m_ParentEnumerator; }
        }

        public int OriginalResultIndex
        {
            get { return m_OriginalResultIndex; }
        }

        public bool IsGlobal
        {
            get { return m_IsGlobal; }
        }

        public SortedDictionary<int, DimensionResult> DimensionResults
        {
            get { return new SortedDictionary<int, DimensionResult>(m_DimensionResults); }
        }

        public SpaceResult GetReorganizedResult(SortedDictionary<int, DimensionEnumerator> groupEnumerators)
        {
            if (IsGlobal)
            { return new SpaceResult(m_ParentEnumerator, m_OriginalResultIndex); }

            var originalResultsByEnumerator = DimensionResults.Values.ToDictionary(x => x.ParentEnumerator, x => x);
            var reorderedResults = new SortedDictionary<int, DimensionResult>();

            foreach (var groupEnumeratorIndex in groupEnumerators.Keys)
            {
                var groupEnumerator = groupEnumerators[groupEnumeratorIndex];
                var groupResult = originalResultsByEnumerator[groupEnumerator];
                reorderedResults.Add(reorderedResults.Count, groupResult);
            }

            var nonGroupEnumerators = DimensionResults.Where(x => !groupEnumerators.Values.Contains(x.Value.ParentEnumerator)).ToDictionary(x => x.Key, x => x.Value.ParentEnumerator);
            var orderedNonGroupEnumerators = new SortedDictionary<int, DimensionEnumerator>(nonGroupEnumerators);

            foreach (var nonGroupEnumeratorIndex in orderedNonGroupEnumerators.Keys)
            {
                var nonGroupEnumerator = orderedNonGroupEnumerators[nonGroupEnumeratorIndex];
                var nonGroupResult = originalResultsByEnumerator[nonGroupEnumerator];
                reorderedResults.Add(reorderedResults.Count, nonGroupResult);
            }

            var reorderedSpaceResult = new SpaceResult(m_ParentEnumerator, m_OriginalResultIndex, reorderedResults);
            return reorderedSpaceResult;
        }
    }
}