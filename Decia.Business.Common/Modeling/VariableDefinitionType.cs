using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum VariableDefinitionType
    {
        RequiredBySystem = 0x00000001,
        DefinedByUser = 0x00000010,
        ImpliedDate = 0x00000100,
        ImpliedProbability = 0x00001000
    }

    public static class VariableDefinitionTypeUtils
    {
        public static VariableDefinitionType NotRequiredBySystem
        {
            get
            {
                VariableDefinitionType notRequiredBySystem = VariableDefinitionType.DefinedByUser;
                foreach (VariableDefinitionType type in Enum.GetValues(typeof(VariableDefinitionType)))
                {
                    if (type == VariableDefinitionType.RequiredBySystem)
                    { continue; }
                    notRequiredBySystem = notRequiredBySystem | type;
                }
                return notRequiredBySystem;
            }
        }

        public static bool IsBaseValue(this VariableDefinitionType definitionType)
        {
            foreach (VariableDefinitionType type in Enum.GetValues(typeof(VariableDefinitionType)))
            {
                if (type == definitionType)
                { return true; }
            }
            return false;
        }
    }
}