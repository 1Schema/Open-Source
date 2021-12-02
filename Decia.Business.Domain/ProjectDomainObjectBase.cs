using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public abstract class ProjectDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT> : ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>, IProjectMember
        where KEY : IProjectMember
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IProjectMember, IPermissionable
    {
        #region Constructors

        public ProjectDomainObjectBase(IProjectMember projectMember)
            : this(projectMember.ProjectGuid, projectMember.RevisionNumber_NonNull)
        { }

        public ProjectDomainObjectBase(Guid projectGuid, long revisionNumber)
            : base()
        {
            InitializeMembers(projectGuid, revisionNumber);
        }

        public ProjectDomainObjectBase(IUser creator, IProjectMember projectMember)
            : this(creator.UserGuid, projectMember.ProjectGuid, projectMember.RevisionNumber_NonNull)
        { }

        public ProjectDomainObjectBase(Guid creatorGuid, Guid projectGuid, long revisionNumber)
            : base(creatorGuid)
        {
            InitializeMembers(projectGuid, revisionNumber);
        }

        protected virtual void InitializeMembers(Guid projectGuid, long revisionNumber)
        {
            SetProjectGuid(projectGuid);
            SetRevisionNumber(revisionNumber);
        }

        #endregion

        #region Abstract Methods

        protected abstract Guid GetProjectGuid();
        protected abstract long? GetRevisionNumber();

        protected abstract void SetProjectGuid(Guid projectGuid);
        protected abstract void SetRevisionNumber(long revisionNumber);

        #endregion

        #region IProjectMember Implementation

        public Guid ProjectGuid
        {
            get { return GetProjectGuid(); }
        }

        public bool IsRevisionSpecific
        {
            get { return true; }
        }

        public long? RevisionNumber
        {
            get { return GetRevisionNumber(); }
        }

        public long RevisionNumber_NonNull
        {
            get { return GetRevisionNumber().Value; }
        }

        #endregion

        #region Copy and Equals Methods

        protected override void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            base.CopyBaseValuesTo(otherObject);

            var otherObject_Typed = (otherObject as ProjectDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.ProjectGuid != this.ProjectGuid)
            { otherObject_Typed.SetProjectGuid(this.ProjectGuid); }
            if (otherObject_Typed.RevisionNumber != this.RevisionNumber)
            { otherObject_Typed.SetRevisionNumber(this.RevisionNumber_NonNull); }
        }

        protected override bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            if (!base.EqualsBaseValues(otherObject))
            { return false; }

            var otherObject_Typed = (otherObject as ProjectDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.ProjectGuid != this.ProjectGuid)
            { return false; }
            if (otherObject_Typed.RevisionNumber != this.RevisionNumber)
            { return false; }
            return true;
        }

        #endregion
    }
}