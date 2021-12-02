using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Domain.Permissions
{
    public interface IProjectPermissionSet
    {
        Guid UserGuid { get; }
        UserId UserId { get; }

        Guid ProjectGuid { get; }
        ProjectId ProjectId { get; }

        Nullable<Guid> OrganizationGuid { get; }
        Nullable<OrganizationId> OrganizationId { get; }

        Nullable<Guid> WorkgroupGuid { get; }
        Nullable<WorkgroupId> WorkgroupId { get; }

        Dictionary<ProjectPermissionType, PermissionSetting> ResultingPermissions { get; }

        ProjectPermissionType AllowedPermissions { get; }
        ProjectPermissionType DeniedPermissions { get; }

        void AssertPermissions(ProjectPermissionType requiredPermissions);
        bool HasPermissions(ProjectPermissionType requiredPermissions);
        bool HasPermissions(ProjectPermissionType requiredPermissions, out string failureMessage);
    }
}