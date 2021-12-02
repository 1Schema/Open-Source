using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Decia.Business.Common.Conversion;

namespace Decia.Business.Common.TypedIds
{
    public static class MultiPartGuidGenerator
    {
        public const bool ForceNonStandardPrefix = true;
        public const string IntToTextFormat = "+00000000000";

        public static Guid GenerateGuid(int firstPart, int secondPart)
        {
            var firstPartAsText = firstPart.ToString(IntToTextFormat);
            var secondPartAsText = secondPart.ToString(IntToTextFormat);
            var fullText = firstPartAsText + "," + secondPartAsText;

            var result = GenerateGuid(fullText);
            return result;
        }

        public static Guid GenerateGuid(int firstPart, Guid secondPart)
        {
            var firstPartAsText = firstPart.ToString(IntToTextFormat);
            var secondPartAsText = secondPart.ToString();
            var fullText = firstPartAsText + "," + secondPartAsText;

            var result = GenerateGuid(fullText);
            return result;
        }

        public static Guid GenerateGuid(string text)
        {
            using (var hashMethod = MD5.Create())
            {
                var textAsBytes = Encoding.Default.GetBytes(text);
                var hash = hashMethod.ComputeHash(textAsBytes);
                var result = new Guid(hash);
                return result;
            }
        }
    }
}