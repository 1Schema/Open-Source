using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Domain.Formulas;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public interface IReportRenderingEngine
    {
        IReportingDataProvider DataProvider { get; }
        ProjectMemberId ProjectId { get; }

        void SetInitializationInputs(ModelObjectReference modelTemplateRef, IReport report, IEnumerable<IReportElement> reportElements);
        ModelObjectReference ModelTemplateRef { get; }
        IReport Report { get; }
        IDictionary<ReportElementId, IReportElement> ReportElements { get; }

        void SetValidationInputs(ICurrentState modelTemplateState);
        ICurrentState ModelTemplateState { get; }

        void SetDesignInputs( );

        void SetProductionInputs(IDictionary<ModelObjectReference, ICurrentState> modelInstanceStates);
        ICollection<ModelObjectReference> ModelInstanceRefs { get; }
        IDictionary<ModelObjectReference, ICurrentState> ModelInstanceStates { get; }

        IProcessingState InitializationState { get; }
        RenderingResult Initialize();
        ICollection<ReportElementId> MissingReportElementIds { get; }
        ICollection<ReportElementId> InaccessibleReportElementIds { get; }

        IProcessingState ValidationState { get; }
        RenderingResult Validate();
        ITree<ReportElementId> ElementTree { get; }
        ITree<ReportElementId> StyleTree { get; }
        IRenderingState RenderingState { get; }
        bool IsValid { get; }
        bool IsReportValid { get; }
        bool AreReportElementsValid { get; }
        ICollection<ReportElementId> InvalidReportElementIds { get; }

        IProcessingState RenderForDesignState { get; }
        RenderingResult RenderForDesign();
        RenderedReport DesignReport { get; }

        IProcessingState RenderForProductionState { get; }
        RenderingResult RenderForProduction(ModelObjectReference selectedStructuralInstanceRef);
        IDictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderingResult>> RenderForProduction();
        IDictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderingResult>> RenderForProduction(ICollection<ModelObjectReference> selectedStructuralInstanceRefs);
        IDictionary<ModelObjectReference, IDictionary<ModelObjectReference, RenderedReport>> ProductionReports { get; }
    }
}