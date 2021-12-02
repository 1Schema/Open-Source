using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum PredefinedOrganizationRoleType
    {
        Admin,
        Member,
        Guest,
    }

    public static class PredefinedOrganizationRoleTypeUtils
    {
        public const string Admin_RoleGuid = "22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        public const string Admin_Name = "Organization Administrators";
        public const string Admin_Description = "This is a pre-defined, system-level Role for Organization Administrators.";
        public const string Member_RoleGuid = "22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        public const string Member_Name = "Organization Members";
        public const string Member_Description = "This is a pre-defined, system-level Role for Organization Members.";
        public const string Guest_RoleGuid = "22222222-cccc-cccc-cccc-cccccccccccc";
        public const string Guest_Name = "Organization Guests";
        public const string Guest_Description = "This is a pre-defined, system-level Role for Organization Guests.";

        public static Guid GetRoleGuid_ForRoleType(this PredefinedOrganizationRoleType roleType)
        {
            if (roleType == PredefinedOrganizationRoleType.Admin)
            { return new Guid(Admin_RoleGuid); }
            else if (roleType == PredefinedOrganizationRoleType.Member)
            { return new Guid(Member_RoleGuid); }
            else if (roleType == PredefinedOrganizationRoleType.Guest)
            { return new Guid(Guest_RoleGuid); }
            else
            { throw new InvalidOperationException("Invalid PredefinedOrganizationRoleType encountered."); }
        }

        public static string GetName_ForRoleType(this PredefinedOrganizationRoleType roleType)
        {
            if (roleType == PredefinedOrganizationRoleType.Admin)
            { return Admin_Name; }
            else if (roleType == PredefinedOrganizationRoleType.Member)
            { return Member_Name; }
            else if (roleType == PredefinedOrganizationRoleType.Guest)
            { return Guest_Name; }
            else
            { throw new InvalidOperationException("Invalid PredefinedOrganizationRoleType encountered."); }
        }

        public static string GetDescription_ForRoleType(this PredefinedOrganizationRoleType roleType)
        {
            if (roleType == PredefinedOrganizationRoleType.Admin)
            { return Admin_Description; }
            else if (roleType == PredefinedOrganizationRoleType.Member)
            { return Member_Description; }
            else if (roleType == PredefinedOrganizationRoleType.Guest)
            { return Guest_Description; }
            else
            { throw new InvalidOperationException("Invalid PredefinedOrganizationRoleType encountered."); }
        }

        public static IDictionary<OrganizationPermissionType, PermissionSetting> GetOrganizationPermissions_ForRoleType(this PredefinedOrganizationRoleType roleType)
        {
            var permissions = new Dictionary<OrganizationPermissionType, PermissionSetting>();

            if (roleType == PredefinedOrganizationRoleType.Admin)
            {
                foreach (OrganizationPermissionType permType in Enum.GetValues(typeof(OrganizationPermissionType)))
                {
                    permissions.Add(permType, PermissionSetting.Allow);
                }
                return permissions;
            }
            else if (roleType == PredefinedOrganizationRoleType.Member)
            {
                foreach (OrganizationPermissionType permType in Enum.GetValues(typeof(OrganizationPermissionType)))
                {
                    if ((permType == OrganizationPermissionType.ViewMembers) || (permType == OrganizationPermissionType.ReadOrganizationInfo) || (permType == OrganizationPermissionType.SendCommunications))
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else if (roleType == PredefinedOrganizationRoleType.Guest)
            {
                foreach (OrganizationPermissionType permType in Enum.GetValues(typeof(OrganizationPermissionType)))
                {
                    if (permType == OrganizationPermissionType.ReadOrganizationInfo)
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else
            { throw new InvalidOperationException("Invalid PredefinedOrganizationRoleType encountered."); }
        }

        public static IDictionary<WorkgroupPermissionType, PermissionSetting> GetWorkgroupPermissions_ForRoleType(this PredefinedOrganizationRoleType roleType)
        {
            if (roleType == PredefinedOrganizationRoleType.Admin)
            { return PredefinedWorkgroupRoleType.Admin.GetWorkgroupPermissions_ForRoleType(); }
            else if (roleType == PredefinedOrganizationRoleType.Member)
            { return PredefinedWorkgroupRoleType.Member.GetWorkgroupPermissions_ForRoleType(); }
            else if (roleType == PredefinedOrganizationRoleType.Guest)
            { return PredefinedWorkgroupRoleType.Guest.GetWorkgroupPermissions_ForRoleType(); }
            else
            { throw new InvalidOperationException("Invalid PredefinedOrganizationRoleType encountered."); }
        }

        public static IDictionary<ProjectPermissionType, PermissionSetting> GetProjectPermissions_ForRoleType(this PredefinedOrganizationRoleType roleType)
        {
            if (roleType == PredefinedOrganizationRoleType.Admin)
            { return ProjectRoleType.Admin.GetProjectPermissions_ForRoleType(); }
            else if (roleType == PredefinedOrganizationRoleType.Member)
            { return ProjectRoleType.Member.GetProjectPermissions_ForRoleType(); }
            else if (roleType == PredefinedOrganizationRoleType.Guest)
            { return ProjectRoleType.Guest.GetProjectPermissions_ForRoleType(); }
            else
            { throw new InvalidOperationException("Invalid PredefinedOrganizationRoleType encountered."); }
        }
    }
}