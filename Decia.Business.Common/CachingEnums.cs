using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Decia.Business.Common
{
    public enum LocalCacheMode
    {
        [Description("Do Not Cache")]
        IdOnly,
        [Description("Cache Objects")]
        DeepCopy
    }

    public enum ForeignCacheMode
    {
        [Description("Do Not Cache")]
        None,
        [Description("Cache List of Objects")]
        DeepCopy
    }

    public enum CacheInvalidationMode
    {
        [Description("Stores all relevant objects")]
        None,
        [Description("Stores relevant objects up to NUMBER")]
        LastN,
        [Description("Stores relevant objects over the recent TIMESPAN")]
        RecentT,
        [Description("Stores relevant objects newer than DATE")]
        NewerThanD
    }
}