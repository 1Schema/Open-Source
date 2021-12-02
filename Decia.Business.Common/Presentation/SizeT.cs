using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Presentation
{
    public struct SizeT<T>
    {
        public const string WidthName = "Width";
        public const string HeightName = "Height";

        private T m_Width;
        private T m_Height;

        public SizeT(T size)
            : this(size, size)
        { }

        public SizeT(T width, T height)
        {
            m_Width = width;
            m_Height = height;
        }

        public T Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public T Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public T GetForDimension(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return Width; }
            else if (dimension == Dimension.Y)
            { return Height; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public void SetForDimension(Dimension dimension, T value)
        {
            if (dimension == Dimension.X)
            { Width = value; }
            else if (dimension == Dimension.Y)
            { Height = value; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            SizeT<T> otherKey = (SizeT<T>)obj;
            bool areEqual = ((Width.Equals(otherKey.Width)) && (Height.Equals(otherKey.Height)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string width = TypedIdUtils.ConvertToString(WidthName, Width);
            string height = TypedIdUtils.ConvertToString(HeightName, Height);
            string value = string.Format(ConversionUtils.TwoItemListFormat, width, height);
            return value;
        }

        public static bool operator ==(SizeT<T> a, SizeT<T> b)
        { return a.Equals(b); }

        public static bool operator !=(SizeT<T> a, SizeT<T> b)
        { return !(a == b); }

        public string SaveAsString()
        {
            Dictionary<string, T> values = new Dictionary<string, T>();
            values.Add(WidthName, Width);
            values.Add(HeightName, Height);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromString(string savedValue)
        {
            if (string.IsNullOrWhiteSpace(savedValue))
            {
                m_Width = default(T);
                m_Height = default(T);
            }
            else
            {
                var values = savedValue.ConvertToTypedDictionary<string, T>();

                m_Width = values[WidthName];
                m_Height = values[HeightName];
            }
        }

        public static void ReSetValuesWithinBounds<X>(SizeT<X> current, X min, X max, Func<X, bool> isValidFunc, ref SizeT<X?> stored)
            where X : struct, IComparable
        {
            var currWidth = current.Width;
            var currHeight = current.Height;

            if (!isValidFunc(currWidth))
            {
                if (currWidth.CompareTo(min) == -1)
                { currWidth = min; }
                else
                { currWidth = max; }

                stored = new SizeT<X?>(currWidth, stored.Height);
            }

            if (!isValidFunc(currHeight))
            {
                if (currHeight.CompareTo(min) == -1)
                { currHeight = min; }
                else
                { currHeight = max; }

                stored = new SizeT<X?>(stored.Width, currHeight);
            }
        }
    }
}