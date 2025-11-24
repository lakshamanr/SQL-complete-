using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Core.Config;

namespace SSMSSQLComplete.UI.OptionsPages
{
    public class SchemaOptionsPage : DialogPage
    {
        [Category("Cache")]
        [DisplayName("Auto Refresh")]
        [Description("Automatically refresh schema when database changes")]
        public bool AutoRefresh { get; set; } = true;

        [Category("Cache")]
        [DisplayName("Cache Expiration (minutes)")]
        [Description("Time before cached schema expires")]
        public int CacheExpirationMinutes { get; set; } = 30;

        [Category("Cache")]
        [DisplayName("Max Cache Size")]
        [Description("Maximum number of database schemas to cache")]
        public int MaxCacheSize { get; set; } = 10;

        [Category("Metadata")]
        [DisplayName("Fetch Stored Procedures")]
        [Description("Include stored procedures in schema")]
        public bool FetchStoredProcedures { get; set; } = true;

        [Category("Metadata")]
        [DisplayName("Fetch Functions")]
        [Description("Include functions in schema")]
        public bool FetchFunctions { get; set; } = true;

        [Category("Metadata")]
        [DisplayName("Fetch Views")]
        [Description("Include views in schema")]
        public bool FetchViews { get; set; } = true;

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            LoadSettings();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                SaveSettings();
            }
            base.OnApply(e);
        }

        private void LoadSettings()
        {
            var settings = SettingsManager.Instance.GetSchemaSettings();
            AutoRefresh = settings.AutoRefresh;
            CacheExpirationMinutes = settings.CacheExpirationMinutes;
            MaxCacheSize = settings.MaxCacheSize;
            FetchStoredProcedures = settings.FetchStoredProcedures;
            FetchFunctions = settings.FetchFunctions;
            FetchViews = settings.FetchViews;
        }

        private void SaveSettings()
        {
            var settings = new SchemaSettings
            {
                AutoRefresh = AutoRefresh,
                CacheExpirationMinutes = CacheExpirationMinutes,
                MaxCacheSize = MaxCacheSize,
                FetchStoredProcedures = FetchStoredProcedures,
                FetchFunctions = FetchFunctions,
                FetchViews = FetchViews
            };

            SettingsManager.Instance.SaveSchemaSettings(settings);
        }
    }
}
