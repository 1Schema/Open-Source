﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Domain.DynamicValues;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public interface IVariableDataRange : IValueRange_Configurable, ITransposableElement, IHidableElement
    {
        string StyleGroup { get; set; }

        ModelObjectReference ValueVariableTemplateRef { get; }
    }
}