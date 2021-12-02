using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decia.Business.Common.Conversion;
using Decia.Business.Common.TypedIds;

namespace Decia.Business.Common.Styling
{
    public struct BoxPosition
    {
        public const string LeftName = "Left";
        public const string TopName = "Top";
        public const string WidthName = "Width";
        public const string HeightName = "Height";
        public const string RightName = "Right";
        public const string BottomName = "Bottom";

        private int m_Left;
        private int m_Top;
        private int m_Width;
        private int m_Height;

        public BoxPosition(int left, int top, int width, int height)
        {
            StylingAssertions.AssertDimensionsAreValid(width, height);

            m_Left = left;
            m_Top = top;
            m_Width = width;
            m_Height = height;
        }

        public int Left
        {
            get { return m_Left; }
            set { m_Left = value; }
        }

        public int Top
        {
            get { return m_Top; }
            set { m_Top = value; }
        }

        public int Width
        {
            get { return m_Width; }
            set
            {
                StylingAssertions.AssertDimensionsAreValid(value, m_Height);
                m_Width = value;
            }
        }

        public int Height
        {
            get { return m_Height; }
            set
            {
                StylingAssertions.AssertDimensionsAreValid(m_Width, value);
                m_Height = value;
            }
        }

        public int Right
        {
            get { return (Left + Width); }
        }

        public int Bottom
        {
            get { return (Top + Height); }
        }

        public int Excel_Left
        {
            get { return (Left + 1); }
        }

        public int Excel_Top
        {
            get { return (Top + 1); }
        }

        public int Excel_Right
        {
            get { return (Right); }
        }

        public int Excel_Bottom
        {
            get { return (Bottom); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            BoxPosition otherKey = (BoxPosition)obj;
            bool areEqual = ((Left.Equals(otherKey.Left)) && (Top.Equals(otherKey.Top)) && (Width.Equals(otherKey.Width)) && (Height.Equals(otherKey.Height)));
            return areEqual;
        }

        public override int GetHashCode()
        { return this.ToString().GetHashCode(); }

        public override string ToString()
        {
            string left = TypedIdUtils.ConvertToString(LeftName, Left);
            string top = TypedIdUtils.ConvertToString(TopName, Top);
            string right = TypedIdUtils.ConvertToString(WidthName, Width);
            string bottom = TypedIdUtils.ConvertToString(HeightName, Height);
            string value = string.Format(ConversionUtils.FourItemListFormat, left, top, right, bottom);
            return value;
        }

        public static bool operator ==(BoxPosition a, BoxPosition b)
        { return a.Equals(b); }

        public static bool operator !=(BoxPosition a, BoxPosition b)
        { return !(a == b); }

        public string SaveAsString()
        {
            Dictionary<string, int> values = new Dictionary<string, int>();
            values.Add(LeftName, Left);
            values.Add(TopName, Top);
            values.Add(WidthName, Width);
            values.Add(HeightName, Height);

            string saveValue = values.ConvertToDictionaryAsString();
            return saveValue;
        }

        public void LoadFromString(string savedValue)
        {
            IDictionary<string, int> values = savedValue.ConvertToTypedDictionary<string, int>();

            m_Left = values[LeftName];
            m_Top = values[TopName];
            m_Width = values[WidthName];
            m_Height = values[HeightName];
        }
    }
}