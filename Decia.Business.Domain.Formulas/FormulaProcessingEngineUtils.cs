using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Operations.Time;
using Decia.Business.Domain.Formulas.Operations.Time.Search;

namespace Decia.Business.Domain.Formulas
{
    public static class FormulaProcessingEngineUtils
    {
        public static readonly ICollection<Type> OrderableShiftOperations = new Type[] { typeof(Offset_ByPeriodCount_Operation) };

        public static readonly int ZeroYear = DeciaDataTypeUtils.MinDateTime.Year;
        public static readonly int ZeroDay = (int)Math.Floor(365.0 / 2.0);
        public static readonly DateTime ZeroDate = (new DateTime(ZeroYear, 1, 1)).AddDays(ZeroDay - 1);
        public static TimePeriod ZeroPeriod { get { return GetPeriodForTimeShift(0, 0); } }
        public static readonly int PeriodHourDelta = 1;

        public static Func<DateTime, int> GetTimeD1Shift = ((DateTime shiftAsDate) => (shiftAsDate.Year - ZeroYear));
        public static Func<DateTime, int> GetTimeD2Shift = ((DateTime shiftAsDate) => (shiftAsDate.DayOfYear - ZeroDay));
        public static Func<int, int, DateTime> GetDateForTimeShift = ((int d1Shift, int d2Shift) => ZeroDate.AddYears(d1Shift).AddDays(d2Shift));
        public static Func<int, int, TimePeriod> GetPeriodForTimeShift = ((int d1Shift, int d2Shift) => new TimePeriod(GetDateForTimeShift(d1Shift, d2Shift), GetDateForTimeShift(d1Shift, d2Shift).AddHours(PeriodHourDelta)));


        public static CycleType AnalyzeCycle(this IComputationGroup computationGroup, IFormulaDataProvider dataProvider)
        {
            if (!computationGroup.HasCycle)
            { return CycleType.None; }

            var variableRefsInCycle = computationGroup.CycleGroup.NodesIncluded;
            var variableFormulas = variableRefsInCycle.ToDictionary(v => v, v => dataProvider.GetFormula(v));

            CachedTimeNetwork<ModelObjectReference> timeOrderingNetwork = new CachedTimeNetwork<ModelObjectReference>(variableRefsInCycle, new List<Edge<ModelObjectReference>>());

            foreach (var variableTemplateRef in variableFormulas.Keys)
            {
                var formula = variableFormulas[variableTemplateRef];

                if (formula.IsTimeAggregation)
                { return CycleType.PresentCycle; }

                var countOfUnorderableShiftExpressions = formula.GetUnorderableShiftExpressions().Count;

                if (countOfUnorderableShiftExpressions > 0)
                { return CycleType.PresentCycle; }

                AddReferencesToShiftNetworks(formula, variableTemplateRef, timeOrderingNetwork);
            }

            if (timeOrderingNetwork.TimePeriods.Contains(ZeroPeriod))
            {
                if (timeOrderingNetwork.HasCycles(ZeroPeriod))
                { return CycleType.PresentCycle; }
            }

            var orderedPeriodsMinToMax = timeOrderingNetwork.TimePeriods.OrderBy(tp => tp.StartDate).ToList();
            var orderedNodes = new SortedDictionary<int, ModelObjectReference>();
            var orderedD1Shifts = new SortedDictionary<int, int>();
            var orderedD2Shifts = new SortedDictionary<int, int>();
            var returnValue = CycleType.PastCycle;

            foreach (TimePeriod period in orderedPeriodsMinToMax)
            {
                int d1Shift = GetTimeD1Shift(period.StartDate);
                int d2Shift = GetTimeD2Shift(period.StartDate);

                if (timeOrderingNetwork.HasCycles(period))
                {
                    if ((d1Shift > 0) || (d2Shift > 0))
                    { returnValue = CycleType.FutureCycle; }

                    foreach (var node in timeOrderingNetwork.GetAllNodesForPeriod(period).ToList())
                    {
                        orderedNodes.Add(orderedNodes.Count, node);
                        orderedD1Shifts.Add(orderedD1Shifts.Count, d1Shift);
                        orderedD2Shifts.Add(orderedD2Shifts.Count, d2Shift);
                    }
                }
                else
                {
                    foreach (var node in timeOrderingNetwork.GetDependencyOrderedTraversalFromRoot(period).ToList())
                    {
                        if (timeOrderingNetwork.GetPeriodSpecificIncomingEdgesForPeriod(period, node).Count < 1)
                        { continue; }

                        orderedNodes.Add(orderedNodes.Count, node);
                        orderedD1Shifts.Add(orderedD1Shifts.Count, d1Shift);
                        orderedD2Shifts.Add(orderedD2Shifts.Count, d2Shift);
                    }
                }
            }

            List<ModelObjectReference> orderedVariableRefs = new List<ModelObjectReference>();
            Dictionary<ModelObjectReference, TimeLagSet> variableLags = new Dictionary<ModelObjectReference, TimeLagSet>();

            foreach (int index in orderedNodes.Keys)
            {
                if (orderedVariableRefs.Count == variableRefsInCycle.Count)
                { break; }

                var variableRef = orderedNodes[index];

                var variableLag = new TimeLagSet(orderedD1Shifts[index], orderedD2Shifts[index]);

                orderedVariableRefs.Add(variableRef);
                variableLags.Add(variableRef, variableLag);
            }

            if (orderedVariableRefs.Count != variableRefsInCycle.Count)
            { throw new InvalidOperationException("Could not order all expected references."); }

            bool hasStrictOrdering = (returnValue == CycleType.PastCycle);
            computationGroup.SetTimeOrderedNodes(hasStrictOrdering, orderedVariableRefs, variableLags);

            return returnValue;
        }

        #region GetUnorderableShiftExpressions Methods

        public static IList<IExpression> GetUnorderableShiftExpressions(this IFormula formula)
        {
            List<IExpression> invalidShiftExpressions = new List<IExpression>();
            GetUnorderableShiftExpressions(formula, invalidShiftExpressions);
            return invalidShiftExpressions;
        }

        private static void GetUnorderableShiftExpressions(IFormula formula, List<IExpression> invalidShiftExpressions)
        {
            foreach (var argument in formula.GetArguments_ContainingNestedExpressions())
            {
                var nestedExpression = formula.Expressions[argument.NestedExpressionId];

                if (!nestedExpression.Operation.TimeOperationType.IsShift())
                { continue; }

                if (!OrderableShiftOperations.Contains(nestedExpression.Operation.GetType()))
                {
                    invalidShiftExpressions.Add(nestedExpression);
                    continue;
                }

                if (nestedExpression.OperationId == OperationCatalog.GetOperationId<Offset_ByPeriodCount_Operation>())
                {
                    IArgument timeD1ShiftAmountArg = nestedExpression.ArgumentsByIndex[Offset_ByPeriodCount_Operation.Parameter1.Index];
                    if (timeD1ShiftAmountArg.ArgumentType != ArgumentType.DirectValue)
                    {
                        invalidShiftExpressions.Add(nestedExpression);
                        continue;
                    }

                    if (nestedExpression.ArgumentsByIndex.Count > Offset_ByPeriodCount_Operation.Parameter2.Index)
                    {
                        IArgument timeD2ShiftAmountArg = nestedExpression.ArgumentsByIndex[Offset_ByPeriodCount_Operation.Parameter2.Index];
                        if (timeD2ShiftAmountArg.ArgumentType != ArgumentType.DirectValue)
                        {
                            invalidShiftExpressions.Add(nestedExpression);
                            continue;
                        }
                    }
                }
                else
                { invalidShiftExpressions.Add(nestedExpression); }
            }
        }

        #endregion

        #region AddReferencesToShiftNetworks Methods

        public static void AddReferencesToShiftNetworks(IFormula formula, ModelObjectReference mainVariableTemplateRef, ITimeNetwork<ModelObjectReference> timeOrderingNetwork)
        {
            var currentExpression = formula.RootExpression;
            int currentD1Shift = 0;
            int currentD2Shift = 0;

            AddReferencesToShiftNetworks(formula, mainVariableTemplateRef, timeOrderingNetwork, currentExpression, currentD1Shift, currentD2Shift);
        }

        private static void AddReferencesToShiftNetworks(IFormula formula, ModelObjectReference mainVariableTemplateRef, ITimeNetwork<ModelObjectReference> timeOrderingNetwork, IExpression currentExpression, int currentD1Shift, int currentD2Shift)
        {
            if (currentExpression.OperationId == OperationCatalog.GetOperationId<Offset_ByPeriodCount_Operation>())
            {
                IArgument timeD1ShiftAmountArg = null;
                IArgument timeD2ShiftAmountArg = null;

                bool hasD1Shift = false;
                bool hasD2Shift = false;

                timeD1ShiftAmountArg = currentExpression.ArgumentsByIndex[Offset_ByPeriodCount_Operation.Parameter1.Index];
                if (timeD1ShiftAmountArg.ArgumentType != ArgumentType.DirectValue)
                { throw new InvalidOperationException("Only Direct Value Time Shifts are currently supported for ordering."); }
                hasD1Shift = !timeD1ShiftAmountArg.DirectValue.IsNull;

                if (currentExpression.ArgumentsByIndex.Count > Offset_ByPeriodCount_Operation.Parameter2.Index)
                {
                    timeD2ShiftAmountArg = currentExpression.ArgumentsByIndex[Offset_ByPeriodCount_Operation.Parameter2.Index];
                    if (timeD2ShiftAmountArg.ArgumentType != ArgumentType.DirectValue)
                    { throw new InvalidOperationException("Only Direct Value Time Shifts are currently supported for ordering."); }
                    hasD2Shift = !timeD2ShiftAmountArg.DirectValue.IsNull;
                }

                int d1ShiftDelta = (hasD1Shift) ? timeD1ShiftAmountArg.DirectValue.GetTypedValue<int>() : 0;
                int d2ShiftDelta = (hasD2Shift) ? timeD2ShiftAmountArg.DirectValue.GetTypedValue<int>() : 0;

                currentD1Shift = currentD1Shift + d1ShiftDelta;
                currentD2Shift = currentD2Shift + d2ShiftDelta;
            }


            foreach (var argument in currentExpression.GetArguments_ContainingReferences(EvaluationType.Processing))
            {
                var relatedVariableTemplateRef = argument.ReferencedModelObject;

                if (!timeOrderingNetwork.ForeverNetwork.Nodes.Contains(relatedVariableTemplateRef))
                { continue; }

                TimePeriod shiftPeriod = GetPeriodForTimeShift(currentD1Shift, currentD2Shift);
                Edge<ModelObjectReference> shiftEdge = new Edge<ModelObjectReference>(relatedVariableTemplateRef, mainVariableTemplateRef);
                TimeEdge<ModelObjectReference> shiftTimeEdge = new TimeEdge<ModelObjectReference>(shiftPeriod, shiftEdge);

                bool addEdge = false;
                if (!timeOrderingNetwork.AllEdges.ContainsKey(shiftPeriod))
                { addEdge = true; }
                else
                {
                    if (!timeOrderingNetwork.AllEdges[shiftPeriod].Contains(shiftEdge))
                    { addEdge = true; }
                }

                if (!addEdge)
                { continue; }

                var nodesToAdd = new List<ITimeNode<ModelObjectReference>>();
                nodesToAdd.Add(shiftTimeEdge.OutgoingTimeNode);
                nodesToAdd.Add(shiftTimeEdge.IncomingTimeNode);
                var edgesToAdd = new List<ITimeEdge<ModelObjectReference>>();
                edgesToAdd.Add(shiftTimeEdge);
                timeOrderingNetwork.AddValues(nodesToAdd, edgesToAdd);
            }

            foreach (var argument in currentExpression.GetArguments_ContainingNestedExpressions())
            {
                var nestedExpression = formula.Expressions[argument.NestedExpressionId];
                AddReferencesToShiftNetworks(formula, mainVariableTemplateRef, timeOrderingNetwork, nestedExpression, currentD1Shift, currentD2Shift);
            }
        }

        #endregion
    }
}