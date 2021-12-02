using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public enum NotificationType
    {
        Invite_Acceptance,
        Connection_Request,
        Connection_Acceptance,
        Permission_Granted,
    }

    public enum NotificationStyle
    {
        NotifyOnly,
        Accept,
        AcceptOrReject,
    }

    public enum NotificationResult
    {
        Accept,
        Reject,
    }

    public static class NotificationTypeUtils
    {
        public static NotificationStyle GetStyleForType(this NotificationType notificationType)
        {
            if (notificationType == NotificationType.Invite_Acceptance)
            { return NotificationStyle.Accept; }
            else if (notificationType == NotificationType.Connection_Request)
            { return NotificationStyle.AcceptOrReject; }
            else if (notificationType == NotificationType.Connection_Acceptance)
            { return NotificationStyle.Accept; }
            else if (notificationType == NotificationType.Permission_Granted)
            { return NotificationStyle.Accept; }
            else
            { throw new InvalidOperationException("Unrecognized NotificationType encountered."); }
        }
    }
}