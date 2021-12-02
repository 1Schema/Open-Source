using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Computation
{
    public enum ComputationMode
    {
        OnDemand,
        AsUpdated,
        OnInactivity,
    }

    public static class ComputationModeUtils
    {
        public const ComputationMode DefaultComputationMode = ComputationMode.OnDemand;
        public const int DefaultInactivityDelayInMins = 30;

        public static TimeSpan DefaultInactivityDelay
        {
            get { return new TimeSpan(0, DefaultInactivityDelayInMins, 0); }
        }

        public static Nullable<TimeSpan> GetDefaultDelayTimeSpan(this ComputationMode mode)
        {
            if (mode == ComputationMode.OnDemand)
            { return null; }
            else if (mode == ComputationMode.AsUpdated)
            { return null; }
            else if (mode == ComputationMode.OnInactivity)
            { return DefaultInactivityDelay; }
            else
            { throw new InvalidOperationException("Unrecognized ComputationMode encountered."); }
        }
    }
}