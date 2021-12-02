using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.StorageManagers.EntityFrameworkStorage;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public abstract class ModelDomainObjectBase_DeleteableOrderable<KEY, KEYED_DOMAIN_OBJECT> : ModelDomainObjectBase_Deleteable<KEY, KEYED_DOMAIN_OBJECT>, IOrderable
        where KEY : IModelMember
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IModelMember_Orderable, IProjectMember_Deleteable, IPermissionable
    {
        #region IOrderable Members

        protected Nullable<long> m_OrderNumber;

        #endregion

        #region Constructors

        public ModelDomainObjectBase_DeleteableOrderable(IModelMember modelMember)
            : base(modelMember)
        { }

        public ModelDomainObjectBase_DeleteableOrderable(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid)
        { }

        public ModelDomainObjectBase_DeleteableOrderable(IUser creator, IModelMember modelMember)
            : base(creator, modelMember)
        { }

        public ModelDomainObjectBase_DeleteableOrderable(Guid creatorGuid, Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
            : base(creatorGuid, projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid)
        { }

        protected override void InitializeMembers(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
        {
            base.InitializeMembers(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid);
            m_OrderNumber = null;
        }

        #endregion

        #region Abstract Methods

        protected abstract string GetOrderValue();

        #endregion

        #region IOrderable Implementation

        [NotMapped]
        public Nullable<long> OrderNumber
        {
            get { return m_OrderNumber; }
            set { m_OrderNumber = value; }
        }

        public string OrderValue
        {
            get { return GetOrderValue(); }
        }

        #endregion

        #region Copy and Equals Methods

        protected override void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            base.CopyBaseValuesTo(otherObject);

            var otherObject_Typed = (otherObject as ModelDomainObjectBase_DeleteableOrderable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_OrderNumber != m_OrderNumber)
            { otherObject_Typed.m_OrderNumber = m_OrderNumber; }
        }

        protected override bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            if (!base.EqualsBaseValues(otherObject))
            { return false; }

            var otherObject_Typed = (otherObject as ModelDomainObjectBase_DeleteableOrderable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_OrderNumber != m_OrderNumber)
            { return false; }
            return true;
        }

        #endregion
    }
}