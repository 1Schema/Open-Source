using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Domain.Permissions
{
    public interface IWorkgroupPermissionSet
    {
        Guid UserGuid { get; }
        UserId UserId { get; }

        Guid WorkgroupGuid { get; }
        WorkgroupId WorkgroupId { get; }

        Nullable<Guid> OrganizationGuid { get; }
        Nullable<OrganizationId> OrganizationId { get; }

        Dictionary<WorkgroupPermissionType, PermissionSetting> ResultingPermissions { get; }

        WorkgroupPermissionType AllowedPermissions { get; }
        WorkgroupPermissionType DeniedPermissions { get; }

        void AssertPermissions(WorkgroupPermissionType requiredPermissions);
        bool HasPermissions(WorkgroupPermissionType requiredPermissions);
        bool HasPermissions(WorkgroupPermissionType requiredPermissions, out string failureMessage);
    }
}