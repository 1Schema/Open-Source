using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Common
{
    public interface ISiteActor
    {
        SiteActorType ActorType { get; }
        Guid ActorGuid { get; }
    }
}