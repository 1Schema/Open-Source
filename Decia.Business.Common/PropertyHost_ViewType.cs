using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public enum PropertyHost_ViewType
    {
        None = 0x00000000,
        StructuralMap = 0x00000001,
        DependencyMap = 0x00000010,
        ReportDesigner = 0x00000100,
        All = (StructuralMap | DependencyMap | ReportDesigner)
    }
}