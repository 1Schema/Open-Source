using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.ValueObjects;
using Decia.Business.Common.Modeling;

namespace Decia.Business.Domain.Reporting
{
    public class RenderingResult
    {
        #region Constants

        public const RenderingResultType DefaultResultType = RenderingResultType.ReportValidationPending;
        public const string DefaultErrorMessage = "No validation has been performed yet.";
        public const string NestedErrorMessage = "Nested error occurred.";

        #endregion

        #region Members

        private ModelObjectReference m_ModelTemplateRef;
        private Nullable<ModelObjectReference> m_ModelInstanceRef;
        private Nullable<ModelObjectReference> m_StructuralTypeRef;
        private Nullable<ModelObjectReference> m_StructuralInstanceRef;
        private Nullable<ReportId> m_ReportId;
        private Nullable<int> m_ReportElementInt;

        private Dictionary<ReportId, RenderingResult> m_NestedReportResults;
        private Dictionary<ReportElementId, List<RenderingResult>> m_NestedReportElementResults;

        private ProcessingAcivityType m_MethodType;
        private bool m_IsValid;
        private RenderingResultType m_ResultType;
        private string m_ErrorMessage;

        #endregion

        #region Constructors

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, null, null, null, null)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, ReportId reportId, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, null, null, reportId, null)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, ReportElementId reportElementId, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, null, null, reportElementId.ReportId, reportElementId.ReportElementNumber)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, ReportId reportId, Nullable<int> reportElementInt, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, null, null, reportId, reportElementInt)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, Nullable<ModelObjectReference> structuralTypeRef, Nullable<ModelObjectReference> structuralInstanceRef, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, structuralTypeRef, structuralInstanceRef, null, null)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, Nullable<ModelObjectReference> structuralTypeRef, Nullable<ModelObjectReference> structuralInstanceRef, ReportId reportId, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, structuralTypeRef, structuralInstanceRef, reportId, null)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, Nullable<ModelObjectReference> structuralTypeRef, Nullable<ModelObjectReference> structuralInstanceRef, ReportElementId reportElementId, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, structuralTypeRef, structuralInstanceRef, reportElementId.ReportId, reportElementId.ReportElementNumber)
        { }

        public RenderingResult(ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, Nullable<ModelObjectReference> structuralTypeRef, Nullable<ModelObjectReference> structuralInstanceRef, ReportId reportId, Nullable<int> reportElementInt, ProcessingAcivityType methodType)
            : this(methodType, modelTemplateRef, modelInstanceRef, structuralTypeRef, structuralInstanceRef, reportId, reportElementInt)
        { }

        protected RenderingResult(ProcessingAcivityType methodType, ModelObjectReference modelTemplateRef, Nullable<ModelObjectReference> modelInstanceRef, Nullable<ModelObjectReference> structuralTypeRef, Nullable<ModelObjectReference> structuralInstanceRef, Nullable<ReportId> reportId, Nullable<int> reportElementInt)
        {
            methodType.AssertIsValidInReportRendering();

            m_ModelTemplateRef = modelTemplateRef;
            m_ModelInstanceRef = modelInstanceRef;
            m_StructuralTypeRef = structuralTypeRef;
            m_StructuralInstanceRef = structuralInstanceRef;

            m_ReportId = reportId;
            m_ReportElementInt = reportElementInt;

            m_NestedReportResults = new Dictionary<ReportId, RenderingResult>();
            m_NestedReportElementResults = new Dictionary<Reporting.ReportElementId, List<RenderingResult>>();

            m_MethodType = methodType;
            m_IsValid = false;
            m_ResultType = DefaultResultType;
            m_ErrorMessage = DefaultErrorMessage;
        }

        #endregion

        #region Related Formula State

        public ModelObjectReference ModelTemplateRef
        {
            get { return m_ModelTemplateRef; }
        }

        public Nullable<ModelObjectReference> ModelInstanceRef
        {
            get { return m_ModelInstanceRef; }
        }

        public Nullable<ModelObjectReference> StructuralTypeRef
        {
            get { return m_StructuralTypeRef; }
        }

        public Nullable<ModelObjectReference> StructuralInstanceRef
        {
            get { return m_StructuralInstanceRef; }
        }

        public Nullable<ReportId> ReportId
        {
            get { return m_ReportId; }
        }

        public Nullable<ReportElementId> ReportElementId
        {
            get
            {
                if (!m_ReportId.HasValue)
                { return null; }
                if (!m_ReportElementInt.HasValue)
                { return null; }
                return new ReportElementId(m_ReportId.Value, m_ReportElementInt.Value, false);
            }
        }

        public ReportComponentType ReportComponentType
        {
            get
            {
                if (m_ReportElementInt.HasValue)
                { return ReportComponentType.ReportElement; }
                if (m_ReportId.HasValue)
                { return ReportComponentType.Report; }
                return ReportComponentType.None;
            }
        }

        public IDictionary<ReportId, RenderingResult> NestedReportResults
        {
            get { return new Dictionary<ReportId, RenderingResult>(m_NestedReportResults); }
        }

        public IDictionary<ReportElementId, List<RenderingResult>> NestedReportElementResults
        {
            get { return m_NestedReportElementResults.ToDictionary(x => x.Key, x => x.Value.ToList(), ReportRenderingEngine.EqualityComparer_ReportElementId); }
        }

        #endregion

        #region Validation Properties

        public ProcessingAcivityType AcivityType
        {
            get { return m_MethodType; }
        }

        public bool IsValid
        {
            get { return m_IsValid; }
        }

        public RenderingResultType ResultType
        {
            get { return m_ResultType; }
        }

        public string ErrorMessage
        {
            get { return m_ErrorMessage; }
        }

        #endregion

        internal void AddNestedResult(RenderingResult nestedResult)
        {
            if (nestedResult.ReportComponentType == ReportComponentType.Report)
            {
                m_NestedReportResults.Add(nestedResult.ReportId.Value, nestedResult);
            }
            else if (nestedResult.ReportComponentType == ReportComponentType.ReportElement)
            {
                if (!m_NestedReportElementResults.ContainsKey(nestedResult.ReportElementId.Value))
                { m_NestedReportElementResults.Add(nestedResult.ReportElementId.Value, new List<RenderingResult>()); }

                m_NestedReportElementResults[nestedResult.ReportElementId.Value].Add(nestedResult);
            }
            else
            { throw new InvalidOperationException("Model results must be root-level results."); }
        }

        internal void SetErrorState(RenderingResultType resultType, string errorMessage)
        {
            m_IsValid = false;
            m_ResultType = resultType;
            m_ErrorMessage = errorMessage;
        }

        internal void SetErrorState(RenderingResult nestedResult)
        {
            m_IsValid = false;
            m_ResultType = RenderingResultType.NestedErrorOccurred;
            m_ErrorMessage = NestedErrorMessage;

            AddNestedResult(nestedResult);
        }

        internal void SetModelInitializedState()
        {
            m_IsValid = true;
            m_ResultType = RenderingResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetInitializedState()
        {
            m_IsValid = true;
            m_ResultType = RenderingResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetModelValidatedState()
        {
            m_IsValid = true;
            m_ResultType = RenderingResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetValidatedState(RenderingResult nestedResult)
        {
            m_IsValid = nestedResult.m_IsValid;
            m_ResultType = nestedResult.m_ResultType;
            m_ErrorMessage = nestedResult.m_ErrorMessage;

            AddNestedResult(nestedResult);
        }

        internal void SetValidatedState()
        {
            m_IsValid = true;
            m_ResultType = RenderingResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetModelRenderedState()
        {
            m_IsValid = true;
            m_ResultType = RenderingResultType.Ok;
            m_ErrorMessage = string.Empty;
        }

        internal void SetRenderedState(RenderingResult nestedResult)
        {
            m_IsValid = nestedResult.m_IsValid;
            m_ResultType = nestedResult.m_ResultType;
            m_ErrorMessage = nestedResult.m_ErrorMessage;

            AddNestedResult(nestedResult);
        }

        internal void SetRenderedState()
        {
            m_IsValid = true;
            m_ResultType = RenderingResultType.Ok;
            m_ErrorMessage = string.Empty;
        }
    }
}