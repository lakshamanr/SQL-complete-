using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SSMSSQLComplete.Editor
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("SQL Completion Handler")]
    [ContentType("SQL")]
    [ContentType("TSQL")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class CompletionHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService { get; set; }

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            // Create completion controller
            var controller = new CompletionController(textView, CompletionBroker);

            // Store in properties
            textView.Properties.AddProperty(typeof(CompletionController), controller);
        }
    }
}
