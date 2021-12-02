using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum SiteActorType
    {
        [Description("User")]
        User,
        [Description("Workgroup")]
        Workgroup,
        [Description("Organization")]
        Organization
    }
}