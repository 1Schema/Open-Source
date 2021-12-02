using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.ValueObjects
{
    public enum ProcessingAcivityType
    {
        Initialization,
        Validation,
        Computation,
        DesignRendering,
        ProductionRendering
    }

    public static class ProcessingAcivityTypeUtils
    {
        public static readonly ProcessingAcivityType[] ValidTypesForFormulaProcessing = new ProcessingAcivityType[] { ProcessingAcivityType.Initialization, ProcessingAcivityType.Validation, ProcessingAcivityType.Computation };
        public static readonly ProcessingAcivityType[] ValidTypesForReportRendering = new ProcessingAcivityType[] { ProcessingAcivityType.Initialization, ProcessingAcivityType.Validation, ProcessingAcivityType.DesignRendering, ProcessingAcivityType.ProductionRendering };

        public static bool GetIsValidInFormulaProcessing(this ProcessingAcivityType processingAcivityType)
        {
            return ValidTypesForFormulaProcessing.Contains(processingAcivityType);
        }

        public static void AssertIsValidInFormulaProcessing(this ProcessingAcivityType processingAcivityType)
        {
            if (!GetIsValidInFormulaProcessing(processingAcivityType))
            { throw new InvalidOperationException("The specified Processing Activity Type is not valid in Formula Processing"); }
        }

        public static bool GetIsValidInReportRendering(this ProcessingAcivityType processingAcivityType)
        {
            return ValidTypesForReportRendering.Contains(processingAcivityType);
        }

        public static void AssertIsValidInReportRendering(this ProcessingAcivityType processingAcivityType)
        {
            if (!GetIsValidInReportRendering(processingAcivityType))
            { throw new InvalidOperationException("The specified Processing Activity Type is not valid in Report Rendering"); }
        }
    }
}