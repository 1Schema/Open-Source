using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain
{
    public interface IModelMember : IProjectMember, IModelObjectWithRef
    {
        bool IsInstance { get; }
        int ModelTemplateNumber { get; }
        Nullable<Guid> ModelInstanceGuid { get; }
    }

    public interface IModelMember_Orderable : IModelMember, IOrderable
    { }

    public interface IModelMember<T> : IProjectMember<T>, IModelMember_Orderable
        where T : IModelMember<T>
    { }

    public interface IModelMember_Deleteable<T> : IProjectMember_Deleteable<T>, IModelMember<T>
        where T : IModelMember_Deleteable<T>
    { }
}