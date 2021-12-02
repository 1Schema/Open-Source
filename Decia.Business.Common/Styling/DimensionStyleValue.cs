using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Styling
{
    public struct DimensionStyleValue<T>
    {
        public const string LesserSideName = "LesserSide";
        public const string GreaterSideName = "GreaterSide";

        private T m_LesserSide;
        private T m_GreaterSide;

        public DimensionStyleValue(T value)
            : this(value, value)
        { }

        public DimensionStyleValue(T lesserSide, T greaterSide)
        {
            m_LesserSide = lesserSide;
            m_GreaterSide = greaterSide;
        }

        public T LesserSide
        {
            get { return m_LesserSide; }
            set { m_LesserSide = value; }
        }

        public T GreaterSide
        {
            get { return m_GreaterSide; }
            set { m_GreaterSide = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            DimensionStyleValue<T> otherKey = (DimensionStyleValue<T>)obj;
            bool areEqual = ((LesserSide.Equals(otherKey.LesserSide)) && (GreaterSide.Equals(otherKey.GreaterSide)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string lesserSide = TypedIdUtils.ConvertToString(LesserSideName, LesserSide);
            string greaterSide = TypedIdUtils.ConvertToString(GreaterSideName, GreaterSide);
            string value = string.Format(ConversionUtils.TwoItemListFormat, lesserSide, greaterSide);
            return value;
        }

        public static bool operator ==(DimensionStyleValue<T> a, DimensionStyleValue<T> b)
        { return a.Equals(b); }

        public static bool operator !=(DimensionStyleValue<T> a, DimensionStyleValue<T> b)
        { return !(a == b); }

        public string SaveAsString()
        {
            Dictionary<string, T> values = new Dictionary<string, T>();
            values.Add(LesserSideName, LesserSide);
            values.Add(GreaterSideName, GreaterSide);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromString(string savedValue)
        {
            IDictionary<string, T> values = savedValue.ConvertToTypedDictionary<string, T>();

            m_LesserSide = values[LesserSideName];
            m_GreaterSide = values[GreaterSideName];
        }

        public static void ReSetValuesWithinBounds<X>(DimensionStyleValue<X> current, X min, X max, Func<X, bool> isValidFunc, ref DimensionStyleValue<X?> stored)
            where X : struct, IComparable
        {
            var currLesser = current.LesserSide;
            var currGreater = current.GreaterSide;

            if (!isValidFunc(currLesser))
            {
                if (currLesser.CompareTo(min) == -1)
                { currLesser = min; }
                else
                { currLesser = max; }

                stored = new DimensionStyleValue<X?>(currLesser, stored.GreaterSide);
            }

            if (!isValidFunc(currGreater))
            {
                if (currGreater.CompareTo(min) == -1)
                { currGreater = min; }
                else
                { currGreater = max; }

                stored = new DimensionStyleValue<X?>(stored.LesserSide, currGreater);
            }
        }
    }
}