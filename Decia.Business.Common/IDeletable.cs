using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface IDeletable
    {
        bool IsDeleted { get; }
        UserId DeleterId { get; }
        DateTime DeletionDate { get; }

        bool IsDeletable();
        void SetToDeleted();
        void SetToDeleted(UserId deleterId);
    }

    public static class IDeletableUtils
    {
        #region Static Methods - Asserts

        public static void AssertIsDeleted(this IDeletable obj)
        {
            AssertIsDeleted(obj, "The IDeletable object is not Deleted.");
        }

        public static void AssertIsDeleted(this IDeletable obj, string message)
        {
            if (!obj.IsDeleted)
            { throw new InvalidOperationException(message); }
        }

        public static void AssertIsNotDeleted(this IDeletable obj)
        {
            AssertIsNotDeleted(obj, "The IDeletable object is Deleted.");
        }

        public static void AssertIsNotDeleted(this IDeletable obj, string message)
        {
            if (obj.IsDeleted)
            { throw new InvalidOperationException(message); }
        }

        public static void AssertDeleterIsNotAnonymous(this IDeletable obj)
        {
            obj.DeleterId.AssertIsNotAnonymous("The current object's Deleter must not be anonymous.");
        }

        #endregion
    }
}