using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface INotifiable
    {
        NotificationId NotificationId { get; }

        bool IsHandleable { get; }
        bool IsHandled { get; }
        DateTime LastChangeDate { get; }

        bool ProvidesNotification { get; }
        NotificationType? NotificationType { get; }

        bool TryHandle();
        void Handle();
    }

    public static class INotifiableUtils
    {
        public const bool AutoHandle_OnActivation = false;
        public static readonly TimeSpan Default_ExpirationDuration = new TimeSpan(30, 0, 0, 0, 0);

        public static bool Get_AutoHandle_OnActivation(this INotifiable notifiable)
        {
            return AutoHandle_OnActivation;
        }

        public static bool IsExpired(this INotifiable notifiable)
        {
            return IsExpired(notifiable, DateTime.UtcNow);
        }

        public static bool IsExpired(this INotifiable notifiable, DateTime now)
        {
            var durationPassed = (now - notifiable.LastChangeDate);
            return (Default_ExpirationDuration < durationPassed);
        }

        public static void AssertIsNotExpired(this INotifiable notifiable)
        {
            AssertIsNotExpired(notifiable, DateTime.UtcNow);
        }

        public static void AssertIsNotExpired(this INotifiable notifiable, DateTime now)
        {
            if (IsExpired(notifiable, now))
            { throw new InvalidOperationException("The Notifiable object has expired."); }
        }

        public static void AssertIsHandleable(this INotifiable notifiable)
        {
            if (!notifiable.IsHandleable)
            { throw new InvalidOperationException("The Notifiable object is not Handleable."); }
        }

        public static void AssertIsNotHandleable(this INotifiable notifiable)
        {
            if (notifiable.IsHandleable)
            { throw new InvalidOperationException("The Notifiable object is Handleable."); }
        }

        public static void AssertIsHandled(this INotifiable notifiable)
        {
            if (!notifiable.IsHandled)
            { throw new InvalidOperationException("The Notifiable object is not Handled."); }
        }

        public static void AssertIsNotHandled(this INotifiable notifiable)
        {
            if (notifiable.IsHandled)
            { throw new InvalidOperationException("The Notifiable object is Handled."); }
        }
    }
}