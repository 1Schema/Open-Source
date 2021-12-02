using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;

namespace Decia.Business.Domain
{
    public interface IStructuralMember : IModelMember
    {
        StructuralTypeOption StructuralTypeOption { get; }
        int StructuralTypeNumber { get; }
        Nullable<StructuralInstanceOption> StructuralInstanceOption { get; }
        Nullable<Guid> StructuralInstanceGuid { get; }

        bool MatchesStructuralType(IStructuralMember structuralType, bool checkRevision);
        bool MatchesModelInstance(IModelMember modelInstance, bool checkRevision);
    }

    public interface IStructuralMember_Orderable : IStructuralMember, IModelMember_Orderable
    { }

    public interface IStructuralMember<T> : IModelMember<T>, IStructuralMember
        where T : IStructuralMember<T>
    {
        StructuralSpace StructuralSpace { get; }
        Nullable<StructuralPoint> StructuralPoint { get; }
        StructuralPoint StructuralPoint_NonNull { get; }

        StructuralSpace GetStructuralSpaceWithVariableTemplateRefs(IDictionary<StructuralDimension, ModelObjectReference> variableTemplateRefsByDimension);
    }

    public interface IStructuralMember_Deleteable<T> : IModelMember_Deleteable<T>, IStructuralMember<T>
        where T : IStructuralMember_Deleteable<T>
    { }
}