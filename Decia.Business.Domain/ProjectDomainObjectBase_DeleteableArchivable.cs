﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Domain
{
    public abstract class ProjectDomainObjectBase_DeleteableArchivable<KEY, KEYED_DOMAIN_OBJECT> : ProjectDomainObjectBase_Deleteable<KEY, KEYED_DOMAIN_OBJECT>, IArchivable
        where KEY : IProjectMember
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IProjectMember_Deleteable, IPermissionable, IArchivable
    {
        #region IArchivable Members

        protected bool m_IsArchived;
        protected Nullable<Guid> m_ArchiverGuid;
        protected Nullable<DateTime> m_ArchivalDate;

        #endregion

        #region Constructors

        public ProjectDomainObjectBase_DeleteableArchivable(IProjectMember projectMember)
            : base(projectMember)
        { }

        public ProjectDomainObjectBase_DeleteableArchivable(Guid projectGuid, long revisionNumber)
            : base(projectGuid, revisionNumber)
        { }

        public ProjectDomainObjectBase_DeleteableArchivable(IUser creator, IProjectMember projectMember)
            : base(creator, projectMember)
        { }

        public ProjectDomainObjectBase_DeleteableArchivable(Guid creatorGuid, Guid projectGuid, long revisionNumber)
            : base(creatorGuid, projectGuid, revisionNumber)
        { }

        protected override void InitializeMembers(Guid projectGuid, long revisionNumber)
        {
            base.InitializeMembers(projectGuid, revisionNumber);
            m_IsArchived = false;
            m_ArchiverGuid = null;
            m_ArchivalDate = null;
        }

        #endregion

        #region IArchivable Implementation

        public bool IsArchived
        {
            get { return m_IsArchived; }
        }

        public UserId ArchiverId
        {
            get
            {
                this.AssertIsArchived("The Project has not been archived so no ArchiverId exists.");
                return new UserId(m_ArchiverGuid.Value);
            }
        }

        public DateTime ArchivalDate
        {
            get
            {
                this.AssertIsArchived("The Project has not been archived so no ArchivalDate exists.");
                return m_ArchivalDate.Value;
            }
        }

        public virtual bool IsArchivable()
        {
            return !IsArchived;
        }

        public virtual void SetIsArchived(UserId archiverId, bool isArchived)
        {
            if (isArchived)
            { this.AssertIsNotArchived("The Managed Object is not archivable."); }
            else
            { this.AssertIsArchived("The Managed Object is already archived."); }

            m_IsArchived = isArchived;

            if (m_IsArchived)
            {
                m_ArchiverGuid = archiverId.UserGuid;
                m_ArchivalDate = DateTime.UtcNow;
            }
            else
            {
                m_ArchiverGuid = null;
                m_ArchivalDate = null;
            }
        }

        #endregion

        #region Copy and Equals Methods

        protected override void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            base.CopyBaseValuesTo(otherObject);

            var otherObject_Typed = (otherObject as ProjectDomainObjectBase_DeleteableArchivable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_IsArchived != m_IsArchived)
            { otherObject_Typed.m_IsArchived = m_IsArchived; }
            if (otherObject_Typed.m_ArchiverGuid != m_ArchiverGuid)
            { otherObject_Typed.m_ArchiverGuid = m_ArchiverGuid; }
            if (!ConversionUtils.AreEqualForPrecision(otherObject_Typed.m_ArchivalDate, m_ArchivalDate, this.GetDomainObjectPrecision()))
            { otherObject_Typed.m_ArchivalDate = m_ArchivalDate; }
        }

        protected override bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            if (!base.EqualsBaseValues(otherObject))
            { return false; }

            var otherObject_Typed = (otherObject as ProjectDomainObjectBase_DeleteableArchivable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_IsArchived != m_IsArchived)
            { return false; }
            if (otherObject_Typed.m_ArchiverGuid != m_ArchiverGuid)
            { return false; }
            if (!ConversionUtils.AreEqualForPrecision(otherObject_Typed.m_ArchivalDate, m_ArchivalDate, this.GetDomainObjectPrecision()))
            { return false; }

            return true;
        }

        #endregion
    }
}