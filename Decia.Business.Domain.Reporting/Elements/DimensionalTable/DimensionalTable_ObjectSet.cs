using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Outputs;

namespace Decia.Business.Domain.Reporting
{
    public class DimensionalTable_ObjectSet
    {
        #region Members

        private Dictionary<ReportElementId, CommonTitleBox> m_CommonTitleBoxes;
        private Dictionary<ReportElementId, VariableTitleBox> m_VariableTitleBoxes;
        private Dictionary<ReportElementId, VariableDataBox> m_VariableDataBoxes;
        private Dictionary<ReportElementId, StructuralTitleRange> m_StructuralTitleRanges;
        private Dictionary<ReportElementId, TimeTitleRange> m_TimeTitleRanges;
        private Dictionary<ReportElementId, VariableTitleRange> m_VariableTitleRanges;
        private Dictionary<ReportElementId, VariableDataRange> m_VariableDataRanges;

        #endregion

        #region Constructors

        public DimensionalTable_ObjectSet()
        {
            m_CommonTitleBoxes = new Dictionary<ReportElementId, CommonTitleBox>();
            m_VariableTitleBoxes = new Dictionary<ReportElementId, VariableTitleBox>();
            m_VariableDataBoxes = new Dictionary<ReportElementId, VariableDataBox>();
            m_StructuralTitleRanges = new Dictionary<ReportElementId, StructuralTitleRange>();
            m_TimeTitleRanges = new Dictionary<ReportElementId, TimeTitleRange>();
            m_VariableTitleRanges = new Dictionary<ReportElementId, VariableTitleRange>();
            m_VariableDataRanges = new Dictionary<ReportElementId, VariableDataRange>();
        }

        #endregion

        #region Properties

        public DimensionalTable DimensionalTable { get; set; }
        public Dimension StackingDimension { get { return DimensionalTable.StackingDimension; } }

        public TableHeader TableHeader { get; set; }
        public RowHeader RowHeader { get; set; }
        public ColumnHeader ColumnHeader { get; set; }
        public DataArea DataArea { get; set; }
        public CommonTitleContainer CommonTitleContainer { get; set; }
        public VariableTitleContainer VariableTitleContainer { get; set; }
        public VariableDataContainer VariableDataContainer { get; set; }

        public Dictionary<ReportElementId, CommonTitleBox> CommonTitleBoxes { get { return m_CommonTitleBoxes; } }
        public Dictionary<ReportElementId, VariableTitleBox> VariableTitleBoxes { get { return m_VariableTitleBoxes; } }
        public Dictionary<ReportElementId, VariableDataBox> VariableDataBoxes { get { return m_VariableDataBoxes; } }
        public Dictionary<ReportElementId, StructuralTitleRange> StructuralTitleRanges { get { return m_StructuralTitleRanges; } }
        public Dictionary<ReportElementId, TimeTitleRange> TimeTitleRanges { get { return m_TimeTitleRanges; } }
        public Dictionary<ReportElementId, VariableTitleRange> VariableTitleRanges { get { return m_VariableTitleRanges; } }
        public Dictionary<ReportElementId, VariableDataRange> VariableDataRanges { get { return m_VariableDataRanges; } }

        #endregion
    }
}