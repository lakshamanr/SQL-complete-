using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using SSMSSQLComplete.Core.Schema;
using SSMSSQLComplete.Core.Infrastructure;

namespace SSMSSQLComplete.Editor
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("SQL")]
    [ContentType("TSQL")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TextViewListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            try
            {
                // Get or create text view properties
                textView.Properties.GetOrCreateSingletonProperty(() => new TextViewProperties(textView));

                // Subscribe to view events
                textView.GotAggregateFocus += OnTextViewGotFocus;
                textView.Closed += OnTextViewClosed;

                Logger.Instance.Info("SQL text view listener attached");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in TextViewCreated: {ex.Message}", ex);
            }
        }

        private void OnTextViewGotFocus(object sender, EventArgs e)
        {
            if (sender is IWpfTextView textView)
            {
                try
                {
                    // Trigger schema refresh for the current connection
                    var schemaService = SchemaService.Instance;
                    _ = schemaService.RefreshSchemaAsync(); // Fire and forget
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Error refreshing schema on focus: {ex.Message}", ex);
                }
            }
        }

        private void OnTextViewClosed(object sender, EventArgs e)
        {
            if (sender is IWpfTextView textView)
            {
                textView.GotAggregateFocus -= OnTextViewGotFocus;
                textView.Closed -= OnTextViewClosed;

                // Dispose completion handler to prevent memory leaks
                if (textView.Properties.TryGetProperty(typeof(SqlCompletionCommandHandler), out SqlCompletionCommandHandler handler))
                {
                    handler?.Dispose();
                    textView.Properties.RemoveProperty(typeof(SqlCompletionCommandHandler));
                }
            }
        }

        private class TextViewProperties
        {
            public IWpfTextView TextView { get; }

            public TextViewProperties(IWpfTextView textView)
            {
                TextView = textView;
            }
        }
    }
}
