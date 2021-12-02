using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;

namespace Decia.Business.Domain.Permissions
{
    public interface IUserPermissionsSet
    {
        Guid UserGuid { get; }
        UserId UserId { get; }

        ICollection<OrganizationId> RelevantOrganizationIds { get; }
        ICollection<WorkgroupId> RelevantWorkgroupIds { get; }
        ICollection<ProjectId> RelevantProjectIds { get; }

        IOrganizationPermissionSet GetOrganizationPermissionsSet(Guid organizationGuid);
        IOrganizationPermissionSet GetOrganizationPermissionsSet(OrganizationId organizationId);

        IWorkgroupPermissionSet GetWorkgroupPermissionsSet(Guid workgroupGuid);
        IWorkgroupPermissionSet GetWorkgroupPermissionsSet(WorkgroupId workgroupId);

        IProjectPermissionSet GetProjectPermissionsSet(Guid projectGuid);
        IProjectPermissionSet GetProjectPermissionsSet(IProjectMember projectMember);
    }
}