using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum SystemRoleType
    {
        Admin,
        User,
        Anonymous,
    }

    public static class SystemRoleTypeUtils
    {
        public const string Admin_RoleGuid = "11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        public const string Admin_Name = "Administrators";
        public const string Admin_Description = "System Administrators have full permissions to all groups and projects that exist.";
        public const string User_RoleGuid = "11111111-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        public const string User_Name = "Users";
        public const string User_Description = "System Users must be given permissions on groups and projects that they do not own.";
        public const string Anonymous_RoleGuid = "11111111-cccc-cccc-cccc-cccccccccccc";
        public const string Anonymous_Name = "Anonymous";
        public const string Anonymous_Description = "Anonymous Users (i.e. not Authenticated Users) have absolutely no permissions.";

        public static Guid AdminRoleGuid { get { return GetRoleGuid_ForRoleType(SystemRoleType.Admin); } }
        public static Guid UserRoleGuid { get { return GetRoleGuid_ForRoleType(SystemRoleType.User); } }
        public static Guid AnonymousRoleGuid { get { return GetRoleGuid_ForRoleType(SystemRoleType.Anonymous); } }

        public static Guid GetRoleGuid_ForRoleType(this SystemRoleType roleType)
        {
            if (roleType == SystemRoleType.Admin)
            { return new Guid(Admin_RoleGuid); }
            else if (roleType == SystemRoleType.User)
            { return new Guid(User_RoleGuid); }
            else if (roleType == SystemRoleType.Anonymous)
            { return new Guid(Anonymous_RoleGuid); }
            else
            { throw new InvalidOperationException("Invalid SystemRoleType encountered."); }
        }

        public static string GetName_ForRoleType(this SystemRoleType roleType)
        {
            if (roleType == SystemRoleType.Admin)
            { return Admin_Name; }
            else if (roleType == SystemRoleType.User)
            { return User_Name; }
            else if (roleType == SystemRoleType.Anonymous)
            { return Anonymous_Name; }
            else
            { throw new InvalidOperationException("Invalid SystemRoleType encountered."); }
        }

        public static string GetDescription_ForRoleType(this SystemRoleType roleType)
        {
            if (roleType == SystemRoleType.Admin)
            { return Admin_Description; }
            else if (roleType == SystemRoleType.User)
            { return User_Description; }
            else if (roleType == SystemRoleType.Anonymous)
            { return Anonymous_Description; }
            else
            { throw new InvalidOperationException("Invalid SystemRoleType encountered."); }
        }
    }
}