using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    [Flags]
    public enum OrganizationPermissionType
    {
        [Description("Manage Roles")]
        ManageRoles = 1,
        [Description("Manage Members")]
        ManageMembers = 2,
        [Description("View Members")]
        ViewMembers = 4,
        [Description("Read Organization Info")]
        ReadOrganizationInfo = 8,
        [Description("Edit Organization Info")]
        EditOrganizationInfo = 16,
        [Description("Erase Organization")]
        EraseOrganization = 32,
        [Description("Send Communications")]
        SendCommunications = 64,
    }

    public static class OrganizationPermissionTypeUtils
    {
        public static readonly OrganizationPermissionType FullControl = EnumUtils.GetEnumValues<OrganizationPermissionType>().Bitwise_Combine();
    }
}