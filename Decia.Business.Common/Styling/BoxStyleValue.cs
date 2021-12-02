using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Styling
{
    public struct BoxStyleValue<T>
    {
        public const string LeftName = "Left";
        public const string TopName = "Top";
        public const string RightName = "Right";
        public const string BottomName = "Bottom";

        private T m_Left;
        private T m_Top;
        private T m_Right;
        private T m_Bottom;

        public BoxStyleValue(T value)
            : this(value, value, value, value)
        { }

        public BoxStyleValue(T left, T top, T right, T bottom)
        {
            m_Left = left;
            m_Top = top;
            m_Right = right;
            m_Bottom = bottom;
        }

        public T Left
        {
            get { return m_Left; }
            set { m_Left = value; }
        }

        public T Top
        {
            get { return m_Top; }
            set { m_Top = value; }
        }

        public T Right
        {
            get { return m_Right; }
            set { m_Right = value; }
        }

        public T Bottom
        {
            get { return m_Bottom; }
            set { m_Bottom = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            BoxStyleValue<T> otherKey = (BoxStyleValue<T>)obj;
            bool areEqual = ((Left.Equals(otherKey.Left)) && (Top.Equals(otherKey.Top)) && (Right.Equals(otherKey.Right)) && (Bottom.Equals(otherKey.Bottom)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string left = TypedIdUtils.ConvertToString(LeftName, Left);
            string top = TypedIdUtils.ConvertToString(TopName, Top);
            string right = TypedIdUtils.ConvertToString(RightName, Right);
            string bottom = TypedIdUtils.ConvertToString(BottomName, Bottom);
            string value = string.Format(ConversionUtils.FourItemListFormat, left, top, right, bottom);
            return value;
        }

        public static bool operator ==(BoxStyleValue<T> a, BoxStyleValue<T> b)
        { return a.Equals(b); }

        public static bool operator !=(BoxStyleValue<T> a, BoxStyleValue<T> b)
        { return !(a == b); }

        public string SaveAsString()
        {
            Dictionary<string, T> values = new Dictionary<string, T>();
            values.Add(LeftName, Left);
            values.Add(TopName, Top);
            values.Add(RightName, Right);
            values.Add(BottomName, Bottom);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromString(string savedValue)
        {
            IDictionary<string, T> values = savedValue.ConvertToTypedDictionary<string, T>();

            m_Left = values[LeftName];
            m_Top = values[TopName];
            m_Right = values[RightName];
            m_Bottom = values[BottomName];
        }

        public static void ReSetValuesWithinBounds<X>(BoxStyleValue<X> current, X min, X max, Func<X, bool> isValidFunc, ref BoxStyleValue<X?> stored)
            where X : struct, IComparable
        {
            var currLeft = current.Left;
            var currTop = current.Top;
            var currRight = current.Right;
            var currBottom = current.Bottom;

            if (!isValidFunc(currLeft))
            {
                if (currLeft.CompareTo(min) == -1)
                { currLeft = min; }
                else
                { currLeft = max; }

                stored = new BoxStyleValue<X?>(currLeft, stored.Top, stored.Right, stored.Bottom);
            }

            if (!isValidFunc(currTop))
            {
                if (currTop.CompareTo(min) == -1)
                { currTop = min; }
                else
                { currTop = max; }

                stored = new BoxStyleValue<X?>(stored.Left, currTop, stored.Right, stored.Bottom);
            }

            if (!isValidFunc(currRight))
            {
                if (currRight.CompareTo(min) == -1)
                { currRight = min; }
                else
                { currRight = max; }

                stored = new BoxStyleValue<X?>(stored.Left, stored.Top, currRight, stored.Bottom);
            }

            if (!isValidFunc(currBottom))
            {
                if (currBottom.CompareTo(min) == -1)
                { currBottom = min; }
                else
                { currBottom = max; }

                stored = new BoxStyleValue<X?>(stored.Left, stored.Top, stored.Right, currBottom);
            }
        }
    }
}