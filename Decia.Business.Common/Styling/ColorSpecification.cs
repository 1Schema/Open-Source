using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Styling
{
    public struct ColorSpecification
    {
        public const string AlphaName = "Alpha";
        public const string RedName = "Red";
        public const string GreenName = "Green";
        public const string BlueName = "Blue";

        private int m_Alpha;
        private int m_Red;
        private int m_Green;
        private int m_Blue;

        public ColorSpecification(Color color)
            : this(color.A, color.R, color.G, color.B)
        { }

        public ColorSpecification(int alpha, int red, int green, int blue)
        {
            m_Alpha = alpha;
            m_Red = red;
            m_Green = green;
            m_Blue = blue;
        }

        public bool IsValid
        {
            get { return ((m_Alpha >= 0) && (m_Red >= 0) && (m_Green >= 0) && (m_Blue >= 0)); }
        }

        public int Alpha
        {
            get { return m_Alpha; }
            set { m_Alpha = value; }
        }

        public int Red
        {
            get { return m_Red; }
            set { m_Red = value; }
        }

        public int Green
        {
            get { return m_Green; }
            set { m_Green = value; }
        }

        public int Blue
        {
            get { return m_Blue; }
            set { m_Blue = value; }
        }

        public Color Color
        {
            get { return Color.FromArgb(Alpha, Red, Green, Blue); }
        }

        public Color OpposingColor
        {
            get
            {
                var rgb = 0;
                var darknessFactor = (1.0 - (((.3 * Red) + (.6 * Green) + (.1 * Blue)) / 255.0));
                return (darknessFactor < .5) ? Color.Black : Color.White;
            }
        }

        public ColorSpecification OpposingColorSpec
        {
            get { return new ColorSpecification(OpposingColor); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            ColorSpecification otherKey = (ColorSpecification)obj;
            bool areEqual = ((Alpha.Equals(otherKey.Alpha)) && (Red.Equals(otherKey.Red)) && (Green.Equals(otherKey.Green)) && (Blue.Equals(otherKey.Blue)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string alpha = TypedIdUtils.ConvertToString(AlphaName, Alpha);
            string red = TypedIdUtils.ConvertToString(RedName, Red);
            string green = TypedIdUtils.ConvertToString(GreenName, Green);
            string blue = TypedIdUtils.ConvertToString(BlueName, Blue);
            string value = string.Format(ConversionUtils.FourItemListFormat, alpha, red, green, blue);
            return value;
        }

        public static bool operator ==(ColorSpecification a, ColorSpecification b)
        { return a.Equals(b); }

        public static bool operator !=(ColorSpecification a, ColorSpecification b)
        { return !(a == b); }

        public string SaveAsString()
        {
            Dictionary<string, int> values = new Dictionary<string, int>();
            values.Add(AlphaName, Alpha);
            values.Add(RedName, Red);
            values.Add(GreenName, Green);
            values.Add(BlueName, Blue);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromString(string savedValue)
        {
            IDictionary<string, int> values = savedValue.ConvertToTypedDictionary<string, int>();

            m_Alpha = values[AlphaName];
            m_Red = values[RedName];
            m_Green = values[GreenName];
            m_Blue = values[BlueName];
        }

        public static string SaveAsString(Nullable<ColorSpecification> colorSpec)
        {
            if (!colorSpec.HasValue)
            { return null; }

            return colorSpec.Value.SaveAsString();
        }

        public static void LoadFromString(string savedValue, out Nullable<ColorSpecification> colorSpec)
        {
            if (string.IsNullOrWhiteSpace(savedValue))
            {
                colorSpec = null;
            }
            else
            {
                ColorSpecification tempColor = new ColorSpecification(1, 1, 1, 1);
                tempColor.LoadFromString(savedValue);
                colorSpec = tempColor;
            }
        }
    }
}