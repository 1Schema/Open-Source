using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Constraints;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Events;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain
{
    public class ProjectDomainObject_ManagementConstraint : ConstraintBase
    {
        public static Func<IDomainModel, ProjectMemberId, ModelObjectType?, RepositoryActionType, bool> CheckRevisionIsOpen { get; set; }
        public static Func<IDomainModel, ProjectMemberId, bool> SetRevisionToUpdated { get; set; }

        public ProjectDomainObject_ManagementConstraint(IDomainModel domainModel)
            : base(domainModel)
        { }
    }

    public class ProjectDomainObject_ManagementConstraint<KEY, KEYED_DOMAIN_OBJECT> : ProjectDomainObject_ManagementConstraint
        where KEY : IProjectMember
        where KEYED_DOMAIN_OBJECT : ProjectDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>
    {
        public ProjectDomainObject_ManagementConstraint(IDomainModel domainModel)
            : base(domainModel)
        {
            var objectType = typeof(KEYED_DOMAIN_OBJECT);
            Name = objectType.Name + " - Deletion Constraint";

            Repository.AddingDomainObject += new AddingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_AddingDomainObject);
            Repository.AddedDomainObject += new AddedDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_AddedDomainObject);
            Repository.UpdatingDomainObject += new UpdatingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_UpdatingDomainObject);
            Repository.UpdatedDomainObject += new UpdatedDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_UpdatedDomainObject);
            Repository.RemovingDomainObject += new RemovingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_RemovingDomainObject);
        }

        public IRepository<KEY, KEYED_DOMAIN_OBJECT> Repository
        {
            get { return DomainModel.GetRepository<KEY, KEYED_DOMAIN_OBJECT>(); }
        }

        public bool IsDomainObjectType_ModelObject { get { return (typeof(KEYED_DOMAIN_OBJECT) is IModelObject); } }
        public ModelObjectType? GetModelObjectType(object domainObject) { return (IsDomainObjectType_ModelObject) ? (domainObject as IModelObject).ModelObjectType : (ModelObjectType?)null; }

        protected virtual void Repository_AddingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, AddDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            var modelObjectType = GetModelObjectType(args.ObjectToAdd);
            var actionType = RepositoryActionType.Add;

            if (args.ObjectToAdd is IBatchPersistable)
            {
                if ((args.ObjectToAdd as IBatchPersistable).BatchState == BatchState.Updated)
                { actionType = RepositoryActionType.Update; }
            }

            CheckRevisionIsOpen(DomainModel, args.ObjectToAdd.Key.GetProjectMemberId(), modelObjectType, actionType);
        }

        protected virtual void Repository_AddedDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, AddDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            SetRevisionToUpdated(DomainModel, args.ObjectToAdd.Key.GetProjectMemberId());
        }

        protected virtual void Repository_UpdatingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, UpdateDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            var modelObjectType = GetModelObjectType(args.UpdatedObject);
            CheckRevisionIsOpen(DomainModel, args.UpdatedObject.Key.GetProjectMemberId(), modelObjectType, RepositoryActionType.Update);
        }

        protected virtual void Repository_UpdatedDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, UpdateDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            SetRevisionToUpdated(DomainModel, args.UpdatedObject.Key.GetProjectMemberId());
        }

        protected virtual void Repository_RemovingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, RemoveDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            throw new InvalidOperationException("The current Repository does not allow deletions.");
        }
    }
}