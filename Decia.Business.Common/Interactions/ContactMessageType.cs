using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Decia.Business.Common.Interactions
{
    public enum ContactMessageType
    {
        [Description("Error")]
        Error,
        [Description("Problem")]
        Problem,
        [Description("Question")]
        Question,
        [Description("Feedback")]
        Feedback,
    }

    public static class ContactMessageTypeUtils
    {
        public const ContactMessageType Error_MessageType = ContactMessageType.Error;
        public const ContactMessageType Initial_MessageType = ContactMessageType.Question;

        public static string Error_MessageType_Name { get { return EnumUtils.GetName<ContactMessageType>(Error_MessageType); } }
        public static string Error_MessageType_Description { get { return EnumUtils.GetDescription<ContactMessageType>(Error_MessageType); } }

        public static bool Is_ForPublicDisplay(this ContactMessageType messageType)
        {
            return (messageType != Error_MessageType);
        }

        public static List<SelectListItem> GetOptions_ForPublicDisplay()
        {
            return GetOptions_ForPublicDisplay(Initial_MessageType);
        }

        public static List<SelectListItem> GetOptions_ForPublicDisplay(ContactMessageType selectedMessageType)
        {
            var selectOptions = new List<SelectListItem>();

            foreach (var messageType in EnumUtils.GetEnumValues<ContactMessageType>())
            {
                if (!messageType.Is_ForPublicDisplay())
                { continue; }

                var enumName = EnumUtils.GetName<ContactMessageType>(messageType);
                var enumDescription = EnumUtils.GetDescription<ContactMessageType>(messageType);
                var isSelected = (messageType == selectedMessageType);
                var selectOption = new SelectListItem() { Value = enumName, Text = enumDescription, Selected = isSelected };

                selectOptions.Add(selectOption);
            }
            return selectOptions;
        }
    }
}