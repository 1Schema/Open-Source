using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface IArchivable
    {
        bool IsArchived { get; }
        UserId ArchiverId { get; }
        DateTime ArchivalDate { get; }

        bool IsArchivable();
        void SetIsArchived(UserId archiverId, bool isArchived);
    }

    public static class IArchivableUtils
    {
        #region Static Methods - Asserts

        public static void AssertIsArchived(this IArchivable obj)
        {
            AssertIsArchived(obj, "The IArchivable object is not Archived.");
        }

        public static void AssertIsArchived(this IArchivable obj, string message)
        {
            if (!obj.IsArchived)
            { throw new InvalidOperationException(message); }
        }

        public static void AssertIsNotArchived(this IArchivable obj)
        {
            AssertIsNotArchived(obj, "The IArchivable object is Archived.");
        }

        public static void AssertIsNotArchived(this IArchivable obj, string message)
        {
            if (obj.IsArchived)
            { throw new InvalidOperationException(message); }
        }

        public static void AssertArchiverIsNotAnonymous(this IArchivable obj)
        {
            obj.ArchiverId.AssertIsNotAnonymous("The current object's Archiver must not be anonymous.");
        }

        #endregion
    }
}