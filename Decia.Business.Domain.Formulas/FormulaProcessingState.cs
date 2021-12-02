using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;

namespace Decia.Business.Domain.Formulas
{
    public class FormulaProcessingState : IFormulaProcessingState
    {
        protected ProcessingState m_InitializationState;
        protected ProcessingState m_ValidationState;
        protected ProcessingState m_ComputationState;

        public FormulaProcessingState()
        {
            m_InitializationState = new ProcessingState();
            m_ValidationState = new ProcessingState(new IProcessingState[] { m_InitializationState });
            m_ComputationState = new ProcessingState(new IProcessingState[] { m_InitializationState, m_ValidationState });
        }

        public IProcessingState InitializationState { get { return m_InitializationState; } }
        public IProcessingState ValidationState { get { return m_ValidationState; } }
        public IProcessingState ComputationState { get { return m_ComputationState; } }

        internal IProcessingState_Editable InitializationState_Editable { get { return m_InitializationState; } }
        internal IProcessingState_Editable ValidationState_Editable { get { return m_ValidationState; } }
        internal IProcessingState_Editable ComputationState_Editable { get { return m_ComputationState; } }
    }
}