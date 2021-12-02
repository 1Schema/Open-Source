using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Computation
{
    public class ProcessingState : IProcessingState_Editable
    {
        private List<IProcessingState> m_Precedents;
        private bool m_Started;
        private bool m_Finished;
        private bool m_Succeeded;

        public ProcessingState()
            : this(new IProcessingState[] { })
        { }

        public ProcessingState(IEnumerable<IProcessingState> precedents)
        {
            m_Precedents = new List<IProcessingState>(precedents);
            m_Started = false;
            m_Finished = false;
            m_Succeeded = false;
        }

        public ICollection<IProcessingState> Precedents
        {
            get { return new List<IProcessingState>(m_Precedents); }
        }

        public bool Ready
        {
            get
            {
                foreach (var precedent in m_Precedents)
                {
                    if (!precedent.Finished)
                    { return false; }
                }

                foreach (var precedent in m_Precedents)
                {
                    if (!precedent.Succeeded)
                    { return false; }
                }

                return true;
            }
        }

        public bool Started
        {
            get { return m_Started; }
        }

        public bool Finished
        {
            get { return m_Finished; }
        }

        public bool Succeeded
        {
            get { return m_Succeeded; }
        }

        public void Start()
        {
            if (!Ready)
            { throw new InvalidOperationException("The ProcessingState instance is not Ready."); }
            if (Started)
            { throw new InvalidOperationException("The ProcessingState instance has already Started."); }

            m_Started = true;
        }

        public void Finish(bool succeeded)
        {
            if (!Ready)
            { throw new InvalidOperationException("The ProcessingState instance is not Ready."); }
            if (!Started)
            { throw new InvalidOperationException("The ProcessingState instance is not Started."); }
            if (Finished)
            { throw new InvalidOperationException("The ProcessingState instance has already Finished."); }

            m_Finished = true;
            m_Succeeded = succeeded;
        }
    }
}