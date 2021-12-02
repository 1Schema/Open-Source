using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Time;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public interface IReport<T> : IReport, IModelMember_Deleteable<T>
         where T : IReport<T>
    { }

    public interface IReport : IModelMember_Orderable
    {
        ReportId Key { get; }
        ReportElementId KeyAsElementId { get; }
        Guid ReportGuid { get; }

        string Name { get; set; }
        string Description { get; set; }
        ModelObjectReference ReportTemplateRef { get; }
        bool IsLocked { get; set; }

        bool IsContainer { get; }
        bool AreContentsEditable { get; }
        ICollection<ReportElementType_New> AcceptableContentTypes { get; }

        ModelObjectReference ModelTemplateRef { get; }
        ModelObjectReference StructuralTypeRef { get; set; }
        IDictionary<TimeDimensionType, Nullable<TimePeriodType>> TimeDimensionUsages { get; }

        bool HasTimePeriodType(int timeDimensionNumber);
        TimePeriodType GetTimePeriodType(int timeDimensionNumber);
        Nullable<TimePeriodType> GetNullableTimePeriodType(int timeDimensionNumber);

        double ZoomFactor { get; set; }
        IElementLayout ReportAreaLayout { get; }
        IElementStyle ReportAreaStyle { get; }
        IElementStyle OutsideAreaStyle { get; }

        IElementStyle DefaultTitleStyle { get; }
        IElementStyle DefaultHeaderStyle { get; }
        IElementStyle DefaultDataStyle { get; }

        IReport Copy();
        bool Equals(IReport otherReport);

        RenderingResult Initialize(IReportingDataProvider dataProvider, IRenderingState renderingState);
        RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree);

        RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults);
        RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults);
    }
}