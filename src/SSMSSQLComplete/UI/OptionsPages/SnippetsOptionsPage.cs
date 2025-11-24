using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace SSMSSQLComplete.UI.OptionsPages
{
    public class SnippetsOptionsPage : DialogPage
    {
        [Category("General")]
        [DisplayName("Enable Snippets")]
        [Description("Enable code snippets in completion")]
        public bool EnableSnippets { get; set; } = true;

        [Category("General")]
        [DisplayName("Snippet Trigger Key")]
        [Description("Key to expand snippets (default: Tab)")]
        public string TriggerKey { get; set; } = "Tab";

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
        }
    }
}
