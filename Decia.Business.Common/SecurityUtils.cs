using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Decia.Business.Common
{
    public static class SecurityUtils
    {
        public const bool Default_AllowAnonymous = true;

        public static string GetUserName(this IPrincipal currentUser)
        {
            return GetUserName(currentUser, Default_AllowAnonymous);
        }

        public static string GetUserName(this IPrincipal currentUser, bool allowAnonymous)
        {
            if (currentUser == null)
            { return null; }
            return GetUserName(currentUser.Identity);
        }

        public static string GetUserName(this IIdentity currentUser)
        {
            return GetUserName(currentUser, Default_AllowAnonymous);
        }

        public static string GetUserName(this IIdentity currentUser, bool allowAnonymous)
        {
            if (currentUser == null)
            { return null; }
            if (!allowAnonymous && !currentUser.IsAuthenticated)
            { return null; }
            if (string.IsNullOrWhiteSpace(currentUser.Name))
            { return null; }
            return currentUser.Name;
        }
    }
}