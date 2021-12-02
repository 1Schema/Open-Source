using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Exceptions
{
    public static class ExceptionUtils
    {
        public static bool ContainsTextInMessage(this Exception exception, string text)
        {
            if (exception == null)
            { return false; }

            if (exception.Message.Contains(text))
            { return true; }

            return ContainsTextInMessage(exception.InnerException, text);
        }

        public static bool ContainsTextInStackTrace(this Exception exception, string text)
        {
            if (exception == null)
            { return false; }

            if (exception.StackTrace.Contains(text))
            { return true; }

            return ContainsTextInStackTrace(exception.InnerException, text);
        }
    }
}