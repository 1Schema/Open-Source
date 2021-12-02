using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Common
{
    public abstract class ManagedDomainObjectBase_Archivable<KEY, KEYED_DOMAIN_OBJECT> : ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>, IArchivable
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IPermissionable, IArchivable
    {
        #region IArchivable Members

        protected bool m_IsArchived;
        protected Nullable<Guid> m_ArchiverGuid;
        protected Nullable<DateTime> m_ArchivalDate;

        #endregion

        #region Constructors

        public ManagedDomainObjectBase_Archivable()
            : this(UserState.CurrentThreadState.CurrentUserId)
        { }

        public ManagedDomainObjectBase_Archivable(IUser creator)
            : this(creator.UserGuid)
        { }

        public ManagedDomainObjectBase_Archivable(Guid creatorGuid)
            : base(creatorGuid)
        {
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

            var otherObject_Typed = (otherObject as ManagedDomainObjectBase_Archivable<KEY, KEYED_DOMAIN_OBJECT>);

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

            var otherObject_Typed = (otherObject as ManagedDomainObjectBase_Archivable<KEY, KEYED_DOMAIN_OBJECT>);

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