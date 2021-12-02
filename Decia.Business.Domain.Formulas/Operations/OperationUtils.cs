using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations
{
    public static class OperationUtils
    {
        #region Assertion Methods

        public static void AssertInputValueIsNotNull(this IOperation operation, ChronometricValue inputValue, int argumentIndex)
        {
            if (inputValue != ChronometricValue.NullInstanceAsObject)
            { return; }
            throw new InvalidOperationException("Null input values are not allowed for the \"" + operation.LongName + "\" Operation for Argument at index " + argumentIndex + ".");
        }

        #endregion

        #region Validation Methods

        public static ITimeDimensionSet GetSelfTimeDimensionality(this IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            if (currentState.VariableTemplateRef.ModelObjectType == ModelObjectType.VariableTemplate)
            { return dataProvider.GetAssessedTimeDimensions(currentState.VariableTemplateRef); }
            else if (currentState.VariableTemplateRef.ModelObjectType == ModelObjectType.AnonymousVariableTemplate)
            { return (dataProvider as IFormulaDataProvider_Anonymous).GetAnonymousVariableTimeDimensions(currentState.VariableTemplateRef); }
            else
            { throw new InvalidOperationException("ModelObjectType not allowed."); }
        }

        public static ITimeDimensionSet GetBestTimeDimensionalityForRef(this IFormulaDataProvider dataProvider, ICurrentState currentState, ModelObjectReference variableTemplateRef)
        {
            if (ModelObjectReference.DimensionalComparer.Equals(currentState.VariableTemplateRef, variableTemplateRef))
            { return GetSelfTimeDimensionality(dataProvider, currentState); }
            else if (dataProvider.GetIsValidated(variableTemplateRef) == true)
            { return dataProvider.GetValidatedTimeDimensions(variableTemplateRef); }
            else
            { return dataProvider.GetAssessedTimeDimensions(variableTemplateRef); }
        }

        public static IDictionary<TimeDimensionType, KeyValuePair<ModelObjectReference?, ITimeDimensionSet>> GetNestedArguments_MostGranularTimeDimensionality(this IFormulaDataProvider dataProvider, ICurrentState currentState, IFormula parentFormula, IExpression callingExpression)
        {
            var allArgsWithRefs = FormulaUtils.GetNestedArguments_ForArgumentType(parentFormula, callingExpression, ArgumentType.ReferencedId, null);
            var allRefs = allArgsWithRefs.Select(x => x.ReferencedModelObject).ToList();

            var uniqueRefs = new HashSet<ModelObjectReference>(allRefs, ModelObjectReference.DimensionalComparer);
            var uniqueTimeDimSets = uniqueRefs.ToDictionary(x => x, x => OperationUtils.GetBestTimeDimensionalityForRef(dataProvider, currentState, x), ModelObjectReference.DimensionalComparer);

            var lastTimeDimSet = (ITimeDimensionSet)TimeDimensionSet.EmptyTimeDimensionSet;

            var results = new Dictionary<TimeDimensionType, KeyValuePair<ModelObjectReference?, ITimeDimensionSet>>();
            results.Add(TimeDimensionType.Primary, new KeyValuePair<ModelObjectReference?, ITimeDimensionSet>(null, lastTimeDimSet));
            results.Add(TimeDimensionType.Secondary, new KeyValuePair<ModelObjectReference?, ITimeDimensionSet>(null, lastTimeDimSet));

            if (uniqueTimeDimSets.Count < 1)
            { return results; }

            for (int i = 0; i < uniqueTimeDimSets.Count; i++)
            {
                var currentBucket = uniqueTimeDimSets.ElementAt(i);
                var currentRef = currentBucket.Key;
                var currentTimeDimSet = currentBucket.Value;

                if (TimeComparisonResult.ThisIsMoreGranular == currentTimeDimSet.PrimaryTimeDimension.CompareTimeTo(lastTimeDimSet.PrimaryTimeDimension))
                { results[TimeDimensionType.Primary] = new KeyValuePair<ModelObjectReference?, ITimeDimensionSet>(currentRef, currentTimeDimSet); }
                if (TimeComparisonResult.ThisIsMoreGranular == currentTimeDimSet.SecondaryTimeDimension.CompareTimeTo(lastTimeDimSet.SecondaryTimeDimension))
                { results[TimeDimensionType.Secondary] = new KeyValuePair<ModelObjectReference?, ITimeDimensionSet>(currentRef, currentTimeDimSet); }

                lastTimeDimSet = currentTimeDimSet;
            }
            return results;
        }

        public static Signature GetExportSignature(IFormulaDataProvider dataProvider, ICurrentState currentState, SqlFormulaInfo exportInfo, IFormula parentFormula, IExpression parentExpression)
        {
            var inputTypes = new Dictionary<int, DeciaDataType>();

            foreach (var argBucket in parentExpression.ArgumentsByIndex)
            {
                var argument = argBucket.Value;
                var argDataType = (DeciaDataType?)null;

                if (argument.ArgumentType == ArgumentType.NestedExpression)
                {
                    var nestedExpression = parentFormula.GetExpression(argument.NestedExpressionId);
                    var nestedSignature = GetExportSignature(dataProvider, currentState, exportInfo, parentFormula, nestedExpression);
                    argDataType = nestedSignature.TypeOut;
                }
                else if (argument.ArgumentType == ArgumentType.ReferencedId)
                {
                    argDataType = dataProvider.GetValidatedDataType(argument.ReferencedModelObject);

                    if (!argDataType.HasValue)
                    { argDataType = dataProvider.GetAssessedDataType(argument.ReferencedModelObject); }
                }
                else if (argument.ArgumentType == ArgumentType.DirectValue)
                {
                    argDataType = argument.DirectValue.DataType;
                }
                else
                { throw new InvalidOperationException("Unsupported ArgumentType encountered."); }

                if (!argDataType.HasValue)
                { throw new InvalidOperationException("Invalid argument value encountered."); }

                inputTypes.Add(argument.ArgumentIndex, argDataType.Value);
            }

            Signature signature;
            bool isValid = parentExpression.Operation.SignatureSpecification.TryGetValidSignature(inputTypes, out signature);
            return signature;
        }

        #endregion

        #region Computation Methods

        public static ICollection<StructuralPoint> GetExpandedKeysForAggregation(IFormulaDataProvider dataProvider, ICurrentState currentState, ICollection<OperationMember> arguments)
        {
            var dimensionValues = new Dictionary<StructuralDimension, HashSet<StructuralCoordinate>>();

            foreach (var arg in arguments)
            {
                foreach (var point in arg.AggregationValues.Keys)
                {
                    foreach (var coordinate in point.Coordinates)
                    {
                        if (!dimensionValues.ContainsKey(coordinate.Dimension))
                        { dimensionValues.Add(coordinate.Dimension, new HashSet<StructuralCoordinate>()); }

                        dimensionValues[coordinate.Dimension].Add(coordinate);
                    }
                }
            }

            var resultingPoints = new List<StructuralPoint>();
            GenerateCombinations(dimensionValues, 0, new List<StructuralCoordinate>(), resultingPoints);
            return resultingPoints;
        }

        private static void GenerateCombinations(Dictionary<StructuralDimension, HashSet<StructuralCoordinate>> dimensionValues, int currentDepth, List<StructuralCoordinate> currentCoordinates, List<StructuralPoint> resultingPoints)
        {
            if (currentDepth >= dimensionValues.Count)
            {
                var point = new StructuralPoint(currentCoordinates);
                resultingPoints.Add(point);
                return;
            }

            var dimension = dimensionValues.Keys.ElementAt(currentDepth);
            var updatedDepth = currentDepth + 1;

            foreach (var coordinate in dimensionValues[dimension])
            {
                var updatedCoordinates = new List<StructuralCoordinate>(currentCoordinates);
                updatedCoordinates.Add(coordinate);

                GenerateCombinations(dimensionValues, updatedDepth, updatedCoordinates, resultingPoints);
            }
        }

        #endregion
    }
}