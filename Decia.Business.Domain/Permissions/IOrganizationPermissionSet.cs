using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Permissions;

namespace Decia.Business.Domain.Permissions
{
    public interface IOrganizationPermissionSet
    {
        Guid UserGuid { get; }
        UserId UserId { get; }

        Guid OrganizationGuid { get; }
        OrganizationId OrganizationId { get; }

        Dictionary<OrganizationPermissionType, PermissionSetting> ResultingPermissions { get; }

        OrganizationPermissionType AllowedPermissions { get; }
        OrganizationPermissionType DeniedPermissions { get; }

        void AssertPermissions(OrganizationPermissionType requiredPermissions);
        bool HasPermissions(OrganizationPermissionType requiredPermissions);
        bool HasPermissions(OrganizationPermissionType requiredPermissions, out string failureMessage);
    }
}