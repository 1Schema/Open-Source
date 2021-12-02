using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Permissions
{
    public interface IUserState
    {
        UserId CurrentUserId { get; }
        Guid CurrentUserGuid { get; }
        string CurrentUsername { get; }
        bool ApplyEscapeChars { get; set; }
    }
}