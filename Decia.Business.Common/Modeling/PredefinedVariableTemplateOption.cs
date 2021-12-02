using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum PredefinedVariableTemplateOption
    {
        [Description("Id")]
        Id,
        [Description("Parent Id")]
        Id_Parent,
        [Description("Related Id")]
        Id_Related,
        [Description("Name")]
        Name,
        [Description("Sorting Number")]
        Order
    }

    public static class PredefinedVariableTemplateOptionUtils
    {
        public static string GetNameForOption(this PredefinedVariableTemplateOption option)
        {
            var defaultName = EnumUtils.GetDescription<PredefinedVariableTemplateOption>(option);
            return defaultName;
        }

        public static string GetDescriptionForOption(this PredefinedVariableTemplateOption option)
        {
            return string.Empty;
        }

        public static VariableType GetVariableTypeForOption(this PredefinedVariableTemplateOption option)
        {
            return VariableType.Input;
        }

        public static bool GetIsStructuralVariableForOption(this PredefinedVariableTemplateOption option)
        {
            if (option == PredefinedVariableTemplateOption.Id)
            {
                return false;
            }
            else if (option == PredefinedVariableTemplateOption.Id_Parent)
            {
                return true;
            }
            else if (option == PredefinedVariableTemplateOption.Id_Related)
            {
                return true;
            }
            else if (option == PredefinedVariableTemplateOption.Name)
            {
                return false;
            }
            else if (option == PredefinedVariableTemplateOption.Order)
            {
                return false;
            }
            else
            { throw new InvalidOperationException("Unrecognized PredefinedVariableTemplateOption encountered."); }
        }

        public static DeciaDataType GetDataTypeForOption(this PredefinedVariableTemplateOption option)
        {
            if (option == PredefinedVariableTemplateOption.Id)
            {
                return DeciaDataType.UniqueID;
            }
            else if (option == PredefinedVariableTemplateOption.Id_Parent)
            {
                return DeciaDataType.UniqueID;
            }
            else if (option == PredefinedVariableTemplateOption.Id_Related)
            {
                return DeciaDataType.UniqueID;
            }
            else if (option == PredefinedVariableTemplateOption.Name)
            {
                return DeciaDataType.Text;
            }
            else if (option == PredefinedVariableTemplateOption.Order)
            {
                return DeciaDataType.Integer;
            }
            else
            { throw new InvalidOperationException("Unrecognized PredefinedVariableTemplateOption encountered."); }
        }
    }
}