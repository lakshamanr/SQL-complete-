using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Core.Config;

namespace SSMSSQLComplete.UI.OptionsPages
{
    public class CompletionOptionsPage : DialogPage
    {
        [Category("General")]
        [DisplayName("Enable Completion")]
        [Description("Enable or disable SQL completion")]
        public bool Enabled { get; set; } = true;

        [Category("General")]
        [DisplayName("Auto Trigger")]
        [Description("Automatically trigger completion while typing")]
        public bool AutoTrigger { get; set; } = true;

        [Category("General")]
        [DisplayName("Trigger Delay (ms)")]
        [Description("Delay before triggering completion (milliseconds)")]
        public int TriggerDelay { get; set; } = 100;

        [Category("Suggestions")]
        [DisplayName("Fuzzy Matching")]
        [Description("Enable fuzzy matching for suggestions")]
        public bool FuzzyMatching { get; set; } = true;

        [Category("Suggestions")]
        [DisplayName("Show Keywords")]
        [Description("Show T-SQL keywords in suggestions")]
        public bool ShowKeywords { get; set; } = true;

        [Category("Suggestions")]
        [DisplayName("Show Tables")]
        [Description("Show tables in suggestions")]
        public bool ShowTables { get; set; } = true;

        [Category("Suggestions")]
        [DisplayName("Show Columns")]
        [Description("Show columns in suggestions")]
        public bool ShowColumns { get; set; } = true;

        [Category("Suggestions")]
        [DisplayName("Show Snippets")]
        [Description("Show code snippets in suggestions")]
        public bool ShowSnippets { get; set; } = true;

        [Category("Suggestions")]
        [DisplayName("Max Suggestions")]
        [Description("Maximum number of suggestions to show")]
        public int MaxSuggestions { get; set; } = 50;

        protected override void OnActivate(System.ComponentModel.CancelEventArgs e)
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
            var settings = SettingsManager.Instance.GetCompletionSettings();
            Enabled = settings.Enabled;
            AutoTrigger = settings.AutoTrigger;
            TriggerDelay = settings.TriggerDelay;
            FuzzyMatching = settings.FuzzyMatching;
            ShowKeywords = settings.ShowKeywords;
            ShowTables = settings.ShowTables;
            ShowColumns = settings.ShowColumns;
            ShowSnippets = settings.ShowSnippets;
            MaxSuggestions = settings.MaxSuggestions;
        }

        private void SaveSettings()
        {
            var settings = new CompletionSettings
            {
                Enabled = Enabled,
                AutoTrigger = AutoTrigger,
                TriggerDelay = TriggerDelay,
                FuzzyMatching = FuzzyMatching,
                ShowKeywords = ShowKeywords,
                ShowTables = ShowTables,
                ShowColumns = ShowColumns,
                ShowSnippets = ShowSnippets,
                MaxSuggestions = MaxSuggestions
            };

            SettingsManager.Instance.SaveCompletionSettings(settings);
        }
    }
}
