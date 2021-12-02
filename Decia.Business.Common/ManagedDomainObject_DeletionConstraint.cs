using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Constraints;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.Events;
using DomainDriver.DomainModeling.Repositories;

namespace Decia.Business.Common
{
    public class ManagedDomainObject_DeletionConstraint<KEY, KEYED_DOMAIN_OBJECT> : ConstraintBase
         where KEYED_DOMAIN_OBJECT : ManagedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>
    {
        public ManagedDomainObject_DeletionConstraint(IDomainModel domainModel)
            : base(domainModel)
        {
            var objectType = typeof(KEYED_DOMAIN_OBJECT);
            Name = objectType.Name + " - Deletion Constraint";

            Repository.RemovingDomainObject += new RemovingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_RemovingDomainObject);
        }

        public IRepository<KEY, KEYED_DOMAIN_OBJECT> Repository
        {
            get { return DomainModel.GetRepository<KEY, KEYED_DOMAIN_OBJECT>(); }
        }

        protected virtual void Repository_RemovingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, RemoveDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }

            throw new InvalidOperationException("The current Repository does not allow deletions.");
        }
    }
}