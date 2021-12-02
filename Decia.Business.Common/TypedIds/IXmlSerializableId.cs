using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.TypedIds
{
    public interface IXmlSerializableId
    {
        string StoreToXml();
        void LoadFromXml(string xml);
    }
}