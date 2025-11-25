using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.UI.Dialogs;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Commands
{
    /// <summary>
    /// Command to activate a license
    /// </summary>
    internal sealed class ActivateLicenseCommand
    {
        public const int CommandId = 0x0110;
        public static readonly Guid CommandSet = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        private readonly AsyncPackage package;

        private ActivateLicenseCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ActivateLicenseCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider => package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ActivateLicenseCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dialog = new LicenseActivationDialog();
                var result = dialog.ShowDialog();

                if (result == true && dialog.WasActivated)
                {
                    Core.Infrastructure.Logger.Instance.Info("License activation dialog completed successfully");
                }
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error showing license activation dialog: {ex.Message}", ex);

                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                Microsoft.VisualStudio.Shell.VsShellUtilities.ShowMessageBox(
                    ServiceProvider,
                    $"Error: {ex.Message}",
                    "License Activation Error",
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_CRITICAL,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
