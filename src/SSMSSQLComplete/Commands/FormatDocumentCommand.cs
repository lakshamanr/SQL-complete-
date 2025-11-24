using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using SSMSSQLComplete.Core.Formatting;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Commands
{
    internal sealed class FormatDocumentCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        private readonly AsyncPackage _package;
        private readonly FormattingEngine _formattingEngine;

        private FormatDocumentCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            _formattingEngine = new FormattingEngine();

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static FormatDocumentCommand Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => _package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FormatDocumentCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                // Get active text view
                // This is a simplified implementation - actual implementation would get the current document text
                var documentText = GetCurrentDocumentText();

                if (!string.IsNullOrEmpty(documentText))
                {
                    var formatted = _formattingEngine.Format(documentText);
                    SetCurrentDocumentText(formatted);

                    Core.Infrastructure.TelemetryService.Instance.TrackEvent("FormatDocument");
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error formatting document: {ex.Message}", ex);
            }
        }

        private string GetCurrentDocumentText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var textView = GetActiveTextView();
                if (textView != null)
                {
                    return textView.TextSnapshot.GetText();
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error getting document text: {ex.Message}", ex);
            }

            return string.Empty;
        }

        private void SetCurrentDocumentText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var textView = GetActiveTextView();
                if (textView != null)
                {
                    var edit = textView.TextBuffer.CreateEdit();
                    edit.Replace(0, textView.TextBuffer.CurrentSnapshot.Length, text);
                    edit.Apply();
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error setting document text: {ex.Message}", ex);
            }
        }

        private IWpfTextView GetActiveTextView()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var textManager = ServiceProvider.GetServiceAsync(typeof(SVsTextManager)).Result as IVsTextManager;
                if (textManager == null)
                    return null;

                textManager.GetActiveView(1, null, out IVsTextView textView);
                if (textView == null)
                    return null;

                var componentModel = ServiceProvider.GetServiceAsync(typeof(SComponentModelHost)).Result as IComponentModelHost;
                if (componentModel == null)
                    return null;

                var editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
                return editorAdapter.GetWpfTextView(textView);
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error getting active text view: {ex.Message}", ex);
                return null;
            }
        }
    }
}
