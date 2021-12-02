using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.Outputs;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Presentation
{
    public struct PointT<T>
    {
        public const string XName = "X";
        public const string YName = "Y";

        private T m_X;
        private T m_Y;

        public PointT(T coordinate)
            : this(coordinate, coordinate)
        { }

        public PointT(T x, T y)
        {
            m_X = x;
            m_Y = y;
        }

        public T X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public T Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public T GetForDimension(Dimension dimension)
        {
            if (dimension == Dimension.X)
            { return X; }
            else if (dimension == Dimension.Y)
            { return Y; }
            else
            { throw new InvalidOperationException("Unsupported Dimension encountered."); }
        }

        public void SetForDimension(Dimension dimension, T value)
        {
            if (dimension == Dimension.X)
            { X = value; }
            else if (dimension == Dimension.Y)
            { Y = value; }
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

            PointT<T> otherKey = (PointT<T>)obj;
            bool areEqual = ((X.Equals(otherKey.X)) && (Y.Equals(otherKey.Y)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string x = TypedIdUtils.ConvertToString(XName, X);
            string y = TypedIdUtils.ConvertToString(YName, Y);
            string value = string.Format(ConversionUtils.TwoItemListFormat, x, y);
            return value;
        }

        public static bool operator ==(PointT<T> a, PointT<T> b)
        { return a.Equals(b); }

        public static bool operator !=(PointT<T> a, PointT<T> b)
        { return !(a == b); }

        public string SaveAsString()
        {
            Dictionary<string, T> values = new Dictionary<string, T>();
            values.Add(XName, X);
            values.Add(YName, Y);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromString(string savedValue)
        {
            if (string.IsNullOrWhiteSpace(savedValue))
            {
                m_X = default(T);
                m_Y = default(T);
            }
            else
            {
                var values = savedValue.ConvertToTypedDictionary<string, T>();

                m_X = values[XName];
                m_Y = values[YName];
            }
        }

        public static void ReSetValuesWithinBounds<X>(PointT<X> current, X min, X max, Func<X, bool> isValidFunc, ref PointT<X?> stored)
            where X : struct, IComparable
        {
            var currX = current.X;
            var currY = current.Y;

            if (!isValidFunc(currX))
            {
                if (currX.CompareTo(min) == -1)
                { currX = min; }
                else
                { currX = max; }

                stored = new PointT<X?>(currX, stored.Y);
            }

            if (!isValidFunc(currY))
            {
                if (currY.CompareTo(min) == -1)
                { currY = min; }
                else
                { currY = max; }

                stored = new PointT<X?>(stored.X, currY);
            }
        }
    }
}