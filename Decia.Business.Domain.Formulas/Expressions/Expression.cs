using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas.Operations;
using Decia.Business.Domain.Formulas.Operations.Math;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Expression : KeyedDomainObjectBase<ExpressionId, Expression>, IExpression
    {
        public static readonly DeciaDataType InvalidDataType = DeciaDataTypeUtils.InvalidDataType;
        public static readonly OperationId DefaultOperationId = OperationCatalog.GetOperationId<NoOpOperation>();

        protected OperationId m_OperationId;
        protected bool m_ShowAsOperator;
        protected int m_OuterParenthesesCount;
        protected Dictionary<int, Argument> m_Arguments;

        public Expression()
            : this(FormulaId.DefaultId, DefaultOperationId)
        { }

        public Expression(FormulaId formulaId, OperationId operationId)
            : this(formulaId, Guid.NewGuid(), operationId)
        { }

        protected internal Expression(FormulaId formulaId, Guid expressionGuid, OperationId operationId)
        {
            m_Key = new ExpressionId(formulaId.ProjectGuid, formulaId.RevisionNumber_NonNull, formulaId.FormulaGuid, expressionGuid);
            m_OperationId = operationId;
            m_ShowAsOperator = false;
            m_OuterParenthesesCount = 0;
            m_Arguments = new Dictionary<int, Argument>();
        }

        [NotMapped]
        public OperationId OperationId
        {
            get { return m_OperationId; }
        }

        [NotMapped]
        public IOperation Operation
        {
            get { return OperationCatalog.GetOperation(OperationId); }
        }

        [NotMapped]
        public bool ShowAsOperator
        {
            get { return m_ShowAsOperator; }
            set { m_ShowAsOperator = value; }
        }

        [NotMapped]
        public int OuterParenthesesCount
        {
            get { return m_OuterParenthesesCount; }
            set { m_OuterParenthesesCount = value; }
        }

        [NotMapped]
        public ICollection<ArgumentId> ArgumentIds
        {
            get { return m_Arguments.Keys.Select(arg => new ArgumentId(m_Key, arg)).ToList(); }
        }

        [NotMapped]
        public IDictionary<ArgumentId, IArgument> ArgumentsById
        {
            get { return m_Arguments.ToDictionary(kvp => new ArgumentId(m_Key, kvp.Key), kvp => (IArgument)kvp.Value); }
        }

        [NotMapped]
        public IDictionary<int, IArgument> ArgumentsByIndex
        {
            get { return m_Arguments.ToDictionary(kvp => kvp.Key, kvp => (IArgument)kvp.Value); }
        }

        public bool HasArgument(int argumentIndex)
        {
            return m_Arguments.ContainsKey(argumentIndex);
        }

        public ArgumentId CreateArgument(int argumentIndex)
        {
            Argument argument = new Argument(this, argumentIndex);
            m_Arguments.Add(argument.Key.ArgumentIndex, argument);
            return argument.Key;
        }

        public IArgument GetArgument(int argumentIndex)
        {
            ArgumentId argumentId = new ArgumentId(this.Key, argumentIndex);
            return GetArgument(argumentId);
        }

        public IArgument GetArgument(ArgumentId argumentId)
        {
            if (!m_Arguments.ContainsKey(argumentId.ArgumentIndex))
            { throw new InvalidOperationException("The specified ArgumentId does not exist in the Formula."); }

            return m_Arguments[argumentId.ArgumentIndex];
        }

        public void DeleteArgument(int argumentIndex)
        {
            ArgumentId argumentId = new ArgumentId(this.Key, argumentIndex);
            DeleteArgument(argumentId);
        }

        public void DeleteArgument(ArgumentId argumentId)
        {
            if (!m_Arguments.ContainsKey(argumentId.ArgumentIndex))
            { throw new InvalidOperationException("The specified ArgumentId does not exist in the Formula."); }

            m_Arguments.Remove(argumentId.ArgumentIndex);
        }
    }
}