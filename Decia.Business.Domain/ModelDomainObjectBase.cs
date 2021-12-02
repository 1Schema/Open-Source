using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainObjects;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain
{
    public abstract class ModelDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT> : ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>, IModelMember
        where KEY : IModelMember
        where KEYED_DOMAIN_OBJECT : class, IKeyedDomainObject<KEY, KEYED_DOMAIN_OBJECT>, IModelMember, IPermissionable
    {
        #region Constructors

        public ModelDomainObjectBase(IModelMember modelMember)
            : this(modelMember.ProjectGuid, modelMember.RevisionNumber_NonNull, modelMember.ModelTemplateNumber, modelMember.ModelInstanceGuid)
        { }

        public ModelDomainObjectBase(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
            : base()
        {
            InitializeMembers(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid);
        }

        public ModelDomainObjectBase(IUser creator, IModelMember modelMember)
            : this(creator.UserGuid, modelMember.ProjectGuid, modelMember.RevisionNumber_NonNull, modelMember.ModelTemplateNumber, modelMember.ModelInstanceGuid)
        { }

        public ModelDomainObjectBase(Guid creatorGuid, Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
            : base(creatorGuid)
        {
            InitializeMembers(projectGuid, revisionNumber, modelTemplateNumber, modelInstanceGuid);
        }

        protected virtual void InitializeMembers(Guid projectGuid, long revisionNumber, int modelTemplateNumber, Guid? modelInstanceGuid)
        {
            SetProjectGuid(projectGuid);
            SetRevisionNumber(revisionNumber);
            SetModelTemplateNumber(modelTemplateNumber);
            if (modelInstanceGuid.HasValue)
            { SetModelInstanceGuid(modelInstanceGuid); }
        }

        #endregion

        #region Abstract Methods

        protected abstract Guid GetProjectGuid();
        protected abstract long GetRevisionNumber();
        protected abstract int GetModelTemplateNumber();
        protected abstract Guid? GetModelInstanceGuid();

        protected abstract void SetProjectGuid(Guid projectGuid);
        protected abstract void SetRevisionNumber(long revisionNumber);
        protected abstract void SetModelTemplateNumber(int modelTemplateNumber);
        protected abstract void SetModelInstanceGuid(Guid? modelInstanceNumber);

        #endregion

        #region IModelMember Implementation

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
            get { return GetRevisionNumber(); }
        }

        public bool IsInstance
        {
            get { return GetModelInstanceGuid().HasValue; }
        }

        public int ModelTemplateNumber
        {
            get { return GetModelTemplateNumber(); }
        }

        public Nullable<Guid> ModelInstanceGuid
        {
            get { return GetModelInstanceGuid(); }
        }

        #endregion

        #region IModelObjectWithRef Implementation

        public ModelObjectType ModelObjectType
        {
            get { return Key.ModelObjectType; }
        }

        public Guid ModelObjectId
        {
            get { return Key.ModelObjectId; }
        }

        public bool IdIsInt
        {
            get { return Key.IdIsInt; }
        }

        public int ModelObjectIdAsInt
        {
            get { return Key.ModelObjectIdAsInt; }
        }

        public string ComplexId
        {
            get { return Key.ComplexId; }
        }

        public ModelObjectReference ModelObjectRef
        {
            get { return Key.ModelObjectRef; }
        }

        #endregion

        #region Copy and Equals Methods

        protected override void CopyBaseValuesTo(KEYED_DOMAIN_OBJECT otherObject)
        {
            base.CopyBaseValuesTo(otherObject);

            var otherObject_Typed = (otherObject as ModelDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.ProjectGuid != this.ProjectGuid)
            { otherObject_Typed.SetProjectGuid(this.ProjectGuid); }
            if (otherObject_Typed.RevisionNumber != this.RevisionNumber)
            { otherObject_Typed.SetRevisionNumber(this.RevisionNumber_NonNull); }
            if (otherObject_Typed.ModelTemplateNumber != this.ModelTemplateNumber)
            { otherObject_Typed.SetModelTemplateNumber(this.ModelTemplateNumber); }
            if (otherObject_Typed.ModelInstanceGuid != this.ModelInstanceGuid)
            { otherObject_Typed.SetModelInstanceGuid(this.ModelInstanceGuid); }
        }

        protected override bool EqualsBaseValues(KEYED_DOMAIN_OBJECT otherObject)
        {
            if (!base.EqualsBaseValues(otherObject))
            { return false; }

            var otherObject_Typed = (otherObject as ModelDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>);

            if (otherObject_Typed.ProjectGuid != this.ProjectGuid)
            { return false; }
            if (otherObject_Typed.RevisionNumber != this.RevisionNumber)
            { return false; }
            if (otherObject_Typed.ModelTemplateNumber != this.ModelTemplateNumber)
            { return false; }
            if (otherObject_Typed.ModelInstanceGuid != this.ModelInstanceGuid)
            { return false; }
            return true;
        }

        #endregion
    }
}