using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.DomainModeling.DomainModels;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Styling;
using Decia.Business.Domain.Formulas;

namespace Decia.Business.Domain.Reporting
{
    public interface IReportDataObject
    {
        IDomainModel DomainModel { get; }
        RevisionChain RevisionChain { get; }

        ModelObjectReference ModelTemplateRef { get; }
        int ModelTemplateNumber { get; }

        ModelObjectReference ReportRef { get; }
        Guid ReportGuid { get; }
        ReportId ReportId { get; }
        Report Report { get; }

        IDictionary<ReportElementId, IReportElement> ReportElements { get; }
        IDictionary<FormulaId, Formula> Formulas { get; }
        IDictionary<FormulaId, IReportElement> FormulaElements { get; }

        IDictionary<ModelObjectReference, IReportElement> ReportElementsByRef { get; }
        IDictionary<ModelObjectReference, Formula> FormulasByVariableTemplateRef { get; }
    }

    public static class IReportDataObject_Utils
    {
        public static void Initialize_DefaultStyle(this IReport report, IReportElement element)
        {
            if (element is IReport)
            { return; }
            if (element.DefaultStyleType == KnownStyleType.None)
            { return; }

            var previousDefault = element.ElementStyle.DefaultStyle_Value;

            if (element.DefaultStyleType == KnownStyleType.Data)
            { element.ElementStyle.DefaultStyle_Value = report.DefaultDataStyle; }
            else if (element.DefaultStyleType == KnownStyleType.Header)
            { element.ElementStyle.DefaultStyle_Value = report.DefaultHeaderStyle; }
            else if (element.DefaultStyleType == KnownStyleType.Title)
            { element.ElementStyle.DefaultStyle_Value = report.DefaultTitleStyle; }
            else
            { throw new InvalidOperationException("Unrecognized KnownStyleType encountered."); }

            var currStyle = element.ElementStyle.DefaultStyle_Value;
            while (currStyle.DefaultStyle_Value != null)
            { currStyle = currStyle.DefaultStyle_Value; }

            currStyle.DefaultStyle_Value = previousDefault;
        }
    }
}