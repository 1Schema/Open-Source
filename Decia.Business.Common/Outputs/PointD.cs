using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public struct PointD
    {
        private double m_X;
        private double m_Y;

        public PointD(double x, double y)
        {
            m_X = x;
            m_Y = y;
        }

        public double X { get { return m_X; } }
        public double Y { get { return m_Y; } }
    }
}