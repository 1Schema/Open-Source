using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class IterationBlock
    {
        protected Guid m_GroupId;
        protected Guid m_BlockId;
        protected int m_BlockNumber;
        protected IterationBlock m_ParentBlock;
        protected SortedDictionary<int, SpaceResult> m_Results;

        public IterationBlock(Guid groupId, int blockNumber, IterationBlock parentBlock, IEnumerable<SpaceResult> results)
        {
            m_GroupId = groupId;
            m_BlockId = Guid.NewGuid();
            m_BlockNumber = blockNumber;
            m_ParentBlock = parentBlock;
            m_Results = new SortedDictionary<int, SpaceResult>();

            for (int i = 0; i < results.Count(); i++)
            {
                m_Results.Add(m_Results.Count, results.ElementAt(i));
            }
        }

        public Guid GroupId { get { return m_GroupId; } }
        public Guid BlockId { get { return m_BlockId; } }
        public int BlockNumber { get { return m_BlockNumber; } }
        public IterationBlock ParentBlock { get { return m_ParentBlock; } }

        public bool HasResults { get { return (m_Results.Count > 0); } }
        public int StartIndex { get { return m_Results.Keys.Min(); } }
        public int EndIndex { get { return m_Results.Keys.Max(); } }
        public int Count { get { return (EndIndex - StartIndex) + 1; } }

        public SortedDictionary<int, SpaceResult> Results
        {
            get { return new SortedDictionary<int, SpaceResult>(m_Results); }
        }
    }
}