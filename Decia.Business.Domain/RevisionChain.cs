using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;

namespace Decia.Business.Domain
{
    public class RevisionChain
    {
        public const long EarliestRevisionNumber = 0;

        #region Members

        protected RevisionData m_MaxRevisionData;
        protected long m_MinRevisionNumber;
        protected ReadOnlyHashSet<long> m_DisallowedRevisionNumbers;
        protected ReadOnlySortedDictionary<long, bool> m_RevisionInclusionStates;

        protected Nullable<long> m_DesiredRevisionNumber;

        #endregion

        #region Constructors

        public RevisionChain(RevisionData maxRevisionData, IEnumerable<long> disallowedRevisionNumbers)
            : this(maxRevisionData, EarliestRevisionNumber, disallowedRevisionNumbers)
        { }

        public RevisionChain(RevisionData maxRevisionData, long minRevisionNumber, IEnumerable<long> disallowedRevisionNumbers)
        {
            maxRevisionData.AssertIsValid();
            var maxRevisionNumber = maxRevisionData.RevisionNumber_Current;

            AssertRevisionRangeIsValid(minRevisionNumber, maxRevisionNumber);
            AssertEarliestRevisionIsInChain(disallowedRevisionNumbers);
            AssertMaxRevisionIsInChain(disallowedRevisionNumbers, maxRevisionNumber);

            m_MaxRevisionData = maxRevisionData;
            m_MinRevisionNumber = minRevisionNumber;
            m_RevisionInclusionStates = null;
            m_DisallowedRevisionNumbers = new ReadOnlyHashSet<long>(disallowedRevisionNumbers);

            m_DesiredRevisionNumber = null;
        }

        public RevisionChain(RevisionData maxRevisionData, IDictionary<long, bool> revisionInclusionStates)
            : this(maxRevisionData, EarliestRevisionNumber, revisionInclusionStates)
        { }

        public RevisionChain(RevisionData maxRevisionData, long minRevisionNumber, IDictionary<long, bool> revisionInclusionStates)
        {
            maxRevisionData.AssertIsValid();
            var maxRevisionNumber = maxRevisionData.RevisionNumber_Current;

            AssertRevisionRangeIsValid(minRevisionNumber, maxRevisionNumber);
            AssertEarliestRevisionIsInChain(revisionInclusionStates);
            AssertMaxRevisionIsInChain(revisionInclusionStates, maxRevisionNumber);

            var min = revisionInclusionStates.Keys.Min();
            var max = revisionInclusionStates.Keys.Max();
            var expectedCount = (maxRevisionNumber - minRevisionNumber) + 1;
            var actualCount = revisionInclusionStates.Count;

            if (minRevisionNumber != min)
            { throw new InvalidOperationException("Revisions are missing from the chain."); }
            if (maxRevisionNumber != max)
            { throw new InvalidOperationException("Revisions are missing from the chain."); }
            if (expectedCount != actualCount)
            { throw new InvalidOperationException("Revisions are missing from the chain."); }

            m_MaxRevisionData = maxRevisionData;
            m_MinRevisionNumber = minRevisionNumber;
            m_RevisionInclusionStates = new ReadOnlySortedDictionary<long, bool>(revisionInclusionStates);
            m_DisallowedRevisionNumbers = new ReadOnlyHashSet<long>(m_RevisionInclusionStates.Where(x => !x.Value).Select(x => x.Key));

            m_DesiredRevisionNumber = null;
        }

        #endregion

        #region Properties

        public Guid ProjectGuid { get { return m_MaxRevisionData.ProjectGuid; } }
        public long DesiredRevisionNumber
        {
            get
            {
                if (!m_DesiredRevisionNumber.HasValue)
                { return MaxRevisionNumber; }
                return m_DesiredRevisionNumber.Value;
            }
            set
            {
                AssertRevisionIsAllowed(value);
                m_DesiredRevisionNumber = value;
            }
        }

        public bool HasDesiredRevisionData
        {
            get { return (DesiredRevisionData != null); }
        }

        public RevisionData DesiredRevisionData
        {
            get
            {
                if (!m_DesiredRevisionNumber.HasValue || (m_DesiredRevisionNumber.Value == m_MaxRevisionData.RevisionNumber_Current))
                { return m_MaxRevisionData; }
                return null;
            }
        }

        public ProjectMemberId DesiredProjectMemberId
        {
            get { return new ProjectMemberId(ProjectGuid, DesiredRevisionNumber); }
        }

        public long? PreviousRevisionNumber
        {
            get
            {
                for (long previousRevisionNumber = DesiredRevisionNumber - 1; previousRevisionNumber >= EarliestRevisionNumber; previousRevisionNumber--)
                {
                    if (!DisallowedRevisions.Contains(previousRevisionNumber))
                    { return previousRevisionNumber; }
                }
                return null;
            }
        }
        public ProjectMemberId? PreviousProjectMemberId
        {
            get
            {
                var previousRevisionNumber = PreviousRevisionNumber;

                if (!previousRevisionNumber.HasValue)
                { return null; }
                return new ProjectMemberId(ProjectGuid, previousRevisionNumber.Value);
            }
        }

        public long MinRevisionNumber { get { return m_MinRevisionNumber; } }
        public long MaxRevisionNumber { get { return m_MaxRevisionData.RevisionNumber_Current; } }
        public ReadOnlyHashSet<long> DisallowedRevisions { get { return m_DisallowedRevisionNumbers; } }

        public bool StoresMinimalInfo { get { return (m_RevisionInclusionStates == null); } }

        public ReadOnlySortedDictionary<long, bool> RevisionInclusions
        {
            get
            {
                if (!StoresMinimalInfo)
                { return m_RevisionInclusionStates; }

                var revisionInclusions = new ReadOnlySortedDictionary<long, bool>();
                for (long i = MinRevisionNumber; i <= MaxRevisionNumber; i++)
                {
                    revisionInclusions.Add(i, !m_DisallowedRevisionNumbers.Contains(i));
                }
                return revisionInclusions;
            }
        }

        #endregion

        #region Methods

        public List<long> GetIncludedRevisionsForRange(long minRevisionNumber, long maxRevisionNumber)
        {
            AssertRevisionRangeIsValid(minRevisionNumber, maxRevisionNumber);
            var revisionNumbers = new List<long>();

            for (long revisionNumber = minRevisionNumber; revisionNumber <= maxRevisionNumber; revisionNumber++)
            {
                if (IsRevisionAllowed(revisionNumber))
                { revisionNumbers.Add(revisionNumber); }
            }
            return revisionNumbers;
        }

        public bool IsRevisionDisallowed(long desiredRevision)
        {
            return DisallowedRevisions.Contains(desiredRevision);
        }

        public bool IsRevisionAllowed(long desiredRevision)
        {
            return !IsRevisionDisallowed(desiredRevision);
        }

        public void AssertRevisionIsDisallowed(long desiredRevision)
        {
            if (!IsRevisionDisallowed(desiredRevision))
            { throw new InvalidOperationException("The Revision Numbers is allowed."); }
        }

        public void AssertRevisionIsAllowed(long desiredRevision)
        {
            if (!IsRevisionAllowed(desiredRevision))
            { throw new InvalidOperationException("The Revision Numbers is not allowed."); }
        }

        public void AssertRevisionRangeIsValid(long minRevisionNumber, long maxRevisionNumber)
        {
            if (minRevisionNumber < EarliestRevisionNumber)
            { throw new InvalidOperationException("The Min Revision number is invalid."); }
            if (minRevisionNumber > maxRevisionNumber)
            { throw new InvalidOperationException("The specified Revision range is not valid."); }
        }

        public static void AssertEarliestRevisionIsInChain(IEnumerable<long> disallowedRevisions)
        {
            if (disallowedRevisions.Contains(EarliestRevisionNumber))
            { throw new InvalidOperationException("It is not possible to disallow the Earliest Revision."); }
        }

        public static void AssertMaxRevisionIsInChain(IEnumerable<long> disallowedRevisions, long maxRevisionNumber)
        {
            if (disallowedRevisions.Contains(maxRevisionNumber))
            { throw new InvalidOperationException("It is not possible to disallow the Earliest Revision."); }
        }

        public static void AssertEarliestRevisionIsInChain(IDictionary<long, bool> revisionInclusions)
        {
            if (!revisionInclusions.ContainsKey(EarliestRevisionNumber))
            { throw new InvalidOperationException("It is not possible to disallow the Earliest Revision."); }
            if (revisionInclusions[EarliestRevisionNumber] == false)
            { throw new InvalidOperationException("It is not possible to disallow the Earliest Revision."); }
        }

        public static void AssertMaxRevisionIsInChain(IDictionary<long, bool> revisionInclusions, long maxRevisionNumber)
        {
            if (!revisionInclusions.ContainsKey(maxRevisionNumber))
            { throw new InvalidOperationException("It is not possible to disallow the Earliest Revision."); }
            if (revisionInclusions[maxRevisionNumber] == false)
            { throw new InvalidOperationException("It is not possible to disallow the Earliest Revision."); }
        }

        #endregion
    }
}