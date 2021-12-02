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
    public struct SiteActorId : ISiteActor, IConvertible
    {
        #region Static Members

        public const SiteActorType EmptyActorType = ISiteActorUtils.EmptyActorType;
        public static readonly Guid EmptyActorGuid = ISiteActorUtils.EmptyActorGuid;

        public static readonly SiteActorId DefaultId = new SiteActorId(EmptyActorType, EmptyActorGuid);

        #endregion

        private SiteActorType m_ActorType;
        private Guid m_ActorGuid;

        public SiteActorId(IUser user)
            : this(user.ActorType, user.ActorGuid)
        { }

        public SiteActorId(IWorkgroup workgroup)
            : this(workgroup.ActorType, workgroup.ActorGuid)
        { }

        public SiteActorId(IOrganization organization)
            : this(organization.ActorType, organization.ActorGuid)
        { }

        public SiteActorId(SiteActorType actorType, Guid actorGuid)
        {
            m_ActorType = actorType;
            m_ActorGuid = actorGuid;
        }

        public SiteActorType ActorType
        { get { return m_ActorType; } }

        public Guid ActorGuid
        { get { return m_ActorGuid; } }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            var objAsSiteActor = (obj as ISiteActor);

            if (objAsSiteActor == null)
            { return false; }

            if (objAsSiteActor.ActorType != m_ActorType)
            { return false; }
            if (objAsSiteActor.ActorGuid != m_ActorGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(ISiteActorUtils.ActorType_Prefix, ActorType);
            string item2 = TypedIdUtils.StructToString(ISiteActorUtils.ActorGuid_Prefix, ActorGuid);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(SiteActorId a, SiteActorId b)
        { return a.Equals(b); }

        public static bool operator !=(SiteActorId a, SiteActorId b)
        { return !(a == b); }

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
    }
}