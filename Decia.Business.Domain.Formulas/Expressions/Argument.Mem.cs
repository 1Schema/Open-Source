using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;

namespace Decia.Business.Domain.Formulas.Expressions
{
    public partial class Argument : IValueObject<Argument>
    {
        public static IEqualityComparer<Argument> ValueWiseComparer
        {
            get { return new ValueObjectEqualityComparer<Argument>(); }
        }

        IEqualityComparer<Argument> IValueObject<Argument>.ValueWiseComparer
        {
            get { return Argument.ValueWiseComparer; }
        }

        public override Argument Copy()
        {
            Argument otherObject = new Argument(ExpressionId.DefaultId, ParentOperationId, Key.ArgumentIndex);
            this.CopyTo(otherObject);
            return otherObject;
        }

        public virtual Argument CopyNew()
        {
            Argument otherObject = new Argument(ExpressionId.DefaultId, ParentOperationId, Key.ArgumentIndex);
            this.CopyValuesTo(otherObject);
            return otherObject;
        }

        public override void CopyTo(Argument otherObject)
        {
            if (otherObject.m_Key != m_Key)
            { otherObject.m_Key = m_Key; }

            this.CopyValuesTo(otherObject);
        }

        public virtual void CopyValuesTo(Argument otherObject)
        {
            if (otherObject.m_Key.ArgumentIndex != m_Key.ArgumentIndex)
            { otherObject.m_Key = new ArgumentId(otherObject.m_Key.ExpressionId, m_Key.ArgumentIndex); }
            if (otherObject.m_ArgumentType != m_ArgumentType)
            { otherObject.m_ArgumentType = m_ArgumentType; }
            if (otherObject.m_ParentOperationGuid != m_ParentOperationGuid)
            { otherObject.m_ParentOperationGuid = m_ParentOperationGuid; }
            if (otherObject.m_AutoJoinOrder != m_AutoJoinOrder)
            { otherObject.m_AutoJoinOrder = m_AutoJoinOrder; }
            if (otherObject.m_NestedExpressionId != m_NestedExpressionId)
            { otherObject.m_NestedExpressionId = m_NestedExpressionId; }
            if (otherObject.m_ReferencedModelObject != m_ReferencedModelObject)
            { otherObject.m_ReferencedModelObject = m_ReferencedModelObject; }
            if (otherObject.m_DirectValue != ((object)m_DirectValue))
            { otherObject.m_DirectValue.SetValue(m_DirectValue.DataType, m_DirectValue.GetValue()); }

            if (otherObject.m_IsRefDeleted != m_IsRefDeleted)
            { otherObject.m_IsRefDeleted = m_IsRefDeleted; }
            if (otherObject.m_DeletedRef_StructuralTypeText != m_DeletedRef_StructuralTypeText)
            { otherObject.m_DeletedRef_StructuralTypeText = m_DeletedRef_StructuralTypeText; }
            if (otherObject.m_DeletedRef_VariableTemplateText != m_DeletedRef_VariableTemplateText)
            { otherObject.m_DeletedRef_VariableTemplateText = m_DeletedRef_VariableTemplateText; }
        }

        public override bool Equals(Argument otherObject)
        {
            if (otherObject.m_Key != m_Key)
            { return false; }

            return this.EqualsValues(otherObject);
        }

        public virtual bool EqualsValues(Argument otherObject)
        {
            if (otherObject.m_Key.ArgumentIndex != m_Key.ArgumentIndex)
            { return false; }
            if (otherObject.m_ArgumentType != m_ArgumentType)
            { return false; }
            if (otherObject.m_ParentOperationGuid != m_ParentOperationGuid)
            { return false; }
            if (otherObject.m_AutoJoinOrder != m_AutoJoinOrder)
            { return false; }
            if (otherObject.m_NestedExpressionId != m_NestedExpressionId)
            { return false; }
            if (otherObject.m_ReferencedModelObject != m_ReferencedModelObject)
            { return false; }
            if (otherObject.m_DirectValue != ((object)m_DirectValue))
            { return false; }

            if (otherObject.m_IsRefDeleted != m_IsRefDeleted)
            { return false; }
            if (otherObject.m_DeletedRef_StructuralTypeText != m_DeletedRef_StructuralTypeText)
            { return false; }
            if (otherObject.m_DeletedRef_VariableTemplateText != m_DeletedRef_VariableTemplateText)
            { return false; }

            return true;
        }
    }
}