using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Common
{
    public abstract class ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT> : KeyedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>, IPermissionable, IBatchPersistable
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IPermissionable
    {
        #region Static Members

        public static readonly Guid EmptyConstructor_UserGuid = IUserUtils.EmptyUserGuid;

        #endregion

        #region Members

        protected Guid m_CreatorGuid;
        protected DateTime m_CreationDate;
        protected SiteActorType m_OwnerType;
        protected Guid m_OwnerGuid;

        private BatchState m_BatchState;

        #endregion

        #region Constructors

        public ManagedDomainObjectBase()
            : this(UserState.CurrentThreadState.CurrentUserId)
        { }

        public ManagedDomainObjectBase(IUser creator)
            : this(creator.UserGuid)
        { }

        public ManagedDomainObjectBase(Guid creatorGuid)
        {
            m_CreatorGuid = creatorGuid;
            m_CreationDate = DateTime.UtcNow;
            m_OwnerType = SiteActorType.User;
            m_OwnerGuid = creatorGuid;

            m_BatchState = IBatchPersistableUtils.DefaultBatchState;
        }

        #endregion

        #region IPermissionable Implementation

        [NotMapped]
        public UserId CreatorId
        {
            get { return new UserId(m_CreatorGuid); }
        }

        [NotMapped]
        public DateTime CreationDate
        {
            get { return m_CreationDate; }
        }

        [NotMapped]
        public SiteActorId OwnerId
        {
            get { return new SiteActorId(m_OwnerType, m_OwnerGuid); }
        }

        [NotMapped]
        public SiteActorType OwnerType
        {
            get { return m_OwnerType; }
        }

        [NotMapped]
        public Guid OwnerGuid
        {
            get { return m_OwnerGuid; }
        }

        public virtual bool IsOwnerEditable()
        {
            return true;
        }

        public virtual bool IsOwnerValid(SiteActorId newOwnerId, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public void SetOwner(Nullable<SiteActorId> newOwnerId)
        {
            if (!IsOwnerEditable())
            { throw new InvalidOperationException("The Managed Object's Owner is not editable."); }

            var nonNullNewOwnerId = new SiteActorId(CreatorId);
            if (newOwnerId.HasValue)
            { nonNullNewOwnerId = newOwnerId.Value; }

            string errorMessage;
            if (!IsOwnerValid(nonNullNewOwnerId, out errorMessage))
            {
                errorMessage = (!string.IsNullOrWhiteSpace(errorMessage)) ? errorMessage : "The new Owner is not valid for the Managed Object's type.";
                throw new InvalidOperationException(errorMessage);
            }

            m_OwnerType = nonNullNewOwnerId.ActorType;
            m_OwnerGuid = nonNullNewOwnerId.ActorGuid;
        }

        #endregion

        #region IBatchPersistable Implementation

        BatchState IBatchPersistable.BatchState
        {
            get { return m_BatchState; }
            set
            {
                if (m_BatchState == BatchState.Added)
                { return; }
                if (m_BatchState == BatchState.Removed)
                { return; }

                m_BatchState = value;
            }
        }

        #endregion

        #region Copy and Equals Methods

        protected virtual void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            var otherObject_Typed = (otherObject as ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_CreatorGuid != m_CreatorGuid)
            { otherObject_Typed.m_CreatorGuid = m_CreatorGuid; }
            if (!ConversionUtils.AreEqualForPrecision(otherObject_Typed.m_CreationDate, m_CreationDate, this.GetDomainObjectPrecision()))
            { otherObject_Typed.m_CreationDate = m_CreationDate; }
            if (otherObject_Typed.m_OwnerType != m_OwnerType)
            { otherObject_Typed.m_OwnerType = m_OwnerType; }
            if (otherObject_Typed.m_OwnerGuid != m_OwnerGuid)
            { otherObject_Typed.m_OwnerGuid = m_OwnerGuid; }
        }

        protected virtual bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            var otherObject_Typed = (otherObject as ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_CreatorGuid != m_CreatorGuid)
            { return false; }
            if (!ConversionUtils.AreEqualForPrecision(otherObject_Typed.m_CreationDate, m_CreationDate, this.GetDomainObjectPrecision()))
            { return false; }
            if (otherObject_Typed.m_OwnerType != m_OwnerType)
            { return false; }
            if (otherObject_Typed.m_OwnerGuid != m_OwnerGuid)
            { return false; }

            return true;
        }

        #endregion
    }
}