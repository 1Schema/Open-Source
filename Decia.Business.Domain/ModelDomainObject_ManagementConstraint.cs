using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Constraints;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Events;
using DomainDriver.DomainModeling.Repositories;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public class ModelDomainObject_ManagementConstraint : ConstraintBase
    {
        public static Func<IDomainModel, ModelMemberId, RepositoryActionType, bool> CheckRevisionIsOpen { get; set; }
        public static Func<IDomainModel, ModelMemberId, bool> SetRevisionToUpdated { get; set; }

        public ModelDomainObject_ManagementConstraint(IDomainModel domainModel)
            : base(domainModel)
        { }
    }

    public class ModelDomainObject_ManagementConstraint<KEY, KEYED_DOMAIN_OBJECT> : ModelDomainObject_ManagementConstraint
        where KEY : IModelMember
        where KEYED_DOMAIN_OBJECT : ModelDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>
    {
        public ModelDomainObject_ManagementConstraint(IDomainModel domainModel)
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

        protected virtual void Repository_AddingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, AddDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            var actionType = RepositoryActionType.Add;

            if (args.ObjectToAdd is IBatchPersistable)
            {
                if ((args.ObjectToAdd as IBatchPersistable).BatchState == BatchState.Updated)
                { actionType = RepositoryActionType.Update; }
            }

            CheckRevisionIsOpen(DomainModel, args.ObjectToAdd.Key.GetModelMemberId(), actionType);
        }

        protected virtual void Repository_AddedDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, AddDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            SetRevisionToUpdated(DomainModel, args.ObjectToAdd.Key.GetModelMemberId());
        }

        protected virtual void Repository_UpdatingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, UpdateDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            CheckRevisionIsOpen(DomainModel, args.UpdatedObject.Key.GetModelMemberId(), RepositoryActionType.Update);
        }

        protected virtual void Repository_UpdatedDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, UpdateDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            SetRevisionToUpdated(DomainModel, args.UpdatedObject.Key.GetModelMemberId());
        }

        protected virtual void Repository_RemovingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, RemoveDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            throw new InvalidOperationException("The current Repository does not allow deletions.");
        }
    }
}