using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum PredefinedWorkgroupRoleType
    {
        Admin,
        Member,
        Guest,
    }

    public static class PredefinedWorkgroupRoleTypeUtils
    {
        public const string Admin_RoleGuid = "33333333-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        public const string Admin_Name = "Workgroup Administrators";
        public const string Admin_Description = "This is a pre-defined, system-level Role for Workgroup Administrators.";
        public const string Member_RoleGuid = "33333333-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        public const string Member_Name = "Workgroup Members";
        public const string Member_Description = "This is a pre-defined, system-level Role for Workgroup Members.";
        public const string Guest_RoleGuid = "33333333-cccc-cccc-cccc-cccccccccccc";
        public const string Guest_Name = "Workgroup Guests";
        public const string Guest_Description = "This is a pre-defined, system-level Role for Workgroup Guests.";

        public static Guid GetRoleGuid_ForRoleType(this PredefinedWorkgroupRoleType roleType)
        {
            if (roleType == PredefinedWorkgroupRoleType.Admin)
            { return new Guid(Admin_RoleGuid); }
            else if (roleType == PredefinedWorkgroupRoleType.Member)
            { return new Guid(Member_RoleGuid); }
            else if (roleType == PredefinedWorkgroupRoleType.Guest)
            { return new Guid(Guest_RoleGuid); }
            else
            { throw new InvalidOperationException("Invalid PredefinedWorkgroupRoleType encountered."); }
        }

        public static string GetName_ForRoleType(this PredefinedWorkgroupRoleType roleType)
        {
            if (roleType == PredefinedWorkgroupRoleType.Admin)
            { return Admin_Name; }
            else if (roleType == PredefinedWorkgroupRoleType.Member)
            { return Member_Name; }
            else if (roleType == PredefinedWorkgroupRoleType.Guest)
            { return Guest_Name; }
            else
            { throw new InvalidOperationException("Invalid PredefinedWorkgroupRoleType encountered."); }
        }

        public static string GetDescription_ForRoleType(this PredefinedWorkgroupRoleType roleType)
        {
            if (roleType == PredefinedWorkgroupRoleType.Admin)
            { return Admin_Description; }
            else if (roleType == PredefinedWorkgroupRoleType.Member)
            { return Member_Description; }
            else if (roleType == PredefinedWorkgroupRoleType.Guest)
            { return Guest_Description; }
            else
            { throw new InvalidOperationException("Invalid PredefinedWorkgroupRoleType encountered."); }
        }

        public static IDictionary<WorkgroupPermissionType, PermissionSetting> GetWorkgroupPermissions_ForRoleType(this PredefinedWorkgroupRoleType roleType)
        {
            var permissions = new Dictionary<WorkgroupPermissionType, PermissionSetting>();

            if (roleType == PredefinedWorkgroupRoleType.Admin)
            {
                foreach (WorkgroupPermissionType permType in Enum.GetValues(typeof(WorkgroupPermissionType)))
                {
                    permissions.Add(permType, PermissionSetting.Allow);
                }
                return permissions;
            }
            else if (roleType == PredefinedWorkgroupRoleType.Member)
            {
                foreach (WorkgroupPermissionType permType in Enum.GetValues(typeof(WorkgroupPermissionType)))
                {
                    if ((permType == WorkgroupPermissionType.ViewMembers) || (permType == WorkgroupPermissionType.ReadWorkgroupInfo) || (permType == WorkgroupPermissionType.SendCommunications))
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else if (roleType == PredefinedWorkgroupRoleType.Guest)
            {
                foreach (WorkgroupPermissionType permType in Enum.GetValues(typeof(WorkgroupPermissionType)))
                {
                    if (permType == WorkgroupPermissionType.ReadWorkgroupInfo)
                    { permissions.Add(permType, PermissionSetting.Allow); }
                    else
                    { permissions.Add(permType, PermissionSetting.Deny); }
                }
                return permissions;
            }
            else
            { throw new InvalidOperationException("Invalid PredefinedWorkgroupRoleType encountered."); }
        }

        public static IDictionary<ProjectPermissionType, PermissionSetting> GetProjectPermissions_ForRoleType(this PredefinedWorkgroupRoleType roleType)
        {
            if (roleType == PredefinedWorkgroupRoleType.Admin)
            { return ProjectRoleType.Admin.GetProjectPermissions_ForRoleType(); }
            else if (roleType == PredefinedWorkgroupRoleType.Member)
            { return ProjectRoleType.Member.GetProjectPermissions_ForRoleType(); }
            else if (roleType == PredefinedWorkgroupRoleType.Guest)
            { return ProjectRoleType.Guest.GetProjectPermissions_ForRoleType(); }
            else
            { throw new InvalidOperationException("Invalid PredefinedWorkgroupRoleType encountered."); }
        }
    }
}