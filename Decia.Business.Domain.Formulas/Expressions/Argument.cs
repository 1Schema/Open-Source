using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.Formulas.Operations;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Argument : KeyedDomainObjectBase<ArgumentId, Argument>, IArgument
    {
        public static readonly int DefaultArgumentIndex = ArgumentId.EmptyArgumentIndex;
        public static readonly ArgumentType DefaultArgumentType = ArgumentType.DirectValue;
        public static readonly int DefaultAutoJoinOrder = 0;
        public static readonly DeciaDataType InvalidDataType = DeciaDataTypeUtils.InvalidDataType;

        protected ArgumentType m_ArgumentType;
        protected int m_AutoJoinOrder;
        protected Guid m_ParentOperationGuid;
        protected Nullable<ExpressionId> m_NestedExpressionId;
        protected Nullable<ModelObjectReference> m_ReferencedModelObject;
        protected DynamicValue m_DirectValue;

        protected bool? m_IsRefDeleted;
        protected string m_DeletedRef_StructuralTypeText;
        protected string m_DeletedRef_VariableTemplateText;

        public Argument()
            : this(ExpressionId.DefaultId, Expression.DefaultOperationId, DefaultArgumentIndex)
        { }

        public Argument(IExpression parentExpression, int argumentIndex)
            : this(parentExpression.Key, parentExpression.OperationId, argumentIndex)
        { }

        public Argument(ExpressionId parentExpressionId, OperationId parentOperationId, int argumentIndex)
        {
            m_Key = new ArgumentId(parentExpressionId.ProjectGuid, parentExpressionId.RevisionNumber_NonNull, parentExpressionId.FormulaGuid, parentExpressionId.ExpressionGuid, argumentIndex);
            m_ArgumentType = DefaultArgumentType;
            m_AutoJoinOrder = DefaultAutoJoinOrder;
            m_ParentOperationGuid = parentOperationId.OperationGuid;
            m_NestedExpressionId = null;
            m_ReferencedModelObject = null;
            m_DirectValue = new DynamicValue(DynamicValue.DefaultDataType);

            m_IsRefDeleted = null;
            m_DeletedRef_StructuralTypeText = null;
            m_DeletedRef_VariableTemplateText = null;
        }

        [NotMapped]
        public int ArgumentIndex
        {
            get { return m_Key.ArgumentIndex; }
        }

        [NotMapped]
        public ArgumentType ArgumentType
        {
            get { return m_ArgumentType; }
        }

        [NotMapped]
        public ExpressionId ParentExpressionId
        {
            get { return new ExpressionId(Key.ProjectGuid, Key.RevisionNumber_NonNull, Key.FormulaGuid, Key.ExpressionGuid); }
        }

        [NotMapped]
        public OperationId ParentOperationId
        {
            get { return new OperationId(m_ParentOperationGuid); }
        }

        [NotMapped]
        public IOperation ParentOperation
        {
            get { return OperationCatalog.GetOperation(ParentOperationId); }
        }

        [NotMapped]
        public int AutoJoinOrder
        {
            get { return m_AutoJoinOrder; }
            set { m_AutoJoinOrder = value; }
        }

        [NotMapped]
        public ExpressionId NestedExpressionId
        {
            get
            {
                if (m_ArgumentType != ArgumentType.NestedExpression)
                { throw new InvalidOperationException("The specified Argument is not of type \"NestedExpression\""); }
                if (!m_NestedExpressionId.HasValue)
                { throw new InvalidOperationException("The specified Argument is in an invalid state."); }

                return m_NestedExpressionId.Value;
            }
        }

        [NotMapped]
        public ModelObjectReference ReferencedModelObject
        {
            get
            {
                if (m_ArgumentType != ArgumentType.ReferencedId)
                { throw new InvalidOperationException("The specified Argument is not of type \"ReferencedId\""); }
                if (!m_ReferencedModelObject.HasValue)
                { throw new InvalidOperationException("The specified Argument is in an invalid state."); }

                return m_ReferencedModelObject.Value;
            }
        }

        [NotMapped]
        public DynamicValue DirectValue
        {
            get
            {
                if (m_ArgumentType != ArgumentType.DirectValue)
                { throw new InvalidOperationException("The specified Argument is not of type \"DirectValue\""); }
                if (m_DirectValue == DynamicValue.NullInstanceAsObject)
                { throw new InvalidOperationException("The specified Argument is in an invalid state."); }

                return m_DirectValue;
            }
        }

        [NotMapped]
        public ChronometricValue DirectChronometricValue
        {
            get
            {
                DynamicValue directValue = DirectValue;

                ChronometricValue returnValue = new ChronometricValue(Key.ProjectGuid, Key.RevisionNumber_NonNull);
                returnValue.DataType = directValue.DataType;
                returnValue.SetValue(MultiTimePeriodKey.DimensionlessTimeKey, directValue);

                return returnValue;
            }
        }

        public void SetToNestedExpression(ExpressionId expressionId)
        {
            if (ParentOperation.EvaluationType == EvaluationType.PreProcessing)
            { throw new InvalidOperationException("Pre-processed Expressions are not allowed to have nested Expressions."); }

            m_ArgumentType = ArgumentType.NestedExpression;
            m_NestedExpressionId = expressionId;
            m_ReferencedModelObject = null;
            m_DirectValue.ResetToDefault();

            m_IsRefDeleted = null;
            m_DeletedRef_StructuralTypeText = null;
            m_DeletedRef_VariableTemplateText = null;
        }

        public void SetToReferencedId(ModelObjectReference reference)
        {
            m_ArgumentType = ArgumentType.ReferencedId;
            m_NestedExpressionId = null;
            m_ReferencedModelObject = reference;
            m_DirectValue.ResetToDefault();

            m_IsRefDeleted = null;
            m_DeletedRef_StructuralTypeText = null;
            m_DeletedRef_VariableTemplateText = null;
        }

        public void SetToReferencedId(ModelObjectReference reference, Nullable<int> alternateDimensionNumber)
        {
            ModelObjectReference alternateRef = new ModelObjectReference(reference, alternateDimensionNumber);
            SetToReferencedId(alternateRef);
        }

        public void SetToReferencedId(ModelObjectType referencedType, Guid referencedGuid)
        {
            ModelObjectReference reference = new ModelObjectReference(referencedType, referencedGuid);
            SetToReferencedId(reference);
        }

        public void SetToReferencedId(ModelObjectType referencedType, Guid referencedGuid, Nullable<int> alternateDimensionNumber)
        {
            ModelObjectReference reference = new ModelObjectReference(referencedType, referencedGuid, alternateDimensionNumber);
            SetToReferencedId(reference);
        }

        public void SetToDirectValue(DeciaDataType dataType)
        {
            SetToDirectValue(dataType, dataType.GetDefaultForDataType());
        }

        public void SetToDirectValue(DeciaDataType dataType, object value)
        {
            m_ArgumentType = ArgumentType.DirectValue;
            m_NestedExpressionId = null;
            m_ReferencedModelObject = null;
            m_DirectValue.SetValue(dataType, value);

            m_IsRefDeleted = null;
            m_DeletedRef_StructuralTypeText = null;
            m_DeletedRef_VariableTemplateText = null;
        }

        public void DeleteRef(string structuralTypeText, string variableTemplateText)
        {
            if (ArgumentType != ArgumentType.ReferencedId)
            { throw new InvalidOperationException("Cannot delete Ref that does not exist."); }

            m_IsRefDeleted = true;
            m_DeletedRef_StructuralTypeText = structuralTypeText;
            m_DeletedRef_VariableTemplateText = variableTemplateText;
        }

        [NotMapped]
        public bool IsRefDeleted
        {
            get
            {
                if (ArgumentType != ArgumentType.ReferencedId)
                { return false; }
                return (m_IsRefDeleted == true);
            }
        }

        [NotMapped]
        public string DeletedRef_StructuralTypeText
        {
            get { return m_DeletedRef_StructuralTypeText; }
        }

        [NotMapped]
        public string DeletedRef_VariableTemplateText
        {
            get { return m_DeletedRef_VariableTemplateText; }
        }
    }
}