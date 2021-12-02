using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Presentation
{
    public interface ICellRenderedComponent
    {
        SizeT<Nullable<double>> CellSize_Min { get; }
        SizeT<Nullable<double>> CellSize_Desired { get; }
        SizeT<Nullable<double>> CellSize_Max { get; }
    }
}