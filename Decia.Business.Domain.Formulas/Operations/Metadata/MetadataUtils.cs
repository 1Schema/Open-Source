using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Time;
using Decia.Business.Domain.ChronometricValues;
using Decia.Business.Domain.CompoundUnits;
using Decia.Business.Domain.Formulas.Exports;

namespace Decia.Business.Domain.Formulas.Operations.Metadata
{
    public static class MetadataUtils
    {
        public const string MetadataCategoryName = "Metadata";

        public static SqlArgumentInfo GetSqlArgInfo(this ModelObjectReference variableTemplateRef, IFormula parentFormula, IExpression parentExpression, SqlFormulaInfo formulaInfo)
        {
            var baseInfo = formulaInfo.ArgumentInfos[variableTemplateRef];

            return baseInfo;
        }

        public static string GetSqlText(this ModelObjectReference modelObjRef, SqlFormulaInfo formulaInfo, SqlArgumentInfo argumentInfo)
        {
            if (modelObjRef.ModelObjectType == ModelObjectType.ModelTemplate)
            {
                formulaInfo.Metadata_IsUsed = true;
                return string.Format("{0}.[{1}]", formulaInfo.Metadata_TableAlias, "Name");
            }
            else if (modelObjRef.ModelObjectType == ModelObjectType.TimeType)
            {
                var timeDimensionType = TimeDimensionTypeUtils.GetTimeDimensionTypeForNumber(modelObjRef.ModelObjectIdAsInt);
                return string.Format("{0} Time Dimension", timeDimensionType);
            }
            else if (ModelObjectTypeUtils.IsStructuralType(modelObjRef.ModelObjectType))
            {
                argumentInfo.StructuralType_IsUsed = true;
                return string.Format("{0}.[{1}]", argumentInfo.StructuralType_TableAlias, "Name");
            }
            else if (modelObjRef.ModelObjectType == ModelObjectType.VariableTemplate)
            {
                argumentInfo.VariableTemplate_IsUsed = true;
                return string.Format("{0}.[{1}]", argumentInfo.VariableTemplate_TableAlias, "Name");
            }
            else
            { throw new InvalidOperationException("The specified Object Type is not supported."); }
        }
    }
}