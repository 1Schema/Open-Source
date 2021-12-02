using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class CommonHeaderEnumerator
    {
        protected IRenderingState m_RenderingState;
        protected ICommonTitleContainer m_RootContainer;
        protected Dictionary<ReportElementId, ICommonTitleBox> m_CommonTitleBoxes;
        protected SortedDictionary<int, SpaceResult> m_SpaceResults;

        public CommonHeaderEnumerator(IRenderingState renderingState, ICommonTitleContainer rootContainer, IEnumerable<ICommonTitleBox> commonTitleBoxes, IEnumerable<SpaceResult> spaceResults)
        {
            m_RenderingState = renderingState;
            m_RootContainer = rootContainer;
            m_CommonTitleBoxes = commonTitleBoxes.ToDictionary(x => x.Key, x => x, ReportRenderingEngine.EqualityComparer_ReportElementId);
            m_SpaceResults = new SortedDictionary<int, SpaceResult>(spaceResults.ToDictionary(x => x.OriginalResultIndex, x => x));
        }

        public IRenderingState RenderingState
        {
            get { return m_RenderingState; }
        }

        public ReportElementId RootContainerId
        {
            get { return m_RootContainer.Key; }
        }

        public List<SpaceResult> SpaceResults
        {
            get { return m_SpaceResults.Values.ToList(); }
        }

        public List<CommonHeaderResult> EnumerateResults(ITree<ReportElementId> elementTree)
        {
            var containerGroup = RenderingState.GroupingState.ElementRepeatGroups[m_RootContainer.Key];
            var commonHeaderGroup = containerGroup.NestedRepeatGroups.Values.First();

            var firstTitleBox = m_CommonTitleBoxes.Values.First();

            var spaceResults = SpaceResults;
            var commonHeaderResults = new List<CommonHeaderResult>();

            for (int i = 0; i < SpaceResults.Count; i++)
            {
                var spaceResult = spaceResults[i];
                var commonHeaderResult = new CommonHeaderResult(i, firstTitleBox, elementTree, commonHeaderGroup, spaceResult);
                commonHeaderResults.Add(commonHeaderResult);
            }
            return commonHeaderResults;
        }
    }
}