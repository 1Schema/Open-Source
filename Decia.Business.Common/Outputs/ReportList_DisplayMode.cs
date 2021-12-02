using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public enum ReportList_DisplayMode
    {
        ShowBy_ModelInstance_ReportTemplate,
        ShowBy_ReportTemplate_ModelInstance
    }

    public static class ReportList_DisplayModeUtils
    {
        public static readonly ReportList_DisplayMode Default_DisplayMode = ReportList_DisplayMode.ShowBy_ModelInstance_ReportTemplate;
        public static readonly bool Default_FilterForSpecific_ModelInstances = false;
        public static readonly bool Default_FilterForSpecific_ReportTemplates = false;
    }
}