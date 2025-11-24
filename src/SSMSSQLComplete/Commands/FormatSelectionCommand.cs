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
    internal sealed class FormatSelectionCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        private readonly AsyncPackage _package;
        private readonly FormattingEngine _formattingEngine;

        private FormatSelectionCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            _formattingEngine = new FormattingEngine();

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static FormatSelectionCommand Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => _package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FormatSelectionCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var selectedText = GetSelectedText();

                if (!string.IsNullOrEmpty(selectedText))
                {
                    var formatted = _formattingEngine.Format(selectedText);
                    SetSelectedText(formatted);

                    Core.Infrastructure.TelemetryService.Instance.TrackEvent("FormatSelection");
                }
                else
                {
                    // If no selection, format entire document
                    var textView = GetActiveTextView();
                    if (textView != null)
                    {
                        var documentText = textView.TextSnapshot.GetText();
                        if (!string.IsNullOrEmpty(documentText))
                        {
                            var formatted = _formattingEngine.Format(documentText);
                            var edit = textView.TextBuffer.CreateEdit();
                            edit.Replace(0, textView.TextBuffer.CurrentSnapshot.Length, formatted);
                            edit.Apply();

                            Core.Infrastructure.TelemetryService.Instance.TrackEvent("FormatDocument_FromSelection");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error formatting selection: {ex.Message}", ex);
            }
        }

        private string GetSelectedText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var textView = GetActiveTextView();
                if (textView != null && !textView.Selection.IsEmpty)
                {
                    return textView.Selection.StreamSelectionSpan.GetText();
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error getting selected text: {ex.Message}", ex);
            }

            return string.Empty;
        }

        private void SetSelectedText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var textView = GetActiveTextView();
                if (textView != null && !textView.Selection.IsEmpty)
                {
                    var selectedSpan = textView.Selection.StreamSelectionSpan.SnapshotSpan;
                    var edit = textView.TextBuffer.CreateEdit();
                    edit.Replace(selectedSpan, text);
                    edit.Apply();
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error setting selected text: {ex.Message}", ex);
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
