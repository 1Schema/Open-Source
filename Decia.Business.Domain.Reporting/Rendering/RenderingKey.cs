using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Domain.Reporting.Rendering
{
    public struct RenderingKey : IModelMember, IProjectMember_Revisionless, IConvertible, IComparable
    {
        public const string NullStructuralPointText = "<No SP>";
        public const string NullGroupingDimensionText = "<No GD>";
        public const string NullGroupNumberText = "<No GN>";

        public static readonly ProjectMemberComparer_Revisionless<RenderingKey> Comparer_Revisionless = new ProjectMemberComparer_Revisionless<RenderingKey>();

        private ReportElementId m_ReportElementId;
        private Nullable<StructuralPoint> m_StructuralPoint;
        private MultiTimePeriodKey m_TimeKey;
        private bool m_IsGroup;
        private Nullable<Dimension> m_GroupingDimension;
        private Nullable<int> m_GroupNumber;

        #region Constructors

        public RenderingKey(ReportElementId reportElementId, Nullable<StructuralPoint> structuralPoint, MultiTimePeriodKey timeKey)
        {
            m_ReportElementId = reportElementId;
            m_StructuralPoint = structuralPoint;
            m_TimeKey = timeKey;
            m_IsGroup = false;
            m_GroupingDimension = null;
            m_GroupNumber = null;
        }

        public RenderingKey(ReportElementId reportElementId, Nullable<StructuralPoint> structuralPoint, MultiTimePeriodKey timeKey, Nullable<Dimension> groupingDimension, int groupNumber)
        {
            m_ReportElementId = reportElementId;
            m_StructuralPoint = structuralPoint;
            m_TimeKey = timeKey;
            m_IsGroup = true;
            m_GroupingDimension = groupingDimension;
            m_GroupNumber = groupNumber;
        }

        #endregion

        #region Properties

        public ReportId ReportId
        { get { return m_ReportElementId.ReportId; } }

        public ReportElementId ReportElementId
        { get { return m_ReportElementId; } }

        public Guid ReportGuid
        { get { return ReportId.ReportGuid; } }

        public int ReportElementNumber
        { get { return m_ReportElementId.ReportElementNumber; } }

        public Nullable<StructuralPoint> StructuralPoint
        { get { return m_StructuralPoint; } }

        public MultiTimePeriodKey TimeKey
        { get { return m_TimeKey; } }

        public bool IsGroup
        { get { return m_IsGroup; } }

        public Nullable<Dimension> GroupingDimension
        { get { return m_GroupingDimension; } }

        public Nullable<int> GroupNumber
        { get { return m_GroupNumber; } }

        #endregion

        #region Method Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            RenderingKey otherKey = (RenderingKey)obj;

            if (otherKey.ReportElementId != ReportElementId)
            { return false; }
            if (otherKey.StructuralPoint != StructuralPoint)
            { return false; }
            if (otherKey.TimeKey != TimeKey)
            { return false; }
            if (otherKey.IsGroup != IsGroup)
            { return false; }
            if (otherKey.GroupingDimension != GroupingDimension)
            { return false; }
            if (otherKey.GroupNumber != GroupNumber)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = ReportElementId.ToString();
            string item2 = StructuralPoint.HasValue ? StructuralPoint.Value.ToString() : NullStructuralPointText;
            string item3 = TimeKey.ToString();
            string item4 = IsGroup.ToString();
            string item5 = GroupingDimension.HasValue ? ((int)GroupingDimension).ToString() : NullGroupingDimensionText;
            string item6 = GroupNumber.HasValue ? GroupNumber.ToString() : NullGroupNumberText;

            string value = string.Format(ConversionUtils.SixItemListFormat, item1, item2, item3, item4, item5, item6);
            return value;
        }

        public static bool operator ==(RenderingKey a, RenderingKey b)
        { return a.Equals(b); }

        public static bool operator !=(RenderingKey a, RenderingKey b)
        { return !(a == b); }

        #endregion

        #region IModelMember Implementation

        public Guid ProjectGuid
        {
            get { return m_ReportElementId.ProjectGuid; }
        }

        public bool IsRevisionSpecific
        {
            get { return m_ReportElementId.IsRevisionSpecific; }
        }

        public long? RevisionNumber
        {
            get { return m_ReportElementId.RevisionNumber; }
        }

        public long RevisionNumber_NonNull
        {
            get { return m_ReportElementId.RevisionNumber_NonNull; }
        }

        public bool IsInstance
        {
            get { return m_ReportElementId.IsInstance; }
        }

        public int ModelTemplateNumber
        {
            get { return m_ReportElementId.ModelTemplateNumber; }
        }

        public Guid? ModelInstanceGuid
        {
            get { return m_ReportElementId.ModelInstanceGuid; }
        }

        public ModelObjectReference ModelObjectRef
        {
            get { return m_ReportElementId.ModelObjectRef; }
        }

        public ModelObjectType ModelObjectType
        {
            get { return m_ReportElementId.ModelObjectType; }
        }

        public Guid ModelObjectId
        {
            get { return m_ReportElementId.ModelObjectId; }
        }

        public bool IdIsInt
        {
            get { return m_ReportElementId.IdIsInt; }
        }

        public int ModelObjectIdAsInt
        {
            get { return m_ReportElementId.ModelObjectIdAsInt; }
        }

        public string ComplexId
        {
            get { return m_ReportElementId.ComplexId; }
        }

        #endregion

        #region IProjectMember_Revisionless Implementation

        public bool Equals_Revisionless(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            RenderingKey otherKey = (RenderingKey)obj;

            if (!otherKey.ReportElementId.Equals_Revisionless(ReportElementId))
            { return false; }
            if (otherKey.StructuralPoint != StructuralPoint)
            { return false; }
            if (otherKey.TimeKey != TimeKey)
            { return false; }
            if (otherKey.IsGroup != IsGroup)
            { return false; }
            if (otherKey.GroupingDimension != GroupingDimension)
            { return false; }
            if (otherKey.GroupNumber != GroupNumber)
            { return false; }
            return true;
        }

        public int GetHashCode_Revisionless()
        { return this.ToString_Revisionless().GetHashCode(); }

        public string ToString_Revisionless()
        {
            string item1 = ReportElementId.ToString_Revisionless();
            string item2 = StructuralPoint.HasValue ? StructuralPoint.Value.ToString() : NullStructuralPointText;
            string item3 = TimeKey.ToString();
            string item4 = IsGroup.ToString();
            string item5 = GroupingDimension.HasValue ? ((int)GroupingDimension).ToString() : NullGroupingDimensionText;
            string item6 = GroupNumber.HasValue ? GroupNumber.ToString() : NullGroupNumberText;

            string value = string.Format(ConversionUtils.SixItemListFormat, item1, item2, item3, item4, item5, item6);
            return value;
        }

        #endregion

        #region IConvertible Implementation

        public TypeCode GetTypeCode()
        { return TypeCode.Object; }

        public bool ToBoolean(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public byte ToByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public char ToChar(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public DateTime ToDateTime(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public decimal ToDecimal(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public double ToDouble(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public short ToInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public int ToInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public long ToInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public sbyte ToSByte(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public float ToSingle(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public string ToString(IFormatProvider provider)
        { return this.ToString(); }

        public object ToType(Type conversionType, IFormatProvider provider)
        { return ToString(provider); }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            RenderingKey otherAsElementId = (RenderingKey)obj;
            return this.ReportElementNumber.CompareTo(otherAsElementId.ReportElementNumber);
        }

        #endregion
    }
}