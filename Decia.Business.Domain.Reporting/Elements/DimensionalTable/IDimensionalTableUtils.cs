using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;

namespace Decia.Business.Domain.Reporting
{
    public static class IDimensionalTableUtils
    {
        public static void AssertAllRequiredElementsArePresentAndProperlyConfigured(this IDimensionalTable dimensionalTable, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements)
        {
            var reportIdComparer = ReportId.Comparer_Revisionless;
            var elementIdComparer = ReportElementId.Comparer_Revisionless;

            if (dimensionalTable == null)
            { throw new InvalidOperationException("Unrecognized type of DimensionalTable encountered."); }
            if (commonTitleContainer == null)
            { throw new InvalidOperationException("Unrecognized type of CommonTitleContainer encountered."); }
            if (commonTitleElements == null)
            { throw new InvalidOperationException("Invalid null collection of Common Title Elements encountered."); }
            if (commonTitleElements.Contains(null))
            { throw new InvalidOperationException("Unrecognized type of Common Title Element encountered."); }
            if (variableTitleContainer == null)
            { throw new InvalidOperationException("Unrecognized type of VariableTitleContainer encountered."); }
            if (variableTitleElements == null)
            { throw new InvalidOperationException("Invalid null collection of Variable Title Elements encountered."); }
            if (variableTitleElements.Contains(null))
            { throw new InvalidOperationException("Unrecognized type of Variable Title Element encountered."); }
            if (variableDataContainer == null)
            { throw new InvalidOperationException("Unrecognized type of VariableDataContainer encountered."); }
            if (variableDataElements == null)
            { throw new InvalidOperationException("Invalid null collection of Variable Data Elements encountered."); }
            if (variableDataElements.Contains(null))
            { throw new InvalidOperationException("Unrecognized type of Variable Data Element encountered."); }

            if (!commonTitleContainer.ParentReportId.Equals_Revisionless(dimensionalTable.ParentReportId))
            { throw new InvalidOperationException("The CommonTitleContainer specified belongs to a different Report."); }
            if (commonTitleElements.Where(x => !x.ParentReportId.Equals_Revisionless(dimensionalTable.ParentReportId)).Count() > 0)
            { throw new InvalidOperationException("At least one Common Title Element belongs to a different Report."); }
            if (!variableTitleContainer.ParentReportId.Equals_Revisionless(dimensionalTable.ParentReportId))
            { throw new InvalidOperationException("The VariableTitleContainer specified belongs to a different Report."); }
            if (variableTitleElements.Where(x => !x.ParentReportId.Equals_Revisionless(dimensionalTable.ParentReportId)).Count() > 0)
            { throw new InvalidOperationException("At least one Variable Title Element belongs to a different Report."); }
            if (!variableDataContainer.ParentReportId.Equals_Revisionless(dimensionalTable.ParentReportId))
            { throw new InvalidOperationException("The VariableDataContainer specified belongs to a different Report."); }
            if (variableDataElements.Where(x => !x.ParentReportId.Equals_Revisionless(dimensionalTable.ParentReportId)).Count() > 0)
            { throw new InvalidOperationException("At least one Variable Data Element belongs to a different Report."); }

            var commonTitleElementsByNum = commonTitleElements.ToDictionary(x => x.Key.ReportElementNumber, x => x);
            var variableTitleElementsByNum = variableTitleElements.ToDictionary(x => x.Key.ReportElementNumber, x => x);
            var variableDataElementsByNum = variableDataElements.ToDictionary(x => x.Key.ReportElementNumber, x => x);

            if (commonTitleElements.Where(x => (x.ParentElementNumber == commonTitleContainer.Key.ReportElementNumber) || (commonTitleElementsByNum.ContainsKey(x.ParentElementNumber.Value))).Count() != commonTitleElements.Count())
            { throw new InvalidOperationException("At least one Common Title Element is missing its Parent."); }
            if (variableTitleElements.Where(x => (x.ParentElementNumber == variableTitleContainer.Key.ReportElementNumber) || (variableTitleElementsByNum.ContainsKey(x.ParentElementNumber.Value))).Count() != variableTitleElements.Count())
            { throw new InvalidOperationException("At least one Variable Title Element is missing its Parent."); }
            if (variableDataElements.Where(x => (x.ParentElementNumber == variableDataContainer.Key.ReportElementNumber) || (variableDataElementsByNum.ContainsKey(x.ParentElementNumber.Value))).Count() != variableDataElements.Count())
            { throw new InvalidOperationException("At least one Variable Data Element is missing its Parent."); }

            var allCommonTitleContainers = new List<IReadOnlyContainer>();
            allCommonTitleContainers.Add(commonTitleContainer);
            allCommonTitleContainers.AddRange(commonTitleElements.Where(x => (x is IReadOnlyContainer)).Select(x => (IReadOnlyContainer)x).ToList());
            var allVariableTitleContainers = new List<IReadOnlyContainer>();
            allVariableTitleContainers.Add(variableTitleContainer);
            allVariableTitleContainers.AddRange(variableTitleElements.Where(x => (x is IReadOnlyContainer)).Select(x => (IReadOnlyContainer)x).ToList());
            var allVariableDataContainers = new List<IReadOnlyContainer>();
            allVariableDataContainers.Add(variableDataContainer);
            allVariableDataContainers.AddRange(variableDataElements.Where(x => (x is IReadOnlyContainer)).Select(x => (IReadOnlyContainer)x).ToList());

            var validCommonTitleContainers = allCommonTitleContainers.Where(x => x.ChildNumbers.Intersect(commonTitleElementsByNum.Keys).Count() == x.ChildNumbers.Count).ToList();
            if (validCommonTitleContainers.Count != allCommonTitleContainers.Count)
            { throw new InvalidOperationException("At least one Common Title Element is missing its Parent."); }
            var validVariableTitleContainers = allVariableTitleContainers.Where(x => x.ChildNumbers.Intersect(variableTitleElementsByNum.Keys).Count() == x.ChildNumbers.Count).ToList();
            if (validVariableTitleContainers.Count != allVariableTitleContainers.Count)
            { throw new InvalidOperationException("At least one Common Title Element is missing its Parent."); }
            var validVariableDataContainers = allVariableDataContainers.Where(x => x.ChildNumbers.Intersect(variableDataElementsByNum.Keys).Count() == x.ChildNumbers.Count).ToList();
            if (validVariableDataContainers.Count != allVariableDataContainers.Count)
            { throw new InvalidOperationException("At least one Common Title Element is missing its Parent."); }

            foreach (CommonTitleBox commonTitleBox in commonTitleElements.Where(x => (x is CommonTitleBox)))
            {
                if (commonTitleBox.ParentElementNumber != commonTitleContainer.Key.ReportElementNumber)
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
                if (commonTitleBox.ContainedStructuralTitleRangeIds.Where(x => !commonTitleElementsByNum.ContainsKey(x.ReportElementNumber)).Count() > 0)
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
                if (commonTitleBox.ContainedTimeTitleRangeIds.Where(x => !commonTitleElementsByNum.ContainsKey(x.ReportElementNumber)).Count() > 0)
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
            }
            foreach (StructuralTitleRange structuralTitleRange in commonTitleElements.Where(x => (x is StructuralTitleRange)))
            {
                if (structuralTitleRange.IsVariableTitleRelated)
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
                if (!commonTitleElementsByNum.ContainsKey(structuralTitleRange.ParentElementNumber.Value))
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
            }
            foreach (TimeTitleRange timeTitleRange in commonTitleElements.Where(x => (x is TimeTitleRange)))
            {
                if (timeTitleRange.IsVariableTitleRelated)
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
                if (!commonTitleElementsByNum.ContainsKey(timeTitleRange.ParentElementNumber.Value))
                { throw new InvalidOperationException("CommonTitleContainer contains invalid elements."); }
            }
            foreach (VariableTitleBox variableTitleBox in variableTitleElements.Where(x => (x is VariableTitleBox)))
            {
                if (variableTitleBox.ParentElementNumber != variableTitleContainer.Key.ReportElementNumber)
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
                if (!variableTitleElementsByNum.ContainsKey(variableTitleBox.ContainedVariableTitleRangeNumber))
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
                if (variableTitleBox.ContainedStructuralTitleRangeNumbers.Where(x => !variableTitleElementsByNum.ContainsKey(x)).Count() > 0)
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
                if (variableTitleBox.ContainedTimeTitleRangeNumbers.Where(x => !variableTitleElementsByNum.ContainsKey(x)).Count() > 0)
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
            }
            foreach (VariableTitleRange variableTitleRange in variableTitleElements.Where(x => (x is VariableTitleRange)))
            {
                if (!variableTitleElementsByNum.ContainsKey(variableTitleRange.ParentElementNumber.Value))
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
            }
            foreach (StructuralTitleRange structuralTitleRange in variableTitleElements.Where(x => (x is StructuralTitleRange)))
            {
                if (structuralTitleRange.IsCommonTitleRelated)
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
                if (!variableTitleElementsByNum.ContainsKey(structuralTitleRange.ParentElementNumber.Value))
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
            }
            foreach (TimeTitleRange timeTitleRange in variableTitleElements.Where(x => (x is TimeTitleRange)))
            {
                if (timeTitleRange.IsCommonTitleRelated)
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
                if (!variableTitleElementsByNum.ContainsKey(timeTitleRange.ParentElementNumber.Value))
                { throw new InvalidOperationException("VariableTitleContainer contains invalid elements."); }
            }
            foreach (VariableDataBox variableDataBox in variableDataElements.Where(x => (x is VariableDataBox)))
            {
                if (variableDataBox.ParentElementNumber != variableDataContainer.Key.ReportElementNumber)
                { throw new InvalidOperationException("VariableDataContainer contains invalid elements."); }
                if (!commonTitleElementsByNum.ContainsKey(variableDataBox.RelatedCommonTitleBoxNumber))
                { throw new InvalidOperationException("VariableDataContainer contains invalid elements."); }
                if (!variableTitleElementsByNum.ContainsKey(variableDataBox.RelatedVariableTitleBoxNumber))
                { throw new InvalidOperationException("VariableDataContainer contains invalid elements."); }
            }
            foreach (VariableDataRange variableDataRange in variableDataElements.Where(x => (x is VariableDataRange)))
            {
                if (!variableDataElementsByNum.ContainsKey(variableDataRange.ParentElementNumber.Value))
                { throw new InvalidOperationException("VariableDataContainer contains invalid elements."); }
            }
        }

        public static bool DoDimensionalTableElementsHaveMatchingIsTransposedSettings(this IDimensionalTable dimensionalTable, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements)
        {
            if (commonTitleContainer.IsTransposed != dimensionalTable.IsTransposed)
            { return false; }
            if (commonTitleElements.Where(x => (x.IsTransposed != dimensionalTable.IsTransposed)).Count() > 0)
            { return false; }
            if (variableTitleContainer.IsTransposed != dimensionalTable.IsTransposed)
            { return false; }
            if (variableTitleElements.Where(x => (x.IsTransposed != dimensionalTable.IsTransposed)).Count() > 0)
            { return false; }
            if (variableDataContainer.IsTransposed != dimensionalTable.IsTransposed)
            { return false; }
            if (variableDataElements.Where(x => (x.IsTransposed != dimensionalTable.IsTransposed)).Count() > 0)
            { return false; }
            return true;
        }

        public static void AssertDimensionalTableElementsHaveMatchingIsTransposedSettings(this IDimensionalTable dimensionalTable, ICommonTitleContainer commonTitleContainer, IEnumerable<ITransposableElement> commonTitleElements, IVariableTitleContainer variableTitleContainer, IEnumerable<ITransposableElement> variableTitleElements, IVariableDataContainer variableDataContainer, IEnumerable<ITransposableElement> variableDataElements)
        {
            if (!DoDimensionalTableElementsHaveMatchingIsTransposedSettings(dimensionalTable, commonTitleContainer, commonTitleElements, variableTitleContainer, variableTitleElements, variableDataContainer, variableDataElements))
            { throw new InvalidOperationException("The current DimensionTable Elements do not share the same IsTransposed setting."); }
        }
    }
}