using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public interface IPropertyDisplayDataOverrider
    {
        bool Get_IsVisible_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args);
        bool Get_IsEditable_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args);
        Dictionary<string, string> Get_AvailableOptions_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args);
        string Get_PopupData_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args);
    }

    public interface IPropertyDisplayDataOverrider_WithValue : IPropertyDisplayDataOverrider
    {
        object Get_Value_ForProperty(PropertyDisplayDataAttribute propertyAttribute, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args);
    }
}