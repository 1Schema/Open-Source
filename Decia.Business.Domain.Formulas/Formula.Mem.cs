using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using Decia.Business.Domain.Formulas.Expressions;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula : IValueObject<Formula>
    {
        public static IEqualityComparer<Formula> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<Formula>(); }
        }

        IEqualityComparer<Formula> IValueObject<Formula>.ValueWiseComparer
        {
            get { return Formula.ValueWiseComparer; }
        }

        public override Formula Copy()
        {
            Formula otherObject = new Formula(Key.ProjectGuid, Key.RevisionNumber_NonNull);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual Formula CopyNew()
        {
            Formula otherObject = new Formula(Key.ProjectGuid, Key.RevisionNumber_NonNull);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(Formula otherObject)
        {
            var expressionComparer = EqualityComparer<Expression>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            if (otherObject.m_ParentModelObjectType != m_ParentModelObjectType)
            { otherObject.m_ParentModelObjectType = m_ParentModelObjectType; }
            if (!otherObject.m_ParentModelObjectRefs.AreUnorderedDictionariesEqual(m_ParentModelObjectRefs))
            { otherObject.m_ParentModelObjectRefs = new SortedDictionary<ModelObjectType, ModelObjectReference>(m_ParentModelObjectRefs); }

            if (otherObject.m_RootExpressionGuid != m_RootExpressionGuid)
            { otherObject.m_RootExpressionGuid = m_RootExpressionGuid; }
            if (otherObject.m_ModelObjectRef != m_ModelObjectRef)
            { otherObject.m_ModelObjectRef = m_ModelObjectRef; }

            this.CopyValuesTo(otherObject, expressionComparer);
        }

        public virtual void CopyValuesTo(Formula otherObject)
        {
            var expressionComparer = Expression.ValueWiseComparer;

            this.CopyValuesTo(otherObject, expressionComparer);
        }

        protected virtual void CopyValuesTo(Formula otherObject, IEqualityComparer<Expression> expressionComparer)
        {
            this.CopyBaseValuesTo(otherObject);

            if (otherObject.m_IsNavigationVariable != m_IsNavigationVariable)
            { otherObject.m_IsNavigationVariable = m_IsNavigationVariable; }
            if (otherObject.m_IsStructuralAggregation != m_IsStructuralAggregation)
            { otherObject.m_IsStructuralAggregation = m_IsStructuralAggregation; }
            if (otherObject.m_IsStructuralFilter != m_IsStructuralFilter)
            { otherObject.m_IsStructuralFilter = m_IsStructuralFilter; }

            if (otherObject.m_RootExpressionGuid != m_RootExpressionGuid)
            { otherObject.m_RootExpressionGuid = m_RootExpressionGuid; }

            if (!otherObject.m_Expressions.AreUnorderedDictionariesEqual(m_Expressions, expressionComparer.GetUntypedEqualityComparer()))
            {
                otherObject.m_Expressions = new Dictionary<Guid, Expression>();
                foreach (Expression thisExpression in m_Expressions.Values)
                {
                    Expression otherExpression = new Expression(otherObject.Key, thisExpression.Key.ExpressionGuid, thisExpression.OperationId);
                    thisExpression.CopyValuesTo(otherExpression);
                    otherObject.m_Expressions.Add(otherExpression.Key.ExpressionGuid, otherExpression);
                }
            }
        }

        public override bool Equals(Formula otherObject)
        {
            var expressionComparer = EqualityComparer<Expression>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            if (otherObject.m_ParentModelObjectType != m_ParentModelObjectType)
            { return false; }
            if (!otherObject.m_ParentModelObjectRefs.AreUnorderedDictionariesEqual(m_ParentModelObjectRefs))
            { return false; }

            if (otherObject.m_RootExpressionGuid != m_RootExpressionGuid)
            { return false; }
            if (otherObject.m_ModelObjectRef != m_ModelObjectRef)
            { return false; }

            return this.EqualsValues(otherObject, expressionComparer);
        }

        public virtual bool EqualsValues(Formula otherObject)
        {
            var expressionComparer = Expression.ValueWiseComparer;

            return this.EqualsValues(otherObject, expressionComparer);
        }

        protected virtual bool EqualsValues(Formula otherObject, IEqualityComparer<Expression> expressionComparer)
        {
            if (!this.EqualsBaseValues(otherObject))
            { return false; }

            if (otherObject.m_IsNavigationVariable != m_IsNavigationVariable)
            { return false; }
            if (otherObject.m_IsStructuralAggregation != m_IsStructuralAggregation)
            { return false; }
            if (otherObject.m_IsStructuralFilter != m_IsStructuralFilter)
            { return false; }

            if (otherObject.m_RootExpressionGuid != m_RootExpressionGuid)
            { return false; }

            if (!otherObject.m_Expressions.AreUnorderedDictionariesEqual(m_Expressions, expressionComparer.GetUntypedEqualityComparer()))
            { return false; }
            return true;
        }
    }
}