using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.DataStructures
{
    public class TimeLagSet
    {
        public const int DefaultLag = 0;

        public TimeLagSet()
            : this(DefaultLag, DefaultLag)
        { }

        public TimeLagSet(int lagForD1, int lagForD2)
        {
            LagForD1 = lagForD1;
            LagForD2 = lagForD2;
        }

        public int LagForD1 { get; set; }
        public int LagForD2 { get; set; }
    }
}