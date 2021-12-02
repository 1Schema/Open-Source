using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Structure;
using Decia.Business.Common.Time;

namespace Decia.Business.Common.Modeling
{
    public struct ModelObjectReference : IDimensionalModelObject, IConvertible, IComparable, ITextPersistable<ModelObjectReference>
    {
        public static readonly IEqualityComparer<ModelObjectReference> DefaultComparer = new DefaultModelObjectReferenceComparer();
        public static readonly IEqualityComparer<ModelObjectReference> DimensionalComparer = new DimensionalModelObjectReferenceComparer();

        public const ModelObjectType GlobalType = ModelObjectType.GlobalType;
        public const ModelObjectType GlobalInstance = ModelObjectType.GlobalInstance;
        public const int GlobalTypeNumber = 0;
        public static readonly Guid GlobalInstanceGuid = Guid.Empty;
        public const int TimeTypeNumber = 1;

        public const int MinimumAlternateDimensionNumber = 1;
        public static readonly Nullable<int> DefaultDimensionNumber = null;
        public const Type DefaultReferenceEnum = null;

        public static ModelObjectReference? NullReference { get { return (ModelObjectReference?)null; } }
        public static readonly ModelObjectReference EmptyReference = new ModelObjectReference(ModelObjectType.None, 0);
        public static readonly ModelObjectReference GlobalTypeReference = new ModelObjectReference(GlobalType, GlobalTypeNumber);

        private ModelObjectType m_ObjectType;
        private Guid m_ObjectId;
        private Nullable<int> m_AlternateDimensionNumber;
        private Type m_ReferenceEnum;
        private int? m_VariableTemplateNumber;

        public ModelObjectReference(TimeDimensionType timeDimensionType)
            : this(ModelObjectType.TimeType, TimeTypeNumber, (int)timeDimensionType, typeof(TimeDimensionType))
        {
            TimeDimensionTypeUtils.AssertTimeDimensionTypeIsDirectlyUsable(timeDimensionType);
        }

        public ModelObjectReference(ModelObjectType objectType, int objectId)
            : this(objectType, objectId.ConvertIntToGuid())
        { }

        public ModelObjectReference(ModelObjectType objectType, Guid objectId)
            : this(objectType, objectId, DefaultDimensionNumber)
        { }

        public ModelObjectReference(ModelObjectType objectType, int objectId, Type referenceEnum)
            : this(objectType, objectId.ConvertIntToGuid(), referenceEnum)
        { }

        public ModelObjectReference(ModelObjectType objectType, Guid objectId, Type referenceEnum)
            : this(objectType, objectId, DefaultDimensionNumber, referenceEnum)
        { }

        public ModelObjectReference(ModelObjectType objectType, int objectId, Nullable<int> alternateDimensionNumber)
            : this(objectType, objectId.ConvertIntToGuid(), alternateDimensionNumber)
        { }

        public ModelObjectReference(ModelObjectType objectType, Guid objectId, Nullable<int> alternateDimensionNumber)
            : this(objectType, objectId, alternateDimensionNumber, DefaultReferenceEnum)
        { }

        public ModelObjectReference(ModelObjectType objectType, int objectId, Nullable<int> alternateDimensionNumber, Type referenceEnum)
            : this(objectType, objectId.ConvertIntToGuid(), alternateDimensionNumber, referenceEnum)
        { }

        public ModelObjectReference(ModelObjectType objectType, Guid objectId, Nullable<int> alternateDimensionNumber, Type referenceEnum)
        {
            if (alternateDimensionNumber.HasValue)
            {
                if (alternateDimensionNumber.Value < MinimumAlternateDimensionNumber)
                { throw new InvalidOperationException("The Alternate Dimension Number specified is invalid."); }
                if (!(objectType.IsDimensionableType() || objectType.IsDimensionableInstance()))
                { throw new InvalidOperationException("Only Time or Entity references are allowed to have an alternate dimension number."); }
            }

            m_ObjectType = objectType;
            m_ObjectId = objectId;
            m_AlternateDimensionNumber = alternateDimensionNumber;
            m_ReferenceEnum = referenceEnum;
            m_VariableTemplateNumber = null;
        }

        public ModelObjectReference(ModelObjectReference otherRef, Nullable<int> alternateDimensionNumber)
            : this(otherRef.m_ObjectType, otherRef.m_ObjectId, alternateDimensionNumber, otherRef.m_ReferenceEnum)
        { }

        public ModelObjectType ModelObjectType { get { return m_ObjectType; } }
        public Guid ModelObjectId { get { return m_ObjectId; } }
        public bool IdIsInt { get { return ModelObjectId.IsGuidStandardInt(); } }
        [ScriptIgnore]
        public int ModelObjectIdAsInt { get { return ModelObjectId.ConvertGuidToInt(); } }
        public int? ModelObjectIdAsInt_Nullable { get { return IdIsInt ? ModelObjectIdAsInt : (Nullable<int>)null; } }
        public Nullable<int> AlternateDimensionNumber { get { return m_AlternateDimensionNumber; } }
        public int NonNullAlternateDimensionNumber { get { return m_AlternateDimensionNumber.HasValue ? m_AlternateDimensionNumber.Value : MinimumAlternateDimensionNumber; } }
        public string ComplexId { get { return this.ToString(); } }
        public string ReferenceEnumValue { get { return (m_ReferenceEnum != null) ? Enum.GetName(m_ReferenceEnum, ModelObjectIdAsInt) : string.Empty; } }
        public Type ReferenceEnumType { get { return m_ReferenceEnum; } }

        public int? VariableTemplateNumber
        {
            get { return m_VariableTemplateNumber; }
            set { m_VariableTemplateNumber = value; }
        }

        public string HexBasedName
        {
            get
            {
                var prefix = this.ModelObjectType.ToString().ToLower()[0];
                var postfix = (this.IdIsInt) ? this.ModelObjectIdAsInt.ToString("X") : this.ModelObjectId.ToString("X");
                return prefix + postfix;
            }
        }

        #region TimeType-specific Members

        [ScriptIgnore]
        public Nullable<TimeDimensionType> TimeDimensionType
        {
            get
            {
                if (m_ObjectType != ModelObjectType.TimeType)
                { throw new InvalidOperationException("Cannot convert ModelObjectReference that is not of \"TimeType\" to TimeDimensionType."); }
                if (!m_AlternateDimensionNumber.HasValue)
                { return null; }
                return (TimeDimensionType)m_AlternateDimensionNumber.Value;
            }
        }

        [ScriptIgnore]
        public TimeDimensionType NonNullTimeDimensionType { get { return TimeDimensionType.Value; } }

        #endregion

        public ModelObjectReference ToAlternateDimension(int? alternateDimensionNumber)
        {
            if ((this.ModelObjectType != ModelObjectType.EntityType) && (this.ModelObjectType != ModelObjectType.EntityInstance)
                && (this.ModelObjectType != ModelObjectType.VariableTemplate) && (this.ModelObjectType != ModelObjectType.VariableInstance))
            {
                var defaultRef = new ModelObjectReference(this, this.AlternateDimensionNumber);
                return defaultRef;
            }

            var newRef = new ModelObjectReference(this, alternateDimensionNumber);
            return newRef;
        }

        public StructuralDimension ToStructuralDimension()
        {
            if (this.ModelObjectType != ModelObjectType.EntityType)
            { throw new InvalidOperationException("StructuralDimensions can only be generated from EntityTypes."); }

            var structuralDimension = new StructuralDimension(this.ModelObjectIdAsInt, this.NonNullAlternateDimensionNumber, StructuralDimensionType.NotSet, null);
            return structuralDimension;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }
            if (!(obj is ModelObjectReference))
            { return false; }

            ModelObjectReference other = (ModelObjectReference)obj;

            DefaultModelObjectReferenceComparer comparer = new DefaultModelObjectReferenceComparer();
            return comparer.Equals(this, other);
        }

        public override int GetHashCode()
        {
            DefaultModelObjectReferenceComparer comparer = new DefaultModelObjectReferenceComparer();
            return comparer.GetHashCode(this);
        }

        public override string ToString()
        {
            DefaultModelObjectReferenceComparer comparer = new DefaultModelObjectReferenceComparer();
            return comparer.ToString(this);
        }

        public static bool operator ==(Nullable<ModelObjectReference> a, object b)
        {
            if ((!a.HasValue) && (b == null))
            { return true; }
            if (!a.HasValue)
            { return false; }
            return a.Equals(b);
        }

        public static bool operator !=(Nullable<ModelObjectReference> a, object b)
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

            ModelObjectReference otherKey = (ModelObjectReference)obj;
            bool isInt = this.IdIsInt && otherKey.IdIsInt;

            IComparable thisPart1 = (this.ModelObjectType as IComparable);
            IComparable thisPart2 = !isInt ? (this.ModelObjectId as IComparable) : (this.ModelObjectIdAsInt as IComparable);
            IComparable thisPart3 = ((this.AlternateDimensionNumber.HasValue ? this.AlternateDimensionNumber.Value : 0) as IComparable);
            IComparable otherPart1 = (otherKey.ModelObjectType as IComparable);
            IComparable otherPart2 = !isInt ? (otherKey.ModelObjectId as IComparable) : (otherKey.ModelObjectIdAsInt as IComparable);
            IComparable otherPart3 = ((otherKey.AlternateDimensionNumber.HasValue ? otherKey.AlternateDimensionNumber.Value : 0) as IComparable);

            if (thisPart1.CompareTo(otherPart1) != 0)
            { return thisPart1.CompareTo(otherPart1); }
            if (thisPart2.CompareTo(otherPart2) != 0)
            { return thisPart2.CompareTo(otherPart2); }
            return thisPart3.CompareTo(otherPart3);
        }

        #endregion

        #region ITextPersistable<ModelObjectReference> Implementation

        public string SaveAsText()
        {
            var strValues = new List<string>();

            strValues.Add(((int)m_ObjectType).ToString());
            strValues.Add(m_ObjectId.ToString());
            if (m_AlternateDimensionNumber.HasValue)
            { strValues.Add(m_AlternateDimensionNumber.Value.ToString()); }

            var text = strValues.ConvertToCollectionAsString();
            return text;
        }

        public bool TryLoadFromText(string text, out ModelObjectReference newValue)
        {
            var strValues = text.ConvertToTypedCollection<string>();

            if (strValues.Count < 2)
            {
                newValue = ModelObjectReference.GlobalTypeReference;
                return false;
            }

            var objectType = (ModelObjectType)int.Parse(strValues[0]);
            var objectId = new Guid(strValues[1]);
            var altDimNumber = (strValues.Count > 2) ? (Nullable<int>)int.Parse(strValues[2]) : (Nullable<int>)null;

            newValue = new ModelObjectReference(objectType, objectId, altDimNumber);
            return true;
        }

        public ModelObjectReference LoadFromText(string text)
        {
            ModelObjectReference newValue;
            bool success = TryLoadFromText(text, out newValue);

            if (!success)
            { throw new InvalidOperationException("Attempt to Load value from text failed."); }

            return newValue;
        }

        #endregion
    }
}