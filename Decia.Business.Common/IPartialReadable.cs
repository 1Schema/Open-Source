using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.Constraints;
using DomainDriver.DomainModeling.DomainModels;
using DomainDriver.DomainModeling.DomainObjects;
using DomainDriver.DomainModeling.Events;
using DomainDriver.DomainModeling.Repositories;

namespace Decia.Business.Common
{
    public interface IPartialReadable
    {
        bool IsPartialObject { get; }
    }

    public class PartialReadableObject_PartialObjectConstraint<KEY, KEYED_DOMAIN_OBJECT> : ConstraintBase
         where KEYED_DOMAIN_OBJECT : KeyedDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>, IPartialReadable
    {
        public PartialReadableObject_PartialObjectConstraint(IDomainModel domainModel)
            : base(domainModel)
        {
            var objectType = typeof(KEYED_DOMAIN_OBJECT);
            Name = objectType.Name + " - Partial Object Constraint";

            Repository.AddingDomainObject += new AddingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_AddingDomainObject);
            Repository.UpdatingDomainObject += new UpdatingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_UpdatingDomainObject);
            Repository.RemovingDomainObject += new RemovingDomainObjectDelegate<KEY, KEYED_DOMAIN_OBJECT>(Repository_RemovingDomainObject);
        }

        public IRepository<KEY, KEYED_DOMAIN_OBJECT> Repository
        {
            get { return DomainModel.GetRepository<KEY, KEYED_DOMAIN_OBJECT>(); }
        }

        protected void Repository_AddingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, AddDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }
            if (!args.ObjectToAdd.IsPartialObject)
            { return; }
            throw new InvalidOperationException("Use the Full Object to enable creations.");
        }

        protected void Repository_UpdatingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, UpdateDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }
            if (!args.OriginalObject.IsPartialObject && !args.UpdatedObject.IsPartialObject)
            { return; }
            throw new InvalidOperationException("Use the Full Object to enable updates.");
        }

        protected virtual void Repository_RemovingDomainObject(IRepository<KEY, KEYED_DOMAIN_OBJECT> sender, RemoveDomainObjectEventArgs<KEY, KEYED_DOMAIN_OBJECT> args)
        {
            if (!this.Enabled)
            { return; }
            if (!args.ObjectToRemove.IsPartialObject)
            { return; }
            throw new InvalidOperationException("Use the Full Object to enable deletions.");
        }
    }
}