using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    [Flags]
    public enum WorkgroupPermissionType
    {
        [Description("Manage Roles")]
        ManageRoles = 1,
        [Description("Manage Members")]
        ManageMembers = 2,
        [Description("View Members")]
        ViewMembers = 4,
        [Description("Read Workgroup Info")]
        ReadWorkgroupInfo = 8,
        [Description("Edit Workgroup Info")]
        EditWorkgroupInfo = 16,
        [Description("Erase Workgroup")]
        EraseWorkgroup = 32,
        [Description("Send Communications")]
        SendCommunications = 64,
    }

    public static class WorkgroupPermissionTypeUtils
    {
        public static readonly WorkgroupPermissionType FullControl = EnumUtils.GetEnumValues<WorkgroupPermissionType>().Bitwise_Combine();
    }
}