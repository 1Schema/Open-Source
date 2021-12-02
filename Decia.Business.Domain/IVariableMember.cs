using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Domain
{
    public interface IVariableMember : IModelMember
    {
        int VariableTemplateNumber { get; }
        Nullable<Guid> VariableInstanceGuid { get; }
    }

    public interface IVariableMember_Orderable : IVariableMember, IModelMember_Orderable
    { }

    public interface IVariableMember_Deleteable<T> : IModelMember_Deleteable<T>, IVariableMember
        where T : IVariableMember_Deleteable<T>
    { }
}