using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Modeling
{
    public enum DemoModelType
    {
        Basic,
        Cyclic,
        Extensive
    }

    public static class DemoModelTypeUtils
    {
        public static string GetDefaultProjectName(this DemoModelType demoModelType)
        {
            if (demoModelType == DemoModelType.Basic)
            { return "Basic Test Model Project"; }
            else if (demoModelType == DemoModelType.Cyclic)
            { return "Cyclic Test Model Project"; }
            else if (demoModelType == DemoModelType.Extensive)
            { return "Extensive Test Model Project"; }
            else
            { throw new InvalidOperationException("Unrecognized DemoModelType encountered."); }
        }

        public static string GetDefaultProjectDescription(this DemoModelType demoModelType)
        {
            if (demoModelType == DemoModelType.Basic)
            { return "Project for Model used in \"Basic\" unit tests."; }
            else if (demoModelType == DemoModelType.Cyclic)
            { return "Project for Model used in \"Cyclic\" unit tests."; }
            else if (demoModelType == DemoModelType.Extensive)
            { return "Project for Model used in \"Extensive\" unit tests."; }
            else
            { throw new InvalidOperationException("Unrecognized DemoModelType encountered."); }
        }
    }
}