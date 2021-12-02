using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Collections;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Expressions;
using Decia.Business.Common.Structure;

namespace Decia.Business.Domain.Formulas
{
    public class ComputationResult
    {
        public const ComputationResultType DefaultResultType = ComputationResultType.FormulaValidationPending;
        public const DeciaDataType DefaultDataType = DeciaDataType.Text;
        public const string DefaultErrorMessage = "No validation has been performed yet.";
        public const string NestedErrorMessage = "Nested error occurred.";

        private ModelObjectReference m_ModelTemplateRef;
        private Nullable<ModelObjectReference> m_ModelInstanceRef;
        private Nullable<FormulaId> m_FormulaId;
        private Nullable<Guid> m_ExpressionGuid;
        private Nullable<int> m_ArgumentIndex;

        private Dictionary<FormulaId, ComputationResult> m_NestedFormulaResults;
        private Dictionary<ExpressionId, ComputationResult> m_NestedExpressionResults;
        private Dictionary<ArgumentId, ComputationResult> m_NestedArgumentResults;

        private ProcessingAcivityType m_MethodType;
        private bool m_IsValid;
        private ComputationResultType m_ResultType;
        private string m_ErrorMessage;

        private List<ModelObjectReference> m_ProcessedVariableTemplates;
        private Nullable<StructuralSpace> m_ResultingSpace;

        private Nullable<DeciaDataType> m_ValidatedDataType;
        private ITimeDimensionSet m_ValidatedTimeDimensionality;
        private CompoundUnit m_ValidatedUnit;
        private ReadOnlyDictionary<StructuralPoint, ChronometricValue> m_ComputedValuesByPoint;

        public event EventHandler IsValidChanged;

        public ComputationResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, ProcessingAcivityType methodType)
            : this(modelTemplateRef, modelInstanceRef, null, null, null, methodType)
        { }

        public ComputationResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, FormulaId formulaId)
            : this(modelTemplateRef, modelInstanceRef, formulaId, null, null, modelInstanceRef.HasValue ? ProcessingAcivityType.Computation : ProcessingAcivityType.Validation)
        { }

        public ComputationResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, FormulaId formulaId, Nullable<Guid> expressionGuid)
            : this(modelTemplateRef, modelInstanceRef, formulaId, expressionGuid, null, modelInstanceRef.HasValue ? ProcessingAcivityType.Computation : ProcessingAcivityType.Validation)
        { }

        public ComputationResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, FormulaId formulaId, Nullable<Guid> expressionGuid, Nullable<int> argumentIndex)
            : this(modelTemplateRef, modelInstanceRef, formulaId, expressionGuid, argumentIndex, modelInstanceRef.HasValue ? ProcessingAcivityType.Computation : ProcessingAcivityType.Validation)
        { }

        protected ComputationResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, Nullable<FormulaId> formulaId, Nullable<Guid> expressionGuid, Nullable<int> argumentIndex, ProcessingAcivityType methodType)
        {
            methodType.AssertIsValidInFormulaProcessing();

            m_ModelTemplateRef = modelTemplateRef;
            m_ModelInstanceRef = modelInstanceRef;
            m_FormulaId = formulaId;
            m_ExpressionGuid = expressionGuid;
            m_ArgumentIndex = argumentIndex;

            m_NestedFormulaResults = new Dictionary<Formulas.FormulaId, ComputationResult>();
            m_NestedExpressionResults = new Dictionary<ExpressionId, ComputationResult>();
            m_NestedArgumentResults = new Dictionary<ArgumentId, ComputationResult>();

            m_MethodType = methodType;
            m_IsValid = false;
            m_ResultType = DefaultResultType;
            m_ErrorMessage = DefaultErrorMessage;

            m_ProcessedVariableTemplates = null;
            m_ResultingSpace = null;

            m_ValidatedDataType = null;
            m_ValidatedTimeDimensionality = null;
            m_ValidatedUnit = null;
            m_ComputedValuesByPoint = null;
        }

        protected bool IsValid_Protected
        {
            get { return m_IsValid; }
            set
            {
                m_IsValid = value;

                if (IsValidChanged != null)
                { IsValidChanged(this, new EventArgs()); }
            }
        }

        #region Related Formula State

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef; }
        }

        public Nullable<ModelObjectReference> ModelInstanceRef
        {
            get { return m_ModelInstanceRef; }
        }

        public Nullable<FormulaId> FormulaId
        {
            get { return m_FormulaId; }
        }

        public Nullable<ExpressionId> ExpressionId
        {
            get
            {
                if (!m_FormulaId.HasValue)
                { return null; }
                if (!m_ExpressionGuid.HasValue)
                { return null; }
                return new ExpressionId(m_FormulaId.Value, m_ExpressionGuid.Value);
            }
        }

        public Nullable<ArgumentId> ArgumentId
        {
            get
            {
                if (!m_FormulaId.HasValue)
                { return null; }
                if (!m_ExpressionGuid.HasValue)
                { return null; }
                if (!m_ArgumentIndex.HasValue)
                { return null; }
                return new ArgumentId(m_FormulaId.Value, m_ExpressionGuid.Value, m_ArgumentIndex.Value);
            }
        }

        public FormulaComponentType FormulaComponentType
        {
            get
            {
                if (m_ArgumentIndex.HasValue)
                { return FormulaComponentType.Argument; }
                if (m_ExpressionGuid.HasValue)
                { return FormulaComponentType.Expression; }
                if (m_FormulaId.HasValue)
                { return FormulaComponentType.Formula; }
                return FormulaComponentType.None;
            }
        }

        public IDictionary<FormulaId, ComputationResult> NestedFormulaResults
        {
            get { return new Dictionary<FormulaId, ComputationResult>(m_NestedFormulaResults); }
        }

        public IDictionary<ExpressionId, ComputationResult> NestedExpressionResults
        {
            get { return new Dictionary<ExpressionId, ComputationResult>(m_NestedExpressionResults); }
        }

        public IDictionary<ArgumentId, ComputationResult> NestedArgumentResults
        {
            get { return new Dictionary<ArgumentId, ComputationResult>(m_NestedArgumentResults); }
        }

        #endregion

        #region Validation Properties

        public ProcessingAcivityType AcivityType
        {
            get { return m_MethodType; }
        }

        public bool IsValid
        {
            get { return IsValid_Protected; }
        }

        public ComputationResultType ResultType
        {
            get { return m_ResultType; }
        }

        public string ErrorMessage
        {
            get { return m_ErrorMessage; }
        }

        #endregion

        public ICollection<ModelObjectReference> ProcessedVariableTemplates
        {
            get
            {
                if (m_ProcessedVariableTemplates == null)
                { return new List<ModelObjectReference>(); }
                return m_ProcessedVariableTemplates.ToList();
            }
        }

        public StructuralSpace ResultingSpace
        {
            get
            {
                if (!m_ResultingSpace.HasValue)
                { return StructuralSpace.GlobalStructuralSpace; }
                return m_ResultingSpace.Value;
            }
        }

        public DeciaDataType ValidatedDataType
        {
            get
            {
                if (!m_ValidatedDataType.HasValue)
                { return DefaultDataType; }
                return m_ValidatedDataType.Value;
            }
        }

        public ITimeDimensionSet ValidatedTimeDimensionality
        {
            get
            {
                if (m_ValidatedTimeDimensionality == null)
                { return TimeDimensionSet.EmptyTimeDimensionSet; }
                return m_ValidatedTimeDimensionality;
            }
        }

        public CompoundUnit ValidatedUnit
        {
            get
            {
                return m_ValidatedUnit;
            }
        }

        public IDictionary<StructuralPoint, ChronometricValue> ComputedValuesByPoint
        {
            get
            {
                if (m_ComputedValuesByPoint == null)
                { return new Dictionary<StructuralPoint, ChronometricValue>(); }
                return m_ComputedValuesByPoint;
            }
        }

        public Nullable<StructuralPoint> FirstComputedPoint
        {
            get
            {
                if (m_ComputedValuesByPoint == null)
                { return null; }
                if (m_ComputedValuesByPoint.Count < 1)
                { return null; }
                return m_ComputedValuesByPoint.Keys.First();
            }
        }

        public ChronometricValue FirstComputedValue
        {
            get
            {
                if (m_ComputedValuesByPoint == null)
                { return null; }
                if (m_ComputedValuesByPoint.Count < 1)
                { return null; }
                return m_ComputedValuesByPoint[FirstComputedPoint.Value];
            }
        }

        internal void AddNestedResult(ComputationResult nestedResult)
        {
            if (nestedResult.FormulaComponentType == FormulaComponentType.Formula)
            {
                m_NestedFormulaResults.Add(nestedResult.FormulaId.Value, nestedResult);
            }
            else if (nestedResult.FormulaComponentType == FormulaComponentType.Expression)
            {
                m_NestedExpressionResults.Add(nestedResult.ExpressionId.Value, nestedResult);
            }
            else if (nestedResult.FormulaComponentType == FormulaComponentType.Argument)
            {
                m_NestedArgumentResults.Add(nestedResult.ArgumentId.Value, nestedResult);
            }
            else
            { throw new InvalidOperationException("Model results must be root-level results."); }
        }

        internal void SetErrorState(ComputationResultType resultType, string errorMessage)
        {
            IsValid_Protected = false;
            m_ResultType = resultType;
            m_ErrorMessage = errorMessage;
        }

        internal void SetErrorState(ComputationResult nestedResult)
        {
            IsValid_Protected = false;
            m_ResultType = ComputationResultType.NestedErrorOccurred;
            m_ErrorMessage = NestedErrorMessage;

            AddNestedResult(nestedResult);
        }

        internal void SetModelInitializedState()
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetInitializedState()
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetModelValidatedState()
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetValidatedState(ComputationResult nestedResult)
        {
            IsValid_Protected = nestedResult.IsValid_Protected;
            m_ResultType = nestedResult.m_ResultType;
            m_ErrorMessage = nestedResult.m_ErrorMessage;

            m_ProcessedVariableTemplates = nestedResult.m_ProcessedVariableTemplates;
            m_ResultingSpace = nestedResult.m_ResultingSpace;

            m_ValidatedDataType = nestedResult.m_ValidatedDataType;
            m_ValidatedTimeDimensionality = nestedResult.m_ValidatedTimeDimensionality;
            m_ValidatedUnit = nestedResult.m_ValidatedUnit;

            AddNestedResult(nestedResult);
        }

        internal void SetValidatedState(StructuralSpace resultingSpace, IEnumerable<ModelObjectReference> processedTemplates, DeciaDataType dataType, ITimeDimensionSet timeDimensionality)
        {
            SetValidatedState(resultingSpace, processedTemplates, dataType, timeDimensionality, null);
        }

        internal void SetValidatedState(StructuralSpace resultingSpace, IEnumerable<ModelObjectReference> processedTemplates, DeciaDataType dataType, ITimeDimensionSet timeDimensionality, CompoundUnit unit)
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;

            m_ProcessedVariableTemplates = processedTemplates.ToList();
            m_ResultingSpace = resultingSpace;

            m_ValidatedDataType = dataType;
            m_ValidatedTimeDimensionality = timeDimensionality;
            m_ValidatedUnit = unit;
        }

        internal void SetModelComputedState()
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetComputedState(ComputationResult nestedResult)
        {
            IsValid_Protected = nestedResult.IsValid_Protected;
            m_ResultType = nestedResult.m_ResultType;
            m_ErrorMessage = nestedResult.m_ErrorMessage;

            m_ProcessedVariableTemplates = nestedResult.m_ProcessedVariableTemplates;
            m_ResultingSpace = nestedResult.m_ResultingSpace;

            m_ValidatedDataType = nestedResult.m_ValidatedDataType;
            m_ValidatedTimeDimensionality = nestedResult.m_ValidatedTimeDimensionality;
            m_ValidatedUnit = nestedResult.m_ValidatedUnit;

            m_ComputedValuesByPoint = nestedResult.m_ComputedValuesByPoint;

            AddNestedResult(nestedResult);
        }

        internal void SetComputedState(StructuralSpace resultingSpace, IEnumerable<ModelObjectReference> processedTemplates, StructuralPoint computedPoint, ChronometricValue computedValue)
        {
            SetComputedState(resultingSpace, processedTemplates, computedPoint, computedValue, null);
        }

        internal void SetComputedState(StructuralSpace resultingSpace, IEnumerable<ModelObjectReference> processedTemplates, StructuralPoint computedPoint, ChronometricValue computedValue, CompoundUnit unit)
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;

            m_ProcessedVariableTemplates = processedTemplates.ToList();
            m_ResultingSpace = resultingSpace;

            m_ValidatedDataType = computedValue.DataType;
            m_ValidatedTimeDimensionality = computedValue.TimeDimensionSet;
            m_ValidatedUnit = unit;

            m_ComputedValuesByPoint = new ReadOnlyDictionary<StructuralPoint, ChronometricValue>();
            m_ComputedValuesByPoint.Add(computedPoint, computedValue);
            m_ComputedValuesByPoint.IsReadOnly = true;
        }

        internal void SetComputedState(StructuralSpace resultingSpace, IEnumerable<ModelObjectReference> processedTemplates, DeciaDataType dataType, IDictionary<StructuralPoint, ChronometricValue> computedValuesByPoint)
        {
            SetComputedState(resultingSpace, processedTemplates, dataType, computedValuesByPoint, null);
        }

        internal void SetComputedState(StructuralSpace resultingSpace, IEnumerable<ModelObjectReference> processedTemplates, DeciaDataType dataType, IDictionary<StructuralPoint, ChronometricValue> computedValuesByPoint, CompoundUnit unit)
        {
            IsValid_Protected = true;
            m_ResultType = ComputationResultType.Ok;
            m_ErrorMessage = string.Empty;

            m_ProcessedVariableTemplates = processedTemplates.ToList();
            m_ResultingSpace = resultingSpace;

            m_ValidatedDataType = dataType;
            m_ValidatedUnit = unit;

            m_ComputedValuesByPoint = new ReadOnlyDictionary<StructuralPoint, ChronometricValue>(computedValuesByPoint);
            m_ComputedValuesByPoint.IsReadOnly = true;
            m_ValidatedTimeDimensionality = (FirstComputedValue != ChronometricValue.NullInstanceAsObject) ? FirstComputedValue.TimeDimensionSet : TimeDimensionSet.EmptyTimeDimensionSet;
        }
    }
}