using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using SSMSSQLComplete.Core.Infrastructure;

namespace SSMSSQLComplete.Editor
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("SQL")]
    [ContentType("TSQL")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class SqlTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        internal Microsoft.VisualStudio.Language.Intellisense.ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal Microsoft.VisualStudio.Text.Operations.ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public void VsTextViewCreated(Microsoft.VisualStudio.TextManager.Interop.IVsTextView textViewAdapter)
        {
            try
            {
                ITextView textView = GetTextViewFromAdapter(textViewAdapter);
                if (textView == null)
                    return;

                // Check if this is a SQL document
                if (!IsSqlDocument(textView))
                    return;

                // Attach completion handler
                textView.Properties.GetOrCreateSingletonProperty(() =>
                    new SqlCompletionCommandHandler(textView, CompletionBroker, NavigatorService));

                Logger.Instance.Info($"SQL text view created and completion handler attached");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error creating SQL text view: {ex.Message}", ex);
            }
        }

        private ITextView GetTextViewFromAdapter(Microsoft.VisualStudio.TextManager.Interop.IVsTextView textViewAdapter)
        {
            var componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(
                typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModelHost))
                as Microsoft.VisualStudio.ComponentModelHost.IComponentModelHost;

            var editorAdapterFactory = componentModel?.GetService<Microsoft.VisualStudio.Editor.IVsEditorAdaptersFactoryService>();
            return editorAdapterFactory?.GetWpfTextView(textViewAdapter);
        }

        private bool IsSqlDocument(ITextView textView)
        {
            var contentType = textView.TextBuffer.ContentType;
            return contentType.IsOfType("SQL") ||
                   contentType.IsOfType("TSQL") ||
                   contentType.DisplayName.Contains("SQL");
        }
    }
}
