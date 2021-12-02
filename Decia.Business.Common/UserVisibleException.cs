using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Decia.Business.Common
{
    public class UserVisibleException : Exception
    {
        #region Constructors

        public UserVisibleException()
            : base()
        { }

        public UserVisibleException(string message)
            : base(message)
        { }

        public UserVisibleException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public UserVisibleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion
    }

    public static class UserVisibleExceptionUtils
    {
        public static UserVisibleException GetVisibleException(this Exception exception)
        {
            while (exception != null)
            {
                if (exception is UserVisibleException)
                { return (exception as UserVisibleException); }

                exception = exception.InnerException;
            }
            return null;
        }
    }
}