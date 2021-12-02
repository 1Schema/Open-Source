using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum ProjectRoleType
    {
        Creator,
        Owner,
        Admin,
        Member,
        Guest,
    }

    public static class ProjectRoleTypeUtils
    {
        public static IDictionary<ProjectPermissionType, PermissionSetting> GetProjectPermissions_ForRoleType(this ProjectRoleType roleType)
        {
            var permissions = new Dictionary<ProjectPermissionType, PermissionSetting>();

            if ((roleType == ProjectRoleType.Creator) || (roleType == ProjectRoleType.Owner))
            {
                foreach (ProjectPermissionType permType in Enum.GetValues(typeof(ProjectPermissionType)))
                {
                    permissions.Add(permType, PermissionSetting.Allow);
                }
                return permissions;
            }
            else if (roleType == ProjectRoleType.Admin)
            {
                foreach (ProjectPermissionType permType in Enum.GetValues(typeof(ProjectPermissionType)))
                {
                    if ((permType == ProjectPermissionType.ManageMembers) || (permType == ProjectPermissionType.ReadProjectInfo) || (permType == ProjectPermissionType.EditProjectInfo) || (permType == ProjectPermissionType.CreateTemplateData) || (permType == ProjectPermissionType.EditTemplateData) || (permType == ProjectPermissionType.DeleteTemplateData) || (permType == ProjectPermissionType.CreateInstanceData) || (permType == ProjectPermissionType.EditInstanceData) || (permType == ProjectPermissionType.DeleteInstanceData) || (permType == ProjectPermissionType.ExportData))
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else if (roleType == ProjectRoleType.Member)
            {
                foreach (ProjectPermissionType permType in Enum.GetValues(typeof(ProjectPermissionType)))
                {
                    if ((permType == ProjectPermissionType.ReadProjectInfo) || (permType == ProjectPermissionType.CreateInstanceData) || (permType == ProjectPermissionType.EditInstanceData) || (permType == ProjectPermissionType.DeleteInstanceData) || (permType == ProjectPermissionType.ExportData))
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else if (roleType == ProjectRoleType.Guest)
            {
                foreach (ProjectPermissionType permType in Enum.GetValues(typeof(ProjectPermissionType)))
                {
                    if (permType == ProjectPermissionType.ReadProjectInfo)
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else
            { throw new InvalidOperationException("Invalid ProjectRoleType encountered."); }
        }
    }
}