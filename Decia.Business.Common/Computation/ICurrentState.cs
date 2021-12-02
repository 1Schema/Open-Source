using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Common.Computation
{
    public interface ICurrentState : ICurrentState_Normal, ICurrentState_Anonymous
    {
    }
}