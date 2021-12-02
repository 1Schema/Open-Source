using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface IOrganization : ISiteActor
    {
        Guid OrganizationGuid { get; }
    }
}