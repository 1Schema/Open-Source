using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public interface IProjectMember
    {
        Guid ProjectGuid { get; }

        bool IsRevisionSpecific { get; }
        Nullable<long> RevisionNumber { get; }
        long RevisionNumber_NonNull { get; }
    }

    public interface IProjectMember_Revisionless : IProjectMember
    {
        bool Equals_Revisionless(object obj);
        int GetHashCode_Revisionless();
        string ToString_Revisionless();
    }

    public interface IProjectMember_Deleteable : IProjectMember, IDeletable
    { }

    public interface IProjectMember<T> : IProjectMember
        where T : IProjectMember<T>
    {
        T CopyForRevision(long newRevisionNumber);
        T CopyForProject(Guid projectGuid, long revisionNumber);
    }

    public interface IProjectMember_Deleteable<T> : IProjectMember<T>, IProjectMember_Deleteable
        where T : IProjectMember_Deleteable<T>
    { }

    public interface IProjectMember_Cloneable<T> : IProjectMember<T>
       where T : IProjectMember_Cloneable<T>
    {
        T CopyAsNew(long newRevisionNumber);
    }
}