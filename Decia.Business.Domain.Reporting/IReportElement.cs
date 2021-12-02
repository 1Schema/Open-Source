using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common;
using Decia.Business.Common.Computation;
using Decia.Business.Common.DataStructures;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Layouts;
using Decia.Business.Domain.Styling;
using Decia.Business.Domain.Reporting.Rendering;

namespace Decia.Business.Domain.Reporting
{
    public interface IReportElement<T> : IReportElement, IModelMember_Deleteable<T>, IProjectMember_Cloneable<T>
         where T : IReportElement<T>
    { }

    public interface IReportElement : IModelMember_Orderable
    {
        ReportElementId Key { get; }
        int ElementNumber { get; }
        string Name { get; set; }
        ModelObjectReference ReportElementTypeRef { get; }

        ReportElementType_New ReportElementType { get; }
        bool HasDataBoundMultiplicity { get; }
        ReportId ParentReportId { get; }
        Guid ParentReportGuid { get; }

        Nullable<ReportElementId> ParentElementId { get; set; }
        Nullable<int> ParentElementNumber { get; set; }
        ReportElementId ParentElementOrReportId { get; }
        int ParentElementOrReportNumber { get; }

        bool IsLocked { get; set; }
        int ZOrder { get; set; }

        bool IsParentEditable { get; set; }
        bool IsDirectlyDeletable { get; set; }

        bool IsContainer { get; }
        bool AutoDeletesChildren { get; }
        bool AreContentsEditable { get; }
        ICollection<ReportElementType_New> AcceptableContentTypes { get; }

        bool IsPositionEditable { get; }
        bool IsSizeEditable { get; }
        bool RequiresDelayedSizing { get; }
        bool IsDirectlyTransposable { get; }
        bool IsStyleEditable { get; }

        IElementLayout ElementLayout { get; }
        KnownStyleType DefaultStyleType { get; set; }
        Nullable<ReportElementId> StyleInheritanceElementId { get; set; }
        Nullable<int> StyleInheritanceElementNumber { get; set; }
        IElementStyle ElementStyle { get; }

        IReportElement Copy();
        bool Equals(IReportElement otherReportElement);

        RenderingResult Initialize(IReportingDataProvider dataProvider, IRenderingState renderingState);
        RenderingResult Validate(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree);

        RenderingResult RenderForDesign(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey);
        RenderingResult RenderForProduction(IReportingDataProvider dataProvider, IRenderingState renderingState, ITree<ReportElementId> elementTree, IDictionary<RenderingKey, RenderedLayout> layoutResults, RenderingKey parentRenderingKey);
    }
}