using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using SDT = Decia.Business.Common.Structure.StructuralDimensionType;

namespace Decia.Business.Common.Structure
{
    public struct StructuralCoordinate : IConvertible, IComparable
    {
        public const int DefaultEntityDimensionNumber = StructuralDimension.DefaultEntityDimensionNumber;
        public const SDT DefaultDimensionType = StructuralDimension.DefaultDimensionType;
        public const bool DefaultUsesTimeDimension = StructuralDimension.DefaultUsesTimeDimension;
        public const Type DefaultEntityTypeEnum = StructuralDimension.DefaultEntityTypeEnum;
        public const Type DefaultEntityInstanceEnum = StructuralDimension.DefaultEntityInstanceEnum;

        public static readonly Guid Null_Value = Guid.Empty;

        private int m_EntityTypeNumber;
        private int m_EntityDimensionNumber;
        private Guid m_EntityInstanceGuid;
        private StructuralDimensionType m_DimensionType;
        private bool m_UsesTimeDimension;
        private Type m_EntityTypeEnum;
        private Type m_EntityInstanceEnum;

        public StructuralCoordinate(int entityTypeNumber, Guid entityInstanceGuid, StructuralDimensionType dimensionType)
            : this(entityTypeNumber, DefaultEntityDimensionNumber, entityInstanceGuid, dimensionType)
        { }

        public StructuralCoordinate(int entityTypeNumber, int entityDimensionNumber, Guid entityInstanceGuid, StructuralDimensionType dimensionType)
            : this(entityTypeNumber, entityDimensionNumber, entityInstanceGuid, dimensionType, DefaultUsesTimeDimension, DefaultEntityTypeEnum, DefaultEntityInstanceEnum)
        { }

        public StructuralCoordinate(int entityTypeNumber, int entityDimensionNumber, Guid entityInstanceGuid, StructuralDimensionType dimensionType, bool usesTimeDimension)
            : this(entityTypeNumber, entityDimensionNumber, entityInstanceGuid, dimensionType, usesTimeDimension, DefaultEntityTypeEnum, DefaultEntityInstanceEnum)
        { }

        public StructuralCoordinate(int entityTypeNumber, int entityDimensionNumber, Guid entityInstanceGuid, StructuralDimensionType dimensionType, Type entityTypeEnum, Type entityInstanceEnum)
            : this(entityTypeNumber, entityDimensionNumber, entityInstanceGuid, dimensionType, DefaultUsesTimeDimension, entityTypeEnum, entityInstanceEnum)
        { }

        public StructuralCoordinate(int entityTypeNumber, int entityDimensionNumber, Guid entityInstanceGuid, StructuralDimensionType dimensionType, bool usesTimeDimension, Type entityTypeEnum, Type entityInstanceEnum)
        {
            m_EntityTypeNumber = entityTypeNumber;
            m_EntityDimensionNumber = entityDimensionNumber;
            m_EntityInstanceGuid = entityInstanceGuid;
            m_DimensionType = dimensionType;
            m_UsesTimeDimension = usesTimeDimension;
            m_EntityTypeEnum = entityTypeEnum;
            m_EntityInstanceEnum = entityInstanceEnum;
        }

        public StructuralCoordinate(StructuralCoordinate originalCoordinate, Nullable<int> alternateDimensionNumber)
            : this(originalCoordinate.m_EntityTypeNumber, (alternateDimensionNumber.HasValue) ? alternateDimensionNumber.Value : originalCoordinate.m_EntityDimensionNumber,
            originalCoordinate.m_EntityInstanceGuid, originalCoordinate.m_DimensionType, originalCoordinate.m_UsesTimeDimension,
            originalCoordinate.m_EntityTypeEnum, originalCoordinate.m_EntityInstanceEnum)
        { }

        public StructuralDimensionType DimensionType
        {
            get { return m_DimensionType; }
        }

        public bool UsesTimeDimension
        {
            get { return m_UsesTimeDimension; }
        }

        public bool IsNull
        {
            get { return (m_EntityInstanceGuid == Null_Value); }
        }

        public string EntityTypeEnumValue
        {
            get { return Enum.GetName(m_EntityTypeEnum, m_EntityTypeNumber); }
        }

        public string EntityInstanceEnumValue
        {
            get
            {
                var entityInstanceInt = m_EntityInstanceGuid.ConvertGuidToInt(true);
                return Enum.GetName(m_EntityInstanceEnum, entityInstanceInt);
            }
        }

        public int CoordinateId
        {
            get { return this.GetHashCode(); }
        }

        public StructuralDimension Dimension
        {
            get { return new StructuralDimension(m_EntityTypeNumber, m_EntityDimensionNumber, m_DimensionType, null, m_UsesTimeDimension, m_EntityTypeEnum, m_EntityInstanceEnum); }
        }

        public int DimensionId
        {
            get { return Dimension.GetHashCode(); }
        }

        public int EntityTypeNumber
        {
            get { return m_EntityTypeNumber; }
        }

        public int EntityDimensionNumber
        {
            get { return m_EntityDimensionNumber; }
        }

        public Guid EntityInstanceGuid
        {
            get { return m_EntityInstanceGuid; }
        }

        public ModelObjectReference EntityTypeRef
        {
            get { return new ModelObjectReference(ModelObjectType.EntityType, m_EntityTypeNumber); }
        }

        public ModelObjectReference EntityInstanceRef
        {
            get { return new ModelObjectReference(ModelObjectType.EntityInstance, m_EntityInstanceGuid); }
        }

        public Type EntityTypeEnumType
        {
            get { return m_EntityTypeEnum; }
        }

        public Type EntityInstanceEnumType
        {
            get { return m_EntityInstanceEnum; }
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            StructuralCoordinate otherKey = (StructuralCoordinate)obj;
            bool areEqual = ((EntityTypeNumber.Equals(otherKey.EntityTypeNumber))
                && (EntityDimensionNumber.Equals(otherKey.EntityDimensionNumber))
                && (EntityInstanceGuid.Equals(otherKey.EntityInstanceGuid)));
            return areEqual;
        }

        public static readonly string EntityType_Prefix = KeyProcessingModeUtils.GetModalDebugText("Entity Type");
        public static readonly string EntityDimension_Prefix = KeyProcessingModeUtils.GetModalDebugText("Entity Dimension");
        public static readonly string EntityInstance_Prefix = KeyProcessingModeUtils.GetModalDebugText("Entity Instance");

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(EntityType_Prefix, EntityTypeNumber);
            string item2 = TypedIdUtils.StructToString(EntityDimension_Prefix, EntityDimensionNumber);
            string item3 = TypedIdUtils.StructToString(EntityInstance_Prefix, EntityInstanceGuid);

            string value = string.Format(ConversionUtils.ThreeItemListFormat, item1, item2, item3);
            return value;
        }

        public static bool operator ==(StructuralCoordinate a, StructuralCoordinate b)
        { return a.Equals(b); }

        public static bool operator !=(StructuralCoordinate a, StructuralCoordinate b)
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

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            if (obj == null)
            { return -1; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return -1; }

            StructuralCoordinate otherKey = (StructuralCoordinate)obj;

            int typeResult = EntityTypeNumber.CompareTo(otherKey.EntityTypeNumber);
            if (typeResult != 0)
            { return typeResult; }

            int dimensionResult = EntityDimensionNumber.CompareTo(otherKey.EntityDimensionNumber);
            if (dimensionResult != 0)
            { return dimensionResult; }

            int instanceResult = EntityInstanceGuid.CompareTo(otherKey.EntityInstanceGuid);
            return instanceResult;
        }

        #endregion
    }
}