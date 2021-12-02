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
    public struct OrganizationId : IOrganization, ISiteActor, IGuidId, IConvertible
    {
        #region Static Members

        public const SiteActorType ActorType = IOrganizationUtils.ActorType;
        public static readonly Guid EmptyOrganizationGuid = IOrganizationUtils.EmptyOrganizationGuid;

        public static readonly OrganizationId DefaultId = new OrganizationId(EmptyOrganizationGuid);

        #endregion

        #region Members

        private Guid m_OrganizationGuid;

        #endregion

        #region Constructors

        public OrganizationId(Guid organizationGuid)
        {
            m_OrganizationGuid = organizationGuid;
        }

        #endregion

        #region Properties

        public Guid OrganizationGuid
        { get { return m_OrganizationGuid; } }

        #endregion

        #region Overrides & Overloads

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is IOrganization))
            { return false; }

            var objAsOrganization = (IOrganization)obj;

            if (objAsOrganization.OrganizationGuid != m_OrganizationGuid)
            { return false; }
            return true;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(IOrganizationUtils.OrganizationGuid_Prefix, OrganizationGuid);
            return item1;
        }

        public static OrganizationId NewId()
        { return new OrganizationId(Guid.NewGuid()); }

        public static bool operator ==(OrganizationId a, OrganizationId b)
        { return a.Equals(b); }

        public static bool operator !=(OrganizationId a, OrganizationId b)
        { return !(a == b); }

        #endregion

        #region ISiteActor Implementation

        SiteActorType ISiteActor.ActorType
        {
            get { return ActorType; }
        }

        Guid ISiteActor.ActorGuid
        {
            get { return OrganizationGuid; }
        }

        #endregion

        #region IGuidId Implementation

        Guid IGuidId.InnerGuid
        {
            get { return OrganizationGuid; }
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
        { return this.OrganizationGuid; }

        public ushort ToUInt16(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public uint ToUInt32(IFormatProvider provider)
        { throw new NotImplementedException(); }

        public ulong ToUInt64(IFormatProvider provider)
        { throw new NotImplementedException(); }

        #endregion
    }
}