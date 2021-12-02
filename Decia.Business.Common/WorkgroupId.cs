using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainDriver.CommonUtilities.Reflectors;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Permissions;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common
{
    public struct WorkgroupId : IWorkgroup, ISiteActor, IGuidId, IConvertible
    {
        #region Static Members

        public const SiteActorType ActorType = IWorkgroupUtils.ActorType;
        public static readonly Guid EmptyWorkgroupGuid = IWorkgroupUtils.EmptyWorkgroupGuid;

        public static readonly WorkgroupId DefaultId = new WorkgroupId(EmptyWorkgroupGuid);

        #endregion

        #region Members

        private Guid m_WorkgroupGuid;

        #endregion

        #region Constructors

        public WorkgroupId(Guid workgroupGuid)
        {
            m_WorkgroupGuid = workgroupGuid;
        }

        #endregion

        #region Properties

        public Guid WorkgroupGuid
        { get { return m_WorkgroupGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IWorkgroup))
            { return false; }

            var objAsWorkgroup = (IWorkgroup)obj;

            if (objAsWorkgroup.WorkgroupGuid != m_WorkgroupGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(IWorkgroupUtils.WorkgroupGuid_Prefix, WorkgroupGuid);
            return item1;
        }

        public static WorkgroupId NewId()
        { return new WorkgroupId(Guid.NewGuid()); }

        public static bool operator ==(WorkgroupId a, WorkgroupId b)
        { return a.Equals(b); }

        public static bool operator !=(WorkgroupId a, WorkgroupId b)
        { return !(a == b); }

        #endregion

        #region ISiteActor Implementation

        SiteActorType ISiteActor.ActorType
        {
            get { return ActorType; }
        }

        Guid ISiteActor.ActorGuid
        {
            get { return WorkgroupGuid; }
        }

        #endregion

        #region IGuidId Implementation

        Guid IGuidId.InnerGuid
        {
            get { return WorkgroupGuid; }
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
        { return this.WorkgroupGuid; }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion
    }
}