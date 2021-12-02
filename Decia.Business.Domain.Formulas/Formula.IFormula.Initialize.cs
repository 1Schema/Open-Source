using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Computation;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Structure;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Domain.Structure;

namespace Decia.Business.Domain.Formulas
{
    public partial class Formula
    {
        public const bool InitilizationUsesExtendedStructure = false;

        public ComputationResult Initialize(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            if (currentState.ProcessingType == ProcessingType.Normal)
            { return Initialize_Normal(dataProvider, currentState); }
            else if (currentState.ProcessingType == ProcessingType.Anonymous)
            { return Initialize_Anonymous((dataProvider as IFormulaDataProvider_Anonymous), currentState); }
            else
            { throw new InvalidOperationException("Invalid ProcessingType encountered."); }
        }

        protected ComputationResult Initialize_Normal(IFormulaDataProvider dataProvider, ICurrentState currentState)
        {
            var result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key);

            var structuralContext = this.BuildStructuralContext(dataProvider, currentState);
            if (!structuralContext.IsResultingSpaceSet)
            {
                result.SetErrorState(ComputationResultType.FormulaInitializationFailed, "Failed to construct a StructuralContext for the current Formula.");
                return result;
            }

            result.SetInitializedState();
            dataProvider.SetIsInitialized(currentState.VariableTemplateRef, true);
            return result;
        }

        protected ComputationResult Initialize_Anonymous(IFormulaDataProvider_Anonymous dataProvider, ICurrentState currentState)
        {
            var result = new ComputationResult(currentState.ModelTemplateRef, currentState.NullableModelInstanceRef, this.Key);

            var structuralContext = this.BuildAnonymousStructuralContext(dataProvider, currentState);
            if (!structuralContext.IsResultingSpaceSet)
            {
                result.SetErrorState(ComputationResultType.FormulaInitializationFailed, "Failed to construct a StructuralContext for the current Formula.");
                return result;
            }

            result.SetInitializedState();
            dataProvider.SetAnonymousVariableIsInitialized(currentState.VariableTemplateRef, true);
            return result;
        }
    }
}