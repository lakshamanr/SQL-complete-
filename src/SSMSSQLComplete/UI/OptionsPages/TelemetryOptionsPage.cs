using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Core.Config;

namespace SSMSSQLComplete.UI.OptionsPages
{
    public class TelemetryOptionsPage : DialogPage
    {
        [Category("Privacy")]
        [DisplayName("Enable Telemetry")]
        [Description("Send anonymous usage data to help improve the product")]
        public bool Enabled { get; set; } = true;

        [Category("Privacy")]
        [DisplayName("Collect Usage Data")]
        [Description("Collect information about feature usage")]
        public bool CollectUsageData { get; set; } = true;

        [Category("Privacy")]
        [DisplayName("Collect Error Reports")]
        [Description("Automatically send error reports")]
        public bool CollectErrorReports { get; set; } = true;

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
            var settings = SettingsManager.Instance.GetTelemetrySettings();
            Enabled = settings.Enabled;
            CollectUsageData = settings.CollectUsageData;
            CollectErrorReports = settings.CollectErrorReports;
        }

        private void SaveSettings()
        {
            var settings = new TelemetrySettings
            {
                Enabled = Enabled,
                CollectUsageData = CollectUsageData,
                CollectErrorReports = CollectErrorReports
            };

            SettingsManager.Instance.SaveTelemetrySettings(settings);
            Core.Infrastructure.TelemetryService.Instance.IsEnabled = Enabled;
        }
    }
}
