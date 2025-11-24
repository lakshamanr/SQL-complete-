using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.Core.Config;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Commands
{
    internal sealed class ToggleCompletionCommand
    {
        public const int CommandId = 0x0103;
        public static readonly Guid CommandSet = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        private readonly AsyncPackage _package;
        private bool _isEnabled;

        private ToggleCompletionCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var settings = SettingsManager.Instance.GetCompletionSettings();
            _isEnabled = settings.Enabled;

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ToggleCompletionCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ToggleCompletionCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                _isEnabled = !_isEnabled;

                var settings = SettingsManager.Instance.GetCompletionSettings();
                settings.Enabled = _isEnabled;
                SettingsManager.Instance.SaveCompletionSettings(settings);

                var status = _isEnabled ? "enabled" : "disabled";
                Core.Infrastructure.Logger.Instance.Info($"SQL Completion {status}");
                Core.Infrastructure.TelemetryService.Instance.TrackEvent("ToggleCompletion",
                    new System.Collections.Generic.Dictionary<string, string> { { "Enabled", _isEnabled.ToString() } });
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error toggling completion: {ex.Message}", ex);
            }
        }
    }
}
