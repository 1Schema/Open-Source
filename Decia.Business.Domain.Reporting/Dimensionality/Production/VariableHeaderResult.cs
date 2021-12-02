using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.DataStructures;

namespace Decia.Business.Domain.Reporting.Dimensionality.Production
{
    public class VariableHeaderResult
    {
        public VariableHeaderResult(int orderNumber, IVariableTitleBox variableTitleBox, ITree<ReportElementId> elementTree, DimensionalRepeatGroup repeatGroup, IterationBlock iterationBlock, SpaceResult spaceResult)
        {
            OrderNumber = orderNumber;
            VariableTitleBox = variableTitleBox;
            RepeatGroup = repeatGroup;
            IterationBlock = iterationBlock;
            SpaceResult = spaceResult;

            VariableTitleRange = elementTree.GetCachedValue<IVariableTitleRange>(variableTitleBox.ContainedVariableTitleRangeId);
            StructuralTitleRanges = variableTitleBox.ContainedStructuralTitleRangeIds.Select(x => elementTree.GetCachedValue<IStructuralTitleRange>(x)).OrderBy(x => x.ZOrder).ToList();
            TimeTitleRanges = variableTitleBox.ContainedTimeTitleRangeIds.Select(x => elementTree.GetCachedValue<ITimeTitleRange>(x)).OrderBy(x => x.ZOrder).ToList();
        }

        public int OrderNumber { get; protected set; }
        public IVariableTitleBox VariableTitleBox { get; protected set; }
        public DimensionalRepeatGroup RepeatGroup { get; protected set; }
        public IterationBlock IterationBlock { get; protected set; }
        public SpaceResult SpaceResult { get; protected set; }

        public IVariableTitleRange VariableTitleRange { get; protected set; }
        public List<IStructuralTitleRange> StructuralTitleRanges { get; protected set; }
        public List<ITimeTitleRange> TimeTitleRanges { get; protected set; }

        public List<IReportElement> ElementsToRender
        {
            get
            {
                var elementsToRender = new List<IReportElement>();
                elementsToRender.Add(VariableTitleBox);
                elementsToRender.AddRange(StructuralTitleRanges);
                elementsToRender.AddRange(TimeTitleRanges);
                elementsToRender = elementsToRender.OrderBy(x => x.ZOrder).ToList();
                return elementsToRender;
            }
        }
    }
}