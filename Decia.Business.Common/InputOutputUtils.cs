using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public static class InputOutputUtils
    {
        public static MemoryStream ConvertStringToStream(this string text)
        {
            var value_AsBytes = Encoding.UTF8.GetBytes(text);
            var stream = new MemoryStream(value_AsBytes);
            return stream;
        }

        public static string ConvertStreamToString(this Stream stream)
        {
            var text = string.Empty;

            using (var streamReader = new StreamReader(stream))
            { text = streamReader.ReadToEnd(); }

            return text;
        }
    }
}