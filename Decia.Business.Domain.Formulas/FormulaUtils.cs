using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Structure;
using Decia.Business.Domain.Time;

namespace Decia.Business.Domain.Formulas
{
    public static class FormulaUtils
    {
        public static void UpdateNavigationReference(this IStructuralMap structuralMap, ModelObjectReference modelInstanceRef, ModelObjectReference variableTemplateRef, ModelObjectReference sourceStructuralInstanceRef, ModelObjectReference destinationEntityTypeRef, Nullable<Guid> destinationStructuralInstanceGuid, Nullable<TimePeriod> navigationTimePeriod)
        {
            if (destinationEntityTypeRef.ModelObjectType != ModelObjectType.EntityType)
            { throw new InvalidOperationException("Only EntityTypes can be used in Navigation Variables."); }

            var updatedExtendedCoordinates = new Dictionary<ModelObjectReference, IDictionary<StructuralDimension, Nullable<StructuralCoordinate>>>(ModelObjectReference.DimensionalComparer);
            updatedExtendedCoordinates.Add(sourceStructuralInstanceRef, new Dictionary<StructuralDimension, Nullable<StructuralCoordinate>>());

            var navigationSpace = structuralMap.GetBaseStructuralSpace(destinationEntityTypeRef);

            if (!destinationStructuralInstanceGuid.HasValue)
            {
                foreach (var dimension in navigationSpace.Dimensions)
                { updatedExtendedCoordinates[sourceStructuralInstanceRef].Add(dimension, null); }
            }
            else
            {
                var destinationStructuralInstanceRef = new ModelObjectReference(ModelObjectType.EntityInstance, destinationStructuralInstanceGuid.Value);
                var navigationPoint = structuralMap.GetBaseStructuralPoint(modelInstanceRef, destinationStructuralInstanceRef);

                foreach (var dimensionBucket in navigationPoint.CoordinatesByDimension)
                { updatedExtendedCoordinates[sourceStructuralInstanceRef].Add(dimensionBucket.Key, dimensionBucket.Value); }
            }

            if (sourceStructuralInstanceRef.ModelObjectType == ModelObjectType.EntityInstance)
            {
                structuralMap.UpdateExtendedCoordinates(modelInstanceRef, navigationTimePeriod, updatedExtendedCoordinates, null);
            }
            else if (sourceStructuralInstanceRef.ModelObjectType == ModelObjectType.RelationInstance)
            {
                structuralMap.UpdateExtendedCoordinates(modelInstanceRef, navigationTimePeriod, null, updatedExtendedCoordinates);
            }
        }

        public static StructuralContext BuildStructuralContext(this Formula formula, IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            bool useExtendedStructure = currentState.UseExtendedStructure;
            bool isStructuralAggregation = ((formula.RootExpression != null) ? formula.RootExpression.Operation.StructuralOperationType.IsAggregation() : false);
            bool isStructuralFilter = (formula.Expressions.Values.Where(exp => (exp.Operation.StructuralOperationType.IsFilter())).ToList().Count > 0);
            bool expectUniqueness = (!(isStructuralAggregation || isStructuralFilter));
            bool isComputation = (currentState.AcivityType == ProcessingAcivityType.Computation);

            if (formula.IsNavigationVariable && isComputation)
            {
                if (currentState.StructuralInstanceRef.ModelObjectType == ModelObjectType.EntityInstance)
                {
                    dataProvider.StructuralMap.RemoveExtendedCoordinates(currentState.ModelInstanceRef, currentState.NavigationPeriod, currentState.StructuralInstanceRef.ToEnumerable(), new ModelObjectReference[] { });
                }
                else if (currentState.StructuralInstanceRef.ModelObjectType == ModelObjectType.RelationInstance)
                {
                    dataProvider.StructuralMap.RemoveExtendedCoordinates(currentState.ModelInstanceRef, currentState.NavigationPeriod, new ModelObjectReference[] { }, currentState.StructuralInstanceRef.ToEnumerable());
                }
            }


            StructuralSpace expectedSpace = dataProvider.StructuralMap.GetStructuralSpace(currentState.StructuralTypeRef, useExtendedStructure);

            StructuralPoint? expectedPoint = null;
            if (isComputation)
            { expectedPoint = dataProvider.StructuralMap.GetStructuralPoint(currentState.ModelInstanceRef, currentState.NavigationPeriod, currentState.StructuralInstanceRef, useExtendedStructure); }

            IList<IArgument> arguments = formula.GetArguments_ContainingReferences(EvaluationType.Processing);
            List<ModelObjectReference> variableTypeRefs = arguments.Select(arg => (!arg.IsRefDeleted) ? arg.ReferencedModelObject : dataProvider.DependencyMap.GetIdVariableTemplate(ModelObjectReference.GlobalTypeReference)).ToList();
            List<ModelObjectReference> structuralTypeRefs = variableTypeRefs.Select(vtr => dataProvider.GetStructuralType(vtr)).ToList();

            Dictionary<ModelObjectReference, bool> originalStructuralTypeRefsAreBound = new Dictionary<ModelObjectReference, bool>(ModelObjectReference.DimensionalComparer);
            foreach (ModelObjectReference structuralTypeRef in structuralTypeRefs)
            {
                if (originalStructuralTypeRefsAreBound.ContainsKey(structuralTypeRef))
                { continue; }

                bool isBound = !formula.IsNavigationVariable;
                originalStructuralTypeRefsAreBound.Add(structuralTypeRef, isBound);
            }

            Dictionary<ModelObjectReference, bool> structuralTypeRefsAreBound = new Dictionary<ModelObjectReference, bool>(originalStructuralTypeRefsAreBound, ModelObjectReference.DimensionalComparer);
            if (structuralTypeRefsAreBound.ContainsKey(currentState.StructuralTypeRef))
            { structuralTypeRefsAreBound.Remove(currentState.StructuralTypeRef); }


            StructuralSpace? tempResultingSpace = dataProvider.StructuralMap.GetRelativeStructuralSpace(currentState.StructuralTypeRef, structuralTypeRefsAreBound.Keys, useExtendedStructure);
            if (!tempResultingSpace.HasValue)
            { return null; }

            StructuralSpace resultingSpace = tempResultingSpace.Value;
            if (expectUniqueness)
            {
                bool isUnique = resultingSpace.IsRelatedAndMoreGeneral(dataProvider.StructuralMap, expectedSpace, useExtendedStructure);

                if (!isUnique)
                { throw new InvalidOperationException("The current Formula requires Aggregation."); }
            }

            var structuralContext = new StructuralContext(dataProvider.StructuralMap, currentState.ModelTemplateRef, currentState.NullableModelInstanceRef);
            structuralContext.SetResultingSpace(resultingSpace, expectUniqueness, structuralTypeRefsAreBound.Keys);

            if (!isComputation)
            { return structuralContext; }

            IDictionary<ListBasedKey<ModelObjectReference>, StructuralPoint> resultingPoints = new Dictionary<ListBasedKey<ModelObjectReference>, StructuralPoint>();
            int maxDimension = 1;
            if (structuralTypeRefsAreBound.Count > 0)
            {
                maxDimension = structuralTypeRefsAreBound.Keys.Select(r => r.AlternateDimensionNumber.HasValue ? r.AlternateDimensionNumber.Value : 1).Max();
            }

            if (!isStructuralFilter)
            {
                resultingPoints = dataProvider.StructuralMap.GetRelativeStructuralPoints(currentState.ModelInstanceRef, currentState.NavigationPeriod, currentState.StructuralInstanceRef, structuralTypeRefsAreBound, useExtendedStructure);
            }
            else
            {
                var structuralTypeRef = currentState.StructuralTypeRef;
                var usesLessGeneralData = originalStructuralTypeRefsAreBound.Keys.Where(x => dataProvider.StructuralMap.IsAccessible(structuralTypeRef, x, useExtendedStructure) && !dataProvider.StructuralMap.IsUnique(structuralTypeRef, x, useExtendedStructure)).HasContents();

                var allStructuralInstanceRefs = (ICollection<ModelObjectReference>)(new ModelObjectReference[] { currentState.StructuralInstanceRef });
                if (usesLessGeneralData)
                { allStructuralInstanceRefs = dataProvider.StructuralMap.GetStructuralInstancesForType(currentState.ModelInstanceRef, currentState.StructuralTypeRef); }

                foreach (var structuralInstanceRef in allStructuralInstanceRefs)
                {
                    structuralTypeRefsAreBound = new Dictionary<ModelObjectReference, bool>(originalStructuralTypeRefsAreBound, ModelObjectReference.DimensionalComparer);
                    if (structuralTypeRefsAreBound.ContainsKey(structuralTypeRef))
                    { structuralTypeRefsAreBound.Remove(structuralTypeRef); }

                    var dimensionPoints = dataProvider.StructuralMap.GetRelativeStructuralPoints(currentState.ModelInstanceRef, currentState.NavigationPeriod, structuralInstanceRef, structuralTypeRefsAreBound, useExtendedStructure);

                    foreach (var dimensionPoint in dimensionPoints)
                    {
                        if (resultingPoints.ContainsKey(dimensionPoint.Key))
                        { continue; }
                        if (resultingPoints.Values.Contains(dimensionPoint.Value))
                        { continue; }

                        resultingPoints.Add(dimensionPoint.Key, dimensionPoint.Value);
                    }
                }
            }

            structuralContext.SetResultingPoints(resultingPoints);
            return structuralContext;
        }

        public static StructuralContext BuildAnonymousStructuralContext(this Formula formula, IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            var useExtendedStructure = currentState.UseExtendedStructure;
            var isUnique = true;
            var isComputation = (currentState.AcivityType == ProcessingAcivityType.Computation);

            var rootStructuralSpace = dataProvider.StructuralMap.GetStructuralSpace(currentState.StructuralTypeRef, useExtendedStructure);
            var localStructuralSpace = currentState.StructuralSpace;
            var totalStructuralSpace = new StructuralSpace(rootStructuralSpace, localStructuralSpace.Dimensions);

            var localRelatedTypeRefs = localStructuralSpace.Dimensions.Select(x => new ModelObjectReference(ModelObjectType.EntityType, x.EntityTypeNumber, x.EntityDimensionNumber)).ToHashSet(ModelObjectReference.DimensionalComparer);

            foreach (var referenceArg in formula.GetArguments_ContainingReferences(EvaluationType.Processing))
            {
                var variableTemplateRef = referenceArg.ReferencedModelObject;
                var structuralTypeRef = dataProvider.DependencyMap.GetStructuralType(variableTemplateRef);

                var isAccessible = dataProvider.StructuralMap.IsDirectlyAccessibleUsingSpace(currentState.StructuralTypeRef, structuralTypeRef, totalStructuralSpace, currentState.UseExtendedStructure);

                if (!isAccessible)
                { return null; }
            }

            var structuralContext = new StructuralContext(dataProvider.StructuralMap, currentState.ModelTemplateRef, currentState.NullableModelInstanceRef);
            structuralContext.SetResultingSpace(totalStructuralSpace, isUnique, localRelatedTypeRefs);

            if (!isComputation)
            { return structuralContext; }

            var rootStructuralPoint = dataProvider.StructuralMap.GetStructuralPoint(currentState.ModelInstanceRef, currentState.NavigationPeriod, currentState.StructuralInstanceRef, useExtendedStructure);
            var localStructuralPoint = currentState.StructuralPoint;
            var totalStructuralPoint = new StructuralPoint(rootStructuralPoint, localStructuralPoint.Coordinates);

            var rootInstanceRefs = rootStructuralPoint.Coordinates.Select(x => new ModelObjectReference(ModelObjectType.EntityInstance, x.EntityInstanceGuid, x.EntityDimensionNumber)).ToHashSet(ModelObjectReference.DimensionalComparer);
            var localRelatedInstanceRefs = localStructuralPoint.Coordinates.Select(x => new ModelObjectReference(ModelObjectType.EntityInstance, x.EntityInstanceGuid, x.EntityDimensionNumber)).ToHashSet(ModelObjectReference.DimensionalComparer);

            var allRelatedInstanceRefs = new List<ModelObjectReference>();
            allRelatedInstanceRefs.AddRange(rootInstanceRefs);
            allRelatedInstanceRefs.AddRange(localRelatedInstanceRefs);

            var totalKey = new ListBasedKey<ModelObjectReference>(allRelatedInstanceRefs.ToHashSet(ModelObjectReference.DimensionalComparer), ModelObjectReference.DimensionalComparer);
            var totalPointsByKey = new Dictionary<ListBasedKey<ModelObjectReference>, StructuralPoint>();
            totalPointsByKey.Add(totalKey, totalStructuralPoint);

            structuralContext.SetResultingPoints(totalPointsByKey, localRelatedInstanceRefs);
            return structuralContext;
        }

        internal static bool MeetsMinimalExpressionConditions(this IFormula formula, ComputationResult result)
        {
            if (!formula.RootExpressionId.HasValue)
            {
                result.SetErrorState(ComputationResultType.NullFormula, "The Root Expression of the Formula is null.");
                return false;
            }
            if (formula.RootExpression == null)
            {
                result.SetErrorState(ComputationResultType.NullFormula, "The Root Expression of the Formula is null.");
                return false;
            }
            if (formula.RootExpression.Operation == null)
            {
                result.SetErrorState(ComputationResultType.NullFormula, "The Root Expression's Operation is not set.");
                return false;
            }
            if (!MeetsShiftConditions(formula, DimensionType.Structure, result))
            { return false; }
            if (!MeetsShiftConditions(formula, DimensionType.Time, result))
            { return false; }
            return true;
        }

        internal static bool MeetsShiftConditions(this IFormula formula, DimensionType dimensionType, ComputationResult result)
        {
            var childParentExpressions = new Dictionary<Guid, Guid>();
            var expressionGuidsWithShift = new HashSet<Guid>();

            var expressionsToHandle = new List<IExpression>();
            expressionsToHandle.Add(formula.RootExpression);

            while (expressionsToHandle.Count > 0)
            {
                var currentExpression = expressionsToHandle.First();
                var currentExpressionId = currentExpression.Key;
                var currentExpressionGuid = currentExpressionId.ExpressionGuid;
                var currentOperation = currentExpression.Operation;
                var currentOperationType = (dimensionType == DimensionType.Structure) ? currentOperation.StructuralOperationType : currentOperation.TimeOperationType;

                expressionsToHandle.Remove(currentExpression);
                var parentExpressionGuid = childParentExpressions.ContainsKey(currentExpressionGuid) ? childParentExpressions[currentExpressionGuid] : Guid.Empty;

                if (currentOperationType == OperationType.Shift)
                {
                    if (expressionGuidsWithShift.Contains(parentExpressionGuid))
                    {
                        result.SetErrorState(ComputationResultType.ShiftNotAllowedError, string.Format("Multiple levels of nested {0} Shifts are not allowed in Formula.", dimensionType));
                        return false;
                    }
                    else
                    { expressionGuidsWithShift.Add(currentExpression.Key.ExpressionGuid); }
                }

                var nestedExpressions = currentExpression.ArgumentsById.Values.Where(x => x.ArgumentType == ArgumentType.NestedExpression).Select(x => formula.GetExpression(x.NestedExpressionId)).ToList();

                foreach (var nestedExpression in nestedExpressions)
                {
                    var nestedExpressionId = nestedExpression.Key;
                    var nestedExpressionGuid = nestedExpressionId.ExpressionGuid;

                    childParentExpressions.Add(nestedExpressionGuid, currentExpressionGuid);
                    expressionsToHandle.Add(nestedExpression);
                }
            }
            return true;
        }

        internal static bool MeetsStructuralAggregationConditions(this IFormula formula, ComputationResult result)
        {
            if (formula.IsStructuralAggregation)
            {
                if (formula.RootExpression.Operation.StructuralOperationType.IsAggregation() || formula.RootExpression.Operation.StructuralOperationType.IsFilter())
                {
                    var nonRootExpressions = formula.Expressions.Values.Where(exp => exp.Key != formula.RootExpression.Key).ToList();
                    if (nonRootExpressions.Where(exp => exp.Operation.StructuralOperationType.IsAggregation()).Count() > 0)
                    {
                        result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Structural Aggregation Operators can only be used at the Root Expression level.");
                        return false;
                    }
                }
                else
                {
                    result.SetErrorState(ComputationResultType.AggregationRequiredError, "Structural Aggregation Formulas must contain Structural Aggregation Operator at the root Expression level.");
                    return false;
                }
            }
            else
            {
                if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.StructuralOperationType.IsAggregation()).Count() > 0)
                {
                    result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Non Structural Aggregation Formulas are not allowed to contain Structural Aggregation Operators.");
                    return false;
                }
                if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.StructuralOperationType.IsFilter()).Count() > 0)
                {
                    result.SetErrorState(ComputationResultType.FilterNotAllowedError, "Non Structural Aggregation Formulas are not allowed to contain Structural Filter Operators.");
                    return false;
                }
            }
            return true;
        }

        internal static bool MeetsStructuralFilterConditions(this IFormula formula, IFormulaDataProvider dataProvider, ICurrentState currentState, ComputationResult result)
        {
            if (formula.IsStructuralFilter)
            {
                if (!(formula.RootExpression.Operation.StructuralOperationType.IsAggregation() || formula.RootExpression.Operation.StructuralOperationType.IsFilter()))
                {
                    result.SetErrorState(ComputationResultType.AggregationRequiredError, "Structural Filter Formulas must contain Structural Aggregation Operator or Structural Filter Operator at the root Expression level.");
                    return false;
                }
                var nonRootExpressions = formula.Expressions.Values.Where(exp => exp.Key != formula.RootExpression.Key).ToList();
                if (nonRootExpressions.Where(exp => exp.Operation.StructuralOperationType.IsAggregation()).Count() > 0)
                {
                    result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Structural Aggregation Operators can only be used at the Root Expression level.");
                    return false;
                }

                if (!currentState.ComputeByPeriod)
                {
                    Nullable<bool> isRootVariableTemplateValidated = dataProvider.GetIsValidated(currentState.VariableTemplateRef);
                    if (!isRootVariableTemplateValidated.HasValue)
                    {
                        result.SetErrorState(ComputationResultType.FormulaValidationPending, "The Formula requires Validation prior to Computation.");
                        return false;
                    }
                    if (!(isRootVariableTemplateValidated.HasValue == true))
                    {
                        result.SetErrorState(ComputationResultType.FormulaIsInvalid, "The Formula failed Validation.");
                        return false;
                    }

                    ITimeDimensionSet validatedDimensionality = dataProvider.GetValidatedTimeDimensions(currentState.VariableTemplateRef);
                    if (validatedDimensionality.PrimaryTimeDimension.HasTimeValue || validatedDimensionality.SecondaryTimeDimension.HasTimeValue)
                    {
                        result.SetErrorState(ComputationResultType.FormulaRequiresPeriodSpecificComputation, "The Formula must be computed Period by Period.");
                        return false;
                    }
                }
            }
            else
            {
                if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.StructuralOperationType.IsFilter()).Count() > 0)
                {
                    result.SetErrorState(ComputationResultType.FilterNotAllowedError, "Non Structural Filter Formulas are not allowed to contain Structural Filter Operators.");
                    return false;
                }
            }
            return true;
        }

        internal static bool MeetsTimeAggregationConditions(this IFormula formula, ComputationResult result)
        {
            if (!formula.IsTimeAggregation)
            {
                if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.TimeOperationType.IsAggregation()).Count() > 0)
                {
                    result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Non Time Aggregation Formulas are not allowed to contain Time Aggregation Operators.");
                    return false;
                }
                if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.TimeOperationType.IsFilter()).Count() > 0)
                {
                    result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Non Time Aggregation Formulas are not allowed to contain Time Filter Operators.");
                    return false;
                }
            }
            if (formula.IsTimeAggregation && formula.IsTimeShift)
            {
                result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Time Aggregation Formulas are not allowed to contain Time Shifts.");
                return false;
            }
            if (formula.IsTimeFilter && formula.IsTimeShift)
            {
                result.SetErrorState(ComputationResultType.FilterNotAllowedError, "Time Filter Formulas are not allowed to contain Time Shifts.");
                return false;
            }
            return true;
        }

        internal static bool MeetsAnonymousFormulaConditions(this IFormula formula, ComputationResult result)
        {
            if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.StructuralOperationType.IsAggregation()).Count() > 0)
            {
                result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Non Structural Aggregation Formulas are not allowed to contain Structural Aggregation Operators.");
                return false;
            }
            if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.StructuralOperationType.IsFilter()).Count() > 0)
            {
                result.SetErrorState(ComputationResultType.FilterNotAllowedError, "Non Structural Aggregation Formulas are not allowed to contain Structural Filter Operators.");
                return false;
            }
            if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.TimeOperationType.IsAggregation()).Count() > 0)
            {
                result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Non Time Aggregation Formulas are not allowed to contain Time Aggregation Operators.");
                return false;
            }
            if (formula.Expressions.Select(exp => exp.Value.Operation).Where(op => op.TimeOperationType.IsFilter()).Count() > 0)
            {
                result.SetErrorState(ComputationResultType.AggregationNotAllowedError, "Non Time Aggregation Formulas are not allowed to contain Time Filter Operators.");
                return false;
            }
            return true;
        }

        internal static bool RequiresComputationByPeriod(this IFormula formula, IFormulaDataProvider dataProvider, ICurrentState currentState, StructuralSpace structuralSpace, out bool isRequirementFromStructuralContext)
        {
            isRequirementFromStructuralContext = false;

            if (formula.IsStructuralFilter)
            { return true; }
            if (formula.IsTimeShift)
            { return true; }
            if (formula.IsTimeIntrospection)
            { return true; }

            var timeDimensions = dataProvider.GetValidatedTimeDimensions(currentState.VariableTemplateRef);
            if (timeDimensions == null)
            { timeDimensions = dataProvider.GetAssessedTimeDimensions(currentState.VariableTemplateRef); }

            bool hasPrimaryTimeDimension = timeDimensions.PrimaryTimeDimension.HasTimeValue;
            bool hasSecondaryTimeDimension = timeDimensions.SecondaryTimeDimension.HasTimeValue;

            if (formula.IsNavigationVariable && (hasPrimaryTimeDimension || hasSecondaryTimeDimension))
            { return true; }

            bool anyVarsReferencedHaveTimeDimension_Primary = false;
            bool anyVarsReferencedHaveTimeDimension_Secondary = false;
            bool anyVarsReferencedHaveTimeDimension = false;
            bool navigationVarsReferencedHaveTimeDimension = false;

            foreach (IArgument argument in formula.GetArguments_ContainingReferences(EvaluationType.Processing))
            {
                var argTimeDimensions = dataProvider.GetValidatedTimeDimensions(argument.ReferencedModelObject);
                if (argTimeDimensions == null)
                { argTimeDimensions = dataProvider.GetAssessedTimeDimensions(argument.ReferencedModelObject); }

                bool argHasPrimaryTimeDimension = argTimeDimensions.PrimaryTimeDimension.HasTimeValue;
                bool argHasSecondaryTimeDimension = argTimeDimensions.SecondaryTimeDimension.HasTimeValue;
                bool argHasTimeDimension = (argHasPrimaryTimeDimension || argHasSecondaryTimeDimension);

                if (argHasPrimaryTimeDimension)
                { anyVarsReferencedHaveTimeDimension_Primary = true; }
                if (argHasSecondaryTimeDimension)
                { anyVarsReferencedHaveTimeDimension_Secondary = true; }
                if (argHasTimeDimension)
                { anyVarsReferencedHaveTimeDimension = true; }

                bool isNavigationVar = dataProvider.IsNavigationVariable(argument.ReferencedModelObject);

                if (isNavigationVar && argHasTimeDimension)
                { navigationVarsReferencedHaveTimeDimension = true; }

                if (anyVarsReferencedHaveTimeDimension && navigationVarsReferencedHaveTimeDimension)
                { break; }
            }

            var dependenciesHaveSameTimeDimensionality = ((anyVarsReferencedHaveTimeDimension_Primary == hasPrimaryTimeDimension) && (anyVarsReferencedHaveTimeDimension_Secondary == hasSecondaryTimeDimension));
            if (formula.IsTimeAggregation && anyVarsReferencedHaveTimeDimension && dependenciesHaveSameTimeDimensionality)
            { return true; }

            bool navigationVarsUsedHaveTimeDimension = false;
            foreach (var dimension in structuralSpace.Dimensions)
            {
                if (dimension.UsesTimeDimension)
                {
                    navigationVarsUsedHaveTimeDimension = true;
                    break;
                }
            }

            if (formula.IsNavigationVariable && anyVarsReferencedHaveTimeDimension)
            { return true; }
            if (navigationVarsUsedHaveTimeDimension)
            {
                isRequirementFromStructuralContext = true;
                return true;
            }

            return false;
        }

        #region Formula - Get Argmuments For Type Methods

        public static IList<IArgument> GetArguments_ContainingNestedExpressions(this IFormula formula)
        {
            return formula.GetArguments_ForArgumentType(ArgumentType.NestedExpression, EvaluationType.Processing);
        }

        public static IList<IArgument> GetArguments_ContainingReferences(this IFormula formula, EvaluationType evaluationType)
        {
            return formula.GetArguments_ForArgumentType(ArgumentType.ReferencedId, evaluationType);
        }

        public static IList<IArgument> GetArguments_ContainingDirectValues(this IFormula formula)
        {
            return formula.GetArguments_ForArgumentType(ArgumentType.DirectValue, EvaluationType.Processing);
        }

        public static IList<IArgument> GetArguments_ForArgumentType(this IFormula formula, ArgumentType argumentType, EvaluationType? evaluationType)
        {
            if (formula == null)
            { throw new InvalidOperationException("The Formula must not be null."); }

            List<IArgument> arguments = formula.Arguments.Values.Where(arg => (arg.ArgumentType == argumentType) && (!evaluationType.HasValue || (arg.ParentOperation.EvaluationType == evaluationType))).OrderBy(arg => arg.AutoJoinOrder).ToList();
            return arguments;
        }

        #endregion

        #region Formula - Get Nested Argmuments For Type Methods

        public static IList<IArgument> GetNestedArguments_ContainingNestedExpressions(this IFormula formula, IExpression expression)
        {
            return formula.GetNestedArguments_ForArgumentType(expression, ArgumentType.NestedExpression, EvaluationType.Processing);
        }

        public static IList<IArgument> GetNestedArguments_ContainingReferences(this IFormula formula, IExpression expression, EvaluationType evaluationType)
        {
            return formula.GetNestedArguments_ForArgumentType(expression, ArgumentType.ReferencedId, evaluationType);
        }

        public static IList<IArgument> GetNestedArguments_ContainingDirectValues(this IFormula formula, IExpression expression)
        {
            return formula.GetNestedArguments_ForArgumentType(expression, ArgumentType.DirectValue, EvaluationType.Processing);
        }

        public static IList<IArgument> GetNestedArguments_ForArgumentType(this IFormula formula, IExpression expression, ArgumentType argumentType, EvaluationType? evaluationType)
        {
            var argsOfType = new List<IArgument>();
            GetNestedArguments_ForArgumentType(formula, expression, argumentType, evaluationType, argsOfType);
            return argsOfType;
        }

        private static void GetNestedArguments_ForArgumentType(this IFormula formula, IExpression expression, ArgumentType argumentType, EvaluationType? evaluationType, List<IArgument> argsOfType)
        {
            foreach (var arg in expression.ArgumentsByIndex.Values)
            {
                if ((arg.ArgumentType == argumentType) && (!evaluationType.HasValue || (arg.ParentOperation.EvaluationType == evaluationType)))
                { argsOfType.Add(arg); }

                if (arg.ArgumentType == ArgumentType.NestedExpression)
                {
                    var nestedExpression = formula.GetExpression(arg.NestedExpressionId);
                    GetNestedArguments_ForArgumentType(formula, nestedExpression, argumentType, evaluationType, argsOfType);
                }
            }
        }

        #endregion

        #region Expression - Get Argmuments For Type Methods

        public static IList<IArgument> GetArguments_ContainingNestedExpressions(this IExpression expression)
        {
            return expression.GetArguments_ForArgumentType(ArgumentType.NestedExpression, EvaluationType.Processing);
        }

        public static IList<IArgument> GetArguments_ContainingReferences(this IExpression expression, EvaluationType evaluationType)
        {
            return expression.GetArguments_ForArgumentType(ArgumentType.ReferencedId, evaluationType);
        }

        public static IList<IArgument> GetArguments_ContainingDirectValues(this IExpression expression)
        {
            return expression.GetArguments_ForArgumentType(ArgumentType.DirectValue, EvaluationType.Processing);
        }

        public static IList<IArgument> GetArguments_ForArgumentType(this IExpression expression, ArgumentType argumentType, EvaluationType? evaluationType)
        {
            if (expression == null)
            { throw new InvalidOperationException("The Expression must not be null."); }

            List<IArgument> arguments = expression.ArgumentsById.Values.Where(arg => (arg.ArgumentType == argumentType) && (!evaluationType.HasValue || (arg.ParentOperation.EvaluationType == evaluationType))).OrderBy(arg => arg.AutoJoinOrder).ToList();
            return arguments;
        }

        #endregion
    }
}