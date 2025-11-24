using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.UI.Dialogs;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Commands
{
    internal sealed class ManageSnippetsCommand
    {
        public const int CommandId = 0x0102;
        public static readonly Guid CommandSet = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        private readonly AsyncPackage _package;

        private ManageSnippetsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ManageSnippetsCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ManageSnippetsCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dialog = new SnippetManagerDialog();
                dialog.ShowDialog();

                Core.Infrastructure.TelemetryService.Instance.TrackEvent("ManageSnippets");
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error opening snippet manager: {ex.Message}", ex);
            }
        }
    }
}
