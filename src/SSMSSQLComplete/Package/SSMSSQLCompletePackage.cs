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

            // Initialize services
            await InitializeServicesAsync();
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
