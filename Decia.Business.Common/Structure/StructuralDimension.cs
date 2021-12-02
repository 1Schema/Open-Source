using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.TypedIds;
using SDT = Decia.Business.Common.Structure.StructuralDimensionType;
using SDTUtils = Decia.Business.Common.Structure.StructuralDimensionTypeUtils;

namespace Decia.Business.Common.Structure
{
    public struct StructuralDimension : IConvertible, IComparable
    {
        public const int DefaultEntityDimensionNumber = ModelObjectReference.MinimumAlternateDimensionNumber;
        public const SDT DefaultDimensionType = SDTUtils.DefaultDimensionType;
        public const bool DefaultUsesTimeDimension = false;
        public const Type DefaultEntityTypeEnum = null;
        public const Type DefaultEntityInstanceEnum = null;

        private int m_EntityTypeNumber;
        private int m_EntityDimensionNumber;
        private StructuralDimensionType m_DimensionType;
        private Nullable<ModelObjectReference> m_CorrespondingVariableRef;
        private bool m_UsesTimeDimension;
        private Type m_EntityTypeEnum;
        private Type m_EntityInstanceEnum;

        public StructuralDimension(int entityTypeNumber, StructuralDimensionType dimensionType, Nullable<ModelObjectReference> correspondingVariableRef)
            : this(entityTypeNumber, DefaultEntityDimensionNumber, dimensionType, correspondingVariableRef)
        { }

        public StructuralDimension(int entityTypeNumber, int entityDimensionNumber, StructuralDimensionType dimensionType, Nullable<ModelObjectReference> correspondingVariableRef)
            : this(entityTypeNumber, entityDimensionNumber, dimensionType, correspondingVariableRef, DefaultUsesTimeDimension)
        { }

        public StructuralDimension(int entityTypeNumber, int entityDimensionNumber, StructuralDimensionType dimensionType, Nullable<ModelObjectReference> correspondingVariableRef, bool usesTimeDimension)
            : this(entityTypeNumber, entityDimensionNumber, dimensionType, correspondingVariableRef, usesTimeDimension, DefaultEntityTypeEnum, DefaultEntityInstanceEnum)
        { }

        public StructuralDimension(int entityTypeNumber, int entityDimensionNumber, StructuralDimensionType dimensionType, Nullable<ModelObjectReference> correspondingVariableRef, Type entityTypeEnum, Type entityInstanceEnum)
            : this(entityTypeNumber, entityDimensionNumber, dimensionType, correspondingVariableRef, DefaultUsesTimeDimension, entityTypeEnum, entityInstanceEnum)
        { }

        public StructuralDimension(int entityTypeNumber, int entityDimensionNumber, StructuralDimensionType dimensionType, Nullable<ModelObjectReference> correspondingVariableRef, bool usesTimeDimension, Type entityTypeEnum, Type entityInstanceEnum)
        {
            m_EntityTypeNumber = entityTypeNumber;
            m_EntityDimensionNumber = entityDimensionNumber;
            m_DimensionType = dimensionType;
            m_CorrespondingVariableRef = correspondingVariableRef;
            m_UsesTimeDimension = usesTimeDimension;
            m_EntityTypeEnum = entityTypeEnum;
            m_EntityInstanceEnum = entityInstanceEnum;
        }

        public StructuralDimension(StructuralDimension originalDimension, Nullable<int> alternateDimensionNumber)
            : this(originalDimension.m_EntityTypeNumber, (alternateDimensionNumber.HasValue) ? alternateDimensionNumber.Value : originalDimension.m_EntityDimensionNumber,
           originalDimension.m_DimensionType, originalDimension.m_CorrespondingVariableRef, originalDimension.m_UsesTimeDimension,
            originalDimension.m_EntityTypeEnum, originalDimension.m_EntityInstanceEnum)
        { }

        public StructuralDimensionType DimensionType
        {
            get { return m_DimensionType; }
        }

        public Nullable<ModelObjectReference> CorrespondingVariableRef
        {
            get { return m_CorrespondingVariableRef; }
        }

        public bool UsesTimeDimension
        {
            get { return m_UsesTimeDimension; }
        }

        public string EntityTypeEnumValue
        {
            get { return Enum.GetName(m_EntityTypeEnum, m_EntityTypeNumber); }
        }

        public int DimensionId
        {
            get { return this.GetHashCode(); }
        }

        public int EntityTypeNumber
        {
            get { return m_EntityTypeNumber; }
        }

        public int EntityDimensionNumber
        {
            get { return m_EntityDimensionNumber; }
        }

        public ModelObjectReference EntityTypeRef
        {
            get { return new ModelObjectReference(ModelObjectType.EntityType, m_EntityTypeNumber); }
        }

        public ModelObjectReference EntityTypeRefWithDimNum
        {
            get { return new ModelObjectReference(ModelObjectType.EntityType, m_EntityTypeNumber, m_EntityDimensionNumber); }
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

            StructuralDimension otherKey = (StructuralDimension)obj;
            bool areEqual = ((EntityTypeNumber.Equals(otherKey.EntityTypeNumber))
                && (EntityDimensionNumber.Equals(otherKey.EntityDimensionNumber)));
            return areEqual;
        }

        public static readonly string EntityType_Prefix = KeyProcessingModeUtils.GetModalDebugText("Entity Type");
        public static readonly string EntityDimension_Prefix = KeyProcessingModeUtils.GetModalDebugText("Entity Dimension");

        public override string ToString()
        {
            string item1 = TypedIdUtils.StructToString(EntityType_Prefix, EntityTypeNumber);
            string item2 = TypedIdUtils.StructToString(EntityDimension_Prefix, EntityDimensionNumber);

            string value = string.Format(ConversionUtils.TwoItemListFormat, item1, item2);
            return value;
        }

        public static bool operator ==(StructuralDimension a, StructuralDimension b)
        { return a.Equals(b); }

        public static bool operator !=(StructuralDimension a, StructuralDimension b)
        { return !(a == b); }

        public StructuralDimension Merge(StructuralDimension otherDimension)
        {
            return Merge(this, otherDimension);
        }

        public static StructuralDimension Merge(StructuralDimension dimension, StructuralDimension otherDimension)
        {
            if (dimension.DimensionId != otherDimension.DimensionId)
            { throw new InvalidOperationException("The DimensionIds of the Dimensions to merge do not match."); }

            if (dimension.UsesTimeDimension)
            { return dimension; }
            if (otherDimension.UsesTimeDimension)
            { return otherDimension; }

            if (dimension.DimensionType.IsReferenceMember())
            { return dimension; }
            if (otherDimension.DimensionType.IsReferenceMember())
            { return otherDimension; }

            if (dimension.DimensionType.IsExistenceMember())
            { return dimension; }
            if (otherDimension.DimensionType.IsExistenceMember())
            { return otherDimension; }

            return dimension;
        }

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

            StructuralDimension otherKey = (StructuralDimension)obj;

            int typeResult = EntityTypeNumber.CompareTo(otherKey.EntityTypeNumber);
            if (typeResult != 0)
            { return typeResult; }

            int dimensionResult = EntityDimensionNumber.CompareTo(otherKey.EntityDimensionNumber);
            return dimensionResult;
        }

        #endregion
    }
}