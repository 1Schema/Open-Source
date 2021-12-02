using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Computation
{
    public enum ProcessingType
    {
        None = 0x00,
        Normal = 0x01,
        Anonymous = 0x10,
        NormalAndAnonymous = (Normal | Anonymous)
    }

    public static class ProcessingTypeUtils
    {
        public static bool IsNormal(this ProcessingType processingType)
        {
            return (processingType == (processingType | ProcessingType.Normal));
        }

        public static bool IsAnonymous(this ProcessingType processingType)
        {
            return (processingType == (processingType | ProcessingType.Anonymous));
        }

        public static bool IsNormalOrAnonymous(this ProcessingType processingType)
        {
            return (IsNormal(processingType) || IsAnonymous(processingType));
        }

        public static bool IsNormalAndAnonymous(this ProcessingType processingType)
        {
            return (IsNormal(processingType) && IsAnonymous(processingType));
        }

        public static void AssertIsNormal(this ProcessingType processingType)
        {
            if (!IsNormal(processingType))
            { throw new InvalidOperationException("The current ProcessingType does not equal \"Normal\"."); }
        }

        public static void AssertIsAnonymous(this ProcessingType processingType)
        {
            if (!IsAnonymous(processingType))
            { throw new InvalidOperationException("The current ProcessingType does not equal \"Anonymous\"."); }
        }

        public static void AssertIsNormalOrAnonymous(this ProcessingType processingType)
        {
            if (!IsNormalOrAnonymous(processingType))
            { throw new InvalidOperationException("The current ProcessingType does not equal \"Normal\" or \"Anonymous\"."); }
        }

        public static void AssertIsNormalAndAnonymous(this ProcessingType processingType)
        {
            if (!IsNormalAndAnonymous(processingType))
            { throw new InvalidOperationException("The current ProcessingType does not equal \"Normal\" and \"Anonymous\"."); }
        }
    }
}