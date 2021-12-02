using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Collections;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression : IValueObject<Expression>
    {
        public static IEqualityComparer<Expression> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<Expression>(); }
        }

        IEqualityComparer<Expression> IValueObject<Expression>.ValueWiseComparer
        {
            get { return Expression.ValueWiseComparer; }
        }

        public override Expression Copy()
        {
            Expression otherObject = new Expression(FormulaId.DefaultId, Key.ExpressionGuid, OperationId);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual Expression CopyNew()
        {
            Expression otherObject = new Expression(FormulaId.DefaultId, Key.ExpressionGuid, OperationId);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(Expression otherObject)
        {
            var argumentComparer = EqualityComparer<Argument>.Default;

            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject, argumentComparer);
        }

        public virtual void CopyValuesTo(Expression otherObject)
        {
            var argumentComparer = Argument.ValueWiseComparer;

            this.CopyValuesTo(otherObject, argumentComparer);
        }

        protected virtual void CopyValuesTo(Expression otherObject, IEqualityComparer<Argument> argumentComparer)
        {
            if (otherObject.m_Key.ExpressionGuid != m_Key.ExpressionGuid)
            { otherObject.m_Key = new ExpressionId(otherObject.m_Key.FormulaId, m_Key.ExpressionGuid); }
            if (otherObject.m_OperationId != m_OperationId)
            { otherObject.m_OperationId = m_OperationId; }
            if (otherObject.m_ShowAsOperator != m_ShowAsOperator)
            { otherObject.m_ShowAsOperator = m_ShowAsOperator; }
            if (otherObject.m_OuterParenthesesCount != m_OuterParenthesesCount)
            { otherObject.m_OuterParenthesesCount = m_OuterParenthesesCount; }
            if (!otherObject.m_Arguments.AreUnorderedDictionariesEqual(m_Arguments, argumentComparer.GetUntypedEqualityComparer()))
            {
                otherObject.m_Arguments = new Dictionary<int, Argument>();
                foreach (Argument thisArgument in m_Arguments.Values)
                {
                    Argument otherArgument = new Argument(otherObject.Key, thisArgument.ParentOperationId, thisArgument.ArgumentIndex);
                    thisArgument.CopyValuesTo(otherArgument);
                    otherObject.m_Arguments.Add(otherArgument.Key.ArgumentIndex, otherArgument);
                }
            }
        }

        public override bool Equals(Expression otherObject)
        {
            var argumentComparer = EqualityComparer<Argument>.Default;

            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject, argumentComparer);
        }

        public virtual bool EqualsValues(Expression otherObject)
        {
            var argumentComparer = Argument.ValueWiseComparer;

            return this.EqualsValues(otherObject, argumentComparer);
        }

        protected virtual bool EqualsValues(Expression otherObject, IEqualityComparer<Argument> argumentComparer)
        {
            if (otherObject.m_Key.ExpressionGuid != m_Key.ExpressionGuid)
            { return false; }
            if (otherObject.m_OperationId != m_OperationId)
            { return false; }
            if (otherObject.m_ShowAsOperator != m_ShowAsOperator)
            { return false; }
            if (otherObject.m_OuterParenthesesCount != m_OuterParenthesesCount)
            { return false; }
            if (!otherObject.m_Arguments.AreUnorderedDictionariesEqual(m_Arguments, argumentComparer.GetUntypedEqualityComparer()))
            { return false; }
            return true;
        }
    }
}