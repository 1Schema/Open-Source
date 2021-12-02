using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public class GroupBox
    {
        private int m_GroupNumber_X;
        private int m_GroupNumber_Y;
        private Rectangle m_ViewRect;

        public GroupBox(int groupNumber_X, int groupNumber_Y, Rectangle viewRect)
        {
            m_GroupNumber_X = groupNumber_X;
            m_GroupNumber_Y = groupNumber_Y;
            m_ViewRect = viewRect;
        }

        public int GroupNumber_X { get { return m_GroupNumber_X; } }
        public int GroupNumber_Y { get { return m_GroupNumber_Y; } }
        public Rectangle ViewRect { get { return m_ViewRect; } }
    }
}