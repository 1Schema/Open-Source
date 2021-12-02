using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class CommonHeaderResult
    {
        internal CommonHeaderResult(DimensionalRepeatGroup repeatGroup)
        {
            OrderNumber = 0;
            CommonTitleBox = null;
            RepeatGroup = repeatGroup;
            SpaceResult = new SpaceResult(null, 0, new List<DimensionResult>());

            StructuralTitleRanges = new List<IStructuralTitleRange>();
            TimeTitleRanges = new List<ITimeTitleRange>();
        }

        public CommonHeaderResult(int orderNumber, ICommonTitleBox commonTitleBox, ITree<ReportElementId> elementTree, DimensionalRepeatGroup repeatGroup, SpaceResult spaceResult)
        {
            OrderNumber = orderNumber;
            CommonTitleBox = commonTitleBox;
            RepeatGroup = repeatGroup;
            SpaceResult = spaceResult;

            StructuralTitleRanges = commonTitleBox.ContainedStructuralTitleRangeIds.Select(x => elementTree.GetCachedValue<IStructuralTitleRange>(x)).OrderBy(x => x.ZOrder).ToList();
            TimeTitleRanges = commonTitleBox.ContainedTimeTitleRangeIds.Select(x => elementTree.GetCachedValue<ITimeTitleRange>(x)).OrderBy(x => x.ZOrder).ToList();
        }

        public int OrderNumber { get; protected set; }
        public ICommonTitleBox CommonTitleBox { get; protected set; }
        public DimensionalRepeatGroup RepeatGroup { get; protected set; }
        public SpaceResult SpaceResult { get; protected set; }

        public List<IStructuralTitleRange> StructuralTitleRanges { get; protected set; }
        public List<ITimeTitleRange> TimeTitleRanges { get; protected set; }

        public List<IReportElement> ElementsToRender
        {
            get
            {
                var elementsToRender = new List<IReportElement>();
                elementsToRender.AddRange(StructuralTitleRanges);
                elementsToRender.AddRange(TimeTitleRanges);
                elementsToRender = elementsToRender.OrderBy(x => x.ZOrder).ToList();
                return elementsToRender;
            }
        }
    }
}