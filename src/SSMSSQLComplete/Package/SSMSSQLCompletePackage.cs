using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Commands;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Package
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(UI.OptionsPages.CompletionOptionsPage), "SSMS SQL Complete", "Completion", 0, 0, true)]
    [ProvideOptionPage(typeof(UI.OptionsPages.FormattingOptionsPage), "SSMS SQL Complete", "Formatting", 0, 0, true)]
    [ProvideOptionPage(typeof(UI.OptionsPages.SchemaOptionsPage), "SSMS SQL Complete", "Schema Cache", 0, 0, true)]
    [ProvideOptionPage(typeof(UI.OptionsPages.SnippetsOptionsPage), "SSMS SQL Complete", "Snippets", 0, 0, true)]
    [ProvideOptionPage(typeof(UI.OptionsPages.TelemetryOptionsPage), "SSMS SQL Complete", "Telemetry", 0, 0, true)]
    public sealed class SSMSSQLCompletePackage : AsyncPackage
    {
        public const string PackageGuidString = "8C9E5A5B-2E3D-4F1A-9B8C-1D5E6F7A8B9C";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Initialize commands
            await FormatDocumentCommand.InitializeAsync(this);
            await FormatSelectionCommand.InitializeAsync(this);
            await ManageSnippetsCommand.InitializeAsync(this);
            await ToggleCompletionCommand.InitializeAsync(this);
            await ShowResultsViewerCommand.InitializeAsync(this);
            await ActivateLicenseCommand.InitializeAsync(this);
            await AboutCommand.InitializeAsync(this);

            // Initialize services
            await InitializeServicesAsync();

            // Check license status
            await CheckLicenseAsync();
        }

        private async Task InitializeServicesAsync()
        {
            await Task.Run(() =>
            {
                // Initialize schema service
                var schemaService = Core.Schema.SchemaService.Instance;

                // Initialize telemetry
                var telemetryService = Core.Infrastructure.TelemetryService.Instance;
                telemetryService.TrackEvent("PackageInitialized");

                // Initialize logger
                var logger = Core.Infrastructure.Logger.Instance;
                logger.Info("SSMS SQL Complete package initialized successfully");
            });
        }

        private async Task CheckLicenseAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var license = Core.Licensing.LicenseManager.Instance.GetCurrentLicense();

                    if (license.Type == Core.Licensing.LicenseType.Unlicensed)
                    {
                        // No license, start trial if not already started
                        var trialInfo = Core.Licensing.TrialManager.Instance.GetTrialInfo();
                        if (!trialInfo.IsTrialStarted)
                        {
                            Core.Licensing.TrialManager.Instance.StartTrial();
                            Core.Infrastructure.Logger.Instance.Info("Trial period started automatically");
                        }
                    }
                    else if (license.Type == Core.Licensing.LicenseType.Trial)
                    {
                        var trialInfo = Core.Licensing.TrialManager.Instance.GetTrialInfo();
                        if (trialInfo.IsExpired)
                        {
                            Core.Infrastructure.Logger.Instance.Warn("Trial period has expired");
                            ShowTrialExpiredNotification();
                        }
                        else if (trialInfo.DaysRemaining <= 7)
                        {
                            Core.Infrastructure.Logger.Instance.Info($"Trial expiring in {trialInfo.DaysRemaining} days");
                        }
                    }

                    Core.Infrastructure.Logger.Instance.Info($"License status: {license.GetDisplayStatus()}");
                    Core.Infrastructure.TelemetryService.Instance.TrackEvent("LicenseChecked", new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "LicenseType", license.Type.ToString() },
                        { "IsValid", license.IsValid().ToString() }
                    });
                }
                catch (Exception ex)
                {
                    Core.Infrastructure.Logger.Instance.Error($"Error checking license: {ex.Message}", ex);
                }
            });
        }

        private void ShowTrialExpiredNotification()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var result = Microsoft.VisualStudio.Shell.VsShellUtilities.ShowMessageBox(
                    this,
                    "Your trial period for SSMS SQL Complete has expired.\n\n" +
                    "Would you like to activate a license now?",
                    "Trial Expired",
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                if (result == 6) // IDYES
                {
                    var dialog = new UI.Dialogs.LicenseActivationDialog();
                    dialog.ShowDialog();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Core.Infrastructure.TelemetryService.Instance.Flush();
                Core.Infrastructure.Logger.Instance.Flush();
            }
            base.Dispose(disposing);
        }
    }
}
