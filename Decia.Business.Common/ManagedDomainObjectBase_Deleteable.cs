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
    public abstract class ManagedDomainObjectBase_Deleteable<KEY, KEYED_DOMAIN_OBJECT> : ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>, IDeletable
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IPermissionable, IDeletable
    {
        #region IDeletable Members

        protected bool m_IsDeleted;
        protected Nullable<Guid> m_DeleterGuid;
        protected Nullable<DateTime> m_DeletionDate;

        #endregion

        #region Constructors

        public ManagedDomainObjectBase_Deleteable()
            : this(UserState.CurrentThreadState.CurrentUserId)
        { }

        public ManagedDomainObjectBase_Deleteable(IUser creator)
            : this(creator.UserGuid)
        { }

        public ManagedDomainObjectBase_Deleteable(Guid creatorGuid)
            : base(creatorGuid)
        {
            m_IsDeleted = false;
            m_DeleterGuid = null;
            m_DeletionDate = null;
        }

        #endregion

        #region IDeletable Implementation

        public bool IsDeleted
        {
            get { return m_IsDeleted; }
        }

        public UserId DeleterId
        {
            get
            {
                this.AssertIsDeleted("The current object has not been deleted so no DeleterId exists.");
                return new UserId(m_DeleterGuid.Value);
            }
        }

        public DateTime DeletionDate
        {
            get
            {
                this.AssertIsDeleted("The current object has not been deleted so no DeletionDate exists.");
                return m_DeletionDate.Value;
            }
        }

        public virtual bool IsDeletable()
        {
            return !IsDeleted;
        }

        public virtual bool IsDeleterValid(UserId deleterId, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public void SetToDeleted()
        {
            SetToDeleted(UserState.CurrentThreadState.CurrentUserId);
        }

        public virtual void SetToDeleted(UserId deleterId)
        {
            this.AssertIsNotDeleted("The current object is not deletable.");

            string errorMessage;
            if (!IsDeleterValid(deleterId, out errorMessage))
            {
                errorMessage = (!string.IsNullOrWhiteSpace(errorMessage)) ? errorMessage : "The Deleter is not valid for the Managed Object's type.";
                throw new InvalidOperationException(errorMessage);
            }

            m_IsDeleted = true;
            m_DeleterGuid = deleterId.UserGuid;
            m_DeletionDate = DateTime.UtcNow;
        }

        #endregion

        #region Copy and Equals Methods

        protected override void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            base.CopyBaseValuesTo(otherObject);

            var otherObject_Typed = (otherObject as ManagedDomainObjectBase_Deleteable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_IsDeleted != m_IsDeleted)
            { otherObject_Typed.m_IsDeleted = m_IsDeleted; }
            if (otherObject_Typed.m_DeleterGuid != m_DeleterGuid)
            { otherObject_Typed.m_DeleterGuid = m_DeleterGuid; }
            if (!ConversionUtils.AreEqualForPrecision(otherObject_Typed.m_DeletionDate, m_DeletionDate, this.GetDomainObjectPrecision()))
            { otherObject_Typed.m_DeletionDate = m_DeletionDate; }
        }

        protected override bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            if (!base.EqualsBaseValues(otherObject))
            { return false; }

            var otherObject_Typed = (otherObject as ManagedDomainObjectBase_Deleteable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_IsDeleted != m_IsDeleted)
            { return false; }
            if (otherObject_Typed.m_DeleterGuid != m_DeleterGuid)
            { return false; }
            if (!ConversionUtils.AreEqualForPrecision(otherObject_Typed.m_DeletionDate, m_DeletionDate, this.GetDomainObjectPrecision()))
            { return false; }

            return true;
        }

        #endregion
    }
}