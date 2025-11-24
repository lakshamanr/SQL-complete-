namespace SSMSSQLComplete.Core.Config
{
    public class TelemetrySettings
    {
        public bool Enabled { get; set; } = true;
        public bool CollectUsageData { get; set; } = true;
        public bool CollectErrorReports { get; set; } = true;
    }
}
