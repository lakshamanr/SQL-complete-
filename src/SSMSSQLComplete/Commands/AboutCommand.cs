using System;
using System.ComponentModel.Design;
using System.Reflection;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Core.Licensing;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Commands
{
    /// <summary>
    /// Command to show About dialog with license information
    /// </summary>
    internal sealed class AboutCommand
    {
        public const int CommandId = 0x0111;
        public static readonly Guid CommandSet = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        private readonly AsyncPackage package;

        private AboutCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static AboutCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider => package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AboutCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var license = LicenseManager.Instance.GetCurrentLicense();
                var trialInfo = TrialManager.Instance.GetTrialInfo();

                string licenseInfo;
                if (license.Type == LicenseType.Trial || license.Type == LicenseType.Unlicensed)
                {
                    licenseInfo = trialInfo.GetDisplayMessage();
                }
                else
                {
                    licenseInfo = $"Licensed to: {license.LicensedTo}\n" +
                                  $"License Type: {license.Type}\n" +
                                  $"Status: {license.GetDisplayStatus()}";
                }

                var message = $"SSMS SQL Complete\n" +
                              $"Version {version.Major}.{version.Minor}.{version.Build}\n\n" +
                              $"{licenseInfo}\n\n" +
                              $"Advanced SQL IntelliSense and productivity tools for SQL Server Management Studio\n\n" +
                              $"© 2024 All Rights Reserved";

                Microsoft.VisualStudio.Shell.VsShellUtilities.ShowMessageBox(
                    ServiceProvider,
                    message,
                    "About SSMS SQL Complete",
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error showing about dialog: {ex.Message}", ex);
            }
        }
    }
}
