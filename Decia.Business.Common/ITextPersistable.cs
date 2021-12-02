using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface ITextPersistable<T>
        where T : ITextPersistable<T>
    {
        string SaveAsText();

        bool TryLoadFromText(string text, out T newValue);
        T LoadFromText(string text);
    }
}