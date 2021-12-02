using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public abstract class ModelDomainObjectBase_DeleteableOrderableTaggable<KEY, KEYED_DOMAIN_OBJECT> : ModelDomainObjectBase_DeleteableOrderable<KEY, KEYED_DOMAIN_OBJECT>, ITaggable
        where KEY : IModelMember
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IModelMember_Orderable, IProjectMember_Deleteable, IPermissionable, ITaggable
    {
        #region ITaggable Members

        protected string m_Tags;

        #endregion

        #region Constructors

        public ModelDomainObjectBase_DeleteableOrderableTaggable(IModelMember modelMember)
            : base(modelMember)
        { }

        public ModelDomainObjectBase_DeleteableOrderableTaggable(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
            : base(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid)
        { }

        public ModelDomainObjectBase_DeleteableOrderableTaggable(IUser creator, IModelMember modelMember)
            : base(creator, modelMember)
        { }

        public ModelDomainObjectBase_DeleteableOrderableTaggable(Guid creatorGuid, Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
            : base(creatorGuid, projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid)
        { }

        protected override void InitializeMembers(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
        {
            base.InitializeMembers(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid);
            m_Tags = string.Empty;
        }

        #endregion

        #region ITaggable Implementation

        [NotMapped]
        public virtual string Tags
        {
            get { return m_Tags; }
            set { m_Tags = value; }
        }

        #endregion

        #region Copy and Equals Methods

        protected override void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            base.CopyBaseValuesTo(otherObject);

            var otherObject_Typed = (otherObject as ModelDomainObjectBase_DeleteableOrderableTaggable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_Tags != m_Tags)
            { otherObject_Typed.m_Tags = m_Tags; }
        }

        protected override bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            if (!base.EqualsBaseValues(otherObject))
            { return false; }

            var otherObject_Typed = (otherObject as ModelDomainObjectBase_DeleteableOrderableTaggable<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.m_Tags != m_Tags)
            { return false; }
            return true;
        }

        #endregion
    }
}