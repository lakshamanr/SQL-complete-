using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Core.Config;
using SSMSSQLComplete.Core.Formatting;

namespace SSMSSQLComplete.UI.OptionsPages
{
    public class FormattingOptionsPage : DialogPage
    {
        [Category("Casing")]
        [DisplayName("Keyword Casing")]
        [Description("Case style for SQL keywords")]
        public KeywordCase KeywordCasing { get; set; } = KeywordCase.Uppercase;

        [Category("Indentation")]
        [DisplayName("Enable Indentation")]
        [Description("Enable automatic indentation")]
        public bool IndentEnabled { get; set; } = true;

        [Category("Indentation")]
        [DisplayName("Indent Size")]
        [Description("Number of spaces for each indent level")]
        public int IndentSize { get; set; } = 4;

        [Category("Layout")]
        [DisplayName("Align Column Lists")]
        [Description("Align columns in SELECT statements")]
        public bool AlignColumnLists { get; set; } = true;

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
            var settings = SettingsManager.Instance.GetFormattingSettings();
            KeywordCasing = settings.KeywordCasing;
            IndentEnabled = settings.IndentEnabled;
            IndentSize = settings.IndentSize;
            AlignColumnLists = settings.AlignColumnLists;
        }

        private void SaveSettings()
        {
            var settings = new FormattingSettings
            {
                KeywordCasing = KeywordCasing,
                IndentEnabled = IndentEnabled,
                IndentSize = IndentSize,
                AlignColumnLists = AlignColumnLists
            };

            SettingsManager.Instance.SaveFormattingSettings(settings);
        }
    }
}
