using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public enum PredefinedUserType
    {
        System,
        Anonymous,
    }

    public static class PredefinedUserTypeUtils
    {
        public const string System_UserGuid = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee";
        public const string System_Username = "System";
        public const string Anonymous_UserGuid = "00000000-0000-0000-0000-000000000000";
        public const string Anonymous_Username = "Anonymous";

        public static UserId SystemUserId { get { return GetUserId_ForUserType(PredefinedUserType.System); } }
        public static UserId AnonymousUserId { get { return GetUserId_ForUserType(PredefinedUserType.Anonymous); } }

        public static UserId GetUserId_ForUserType(this PredefinedUserType userType)
        {
            var userGuid = GetUserGuid_ForUserType(userType);
            return new UserId(userGuid);
        }

        public static Guid GetUserGuid_ForUserType(this PredefinedUserType userType)
        {
            if (userType == PredefinedUserType.System)
            { return new Guid(System_UserGuid); }
            else if (userType == PredefinedUserType.Anonymous)
            { return new Guid(Anonymous_UserGuid); }
            else
            { throw new InvalidOperationException("Invalid PredefinedUserType encountered."); }
        }

        public static string GetUsername_ForUserType(this PredefinedUserType userType)
        {
            if (userType == PredefinedUserType.System)
            { return System_Username; }
            else if (userType == PredefinedUserType.Anonymous)
            { return Anonymous_Username; }
            else
            { throw new InvalidOperationException("Invalid PredefinedUserType encountered."); }
        }

        public static string GetEmailAddress_ForUserType(this PredefinedUserType userType)
        {
            if (userType == PredefinedUserType.System)
            { return (System_Username + "@" + EmailUtils.Decia_Domain); }
            else if (userType == PredefinedUserType.Anonymous)
            { return (Anonymous_Username + "@" + EmailUtils.Decia_Domain); }
            else
            { throw new InvalidOperationException("Invalid PredefinedUserType encountered."); }
        }
    }
}