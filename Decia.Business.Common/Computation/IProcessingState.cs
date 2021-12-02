using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Computation
{
    public interface IProcessingState
    {
        ICollection<IProcessingState> Precedents { get; }

        bool Ready { get; }
        bool Started { get; }
        bool Finished { get; }
        bool Succeeded { get; }
    }

    public interface IProcessingState_Editable : IProcessingState
    {
        void Start();
        void Finish(bool succeeded);
    }
}