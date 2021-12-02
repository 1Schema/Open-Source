using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using Decia.Business.Common;

namespace Decia.Business.Domain
{
    public class ProjectDomainObject_DeletionConstraint<KEY, KEYED_DOMAIN_OBJECT> : ManagedDomainObject_DeletionConstraint<KEY, KEYED_DOMAIN_OBJECT>
        where KEY : IProjectMember
        where KEYED_DOMAIN_OBJECT : ProjectDomainObjectBase<KEY, KEYED_DOMAIN_OBJECT>
    {
        public ProjectDomainObject_DeletionConstraint(IDomainModel domainModel)
            : base(domainModel)
        { }
    }
}