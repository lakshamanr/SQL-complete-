using System;

namespace SSMSSQLComplete.Core.Config
{
    public class SchemaSettings
    {
        public bool AutoRefresh { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 30;
        public int MaxCacheSize { get; set; } = 10;
        public bool FetchStoredProcedures { get; set; } = true;
        public bool FetchFunctions { get; set; } = true;
        public bool FetchViews { get; set; } = true;
    }
}
