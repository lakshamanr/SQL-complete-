using System;
using System.ComponentModel.Design;
using System.Data;
using Microsoft.VisualStudio.Shell;
using SSMSSQLComplete.UI.Dialogs;
using Task = System.Threading.Tasks.Task;

namespace SSMSSQLComplete.Commands
{
    /// <summary>
    /// Command to open enhanced results viewer
    /// </summary>
    internal sealed class ShowResultsViewerCommand
    {
        public const int CommandId = 0x0104;
        public static readonly Guid CommandSet = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        private readonly AsyncPackage _package;

        private ShowResultsViewerCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ShowResultsViewerCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowResultsViewerCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                // For demo purposes, create sample data
                // In real implementation, this would capture actual query results
                var sampleData = CreateSampleData();

                var dialog = new ResultsViewerDialog(sampleData);
                dialog.ShowDialog();

                Core.Infrastructure.TelemetryService.Instance.TrackEvent("ShowResultsViewer");
            }
            catch (Exception ex)
            {
                Core.Infrastructure.Logger.Instance.Error($"Error opening results viewer: {ex.Message}", ex);
            }
        }

        private DataTable CreateSampleData()
        {
            var table = new DataTable();

            // Add columns
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("JsonData", typeof(string));
            table.Columns.Add("CreatedDate", typeof(DateTime));

            // Add sample rows with JSON data
            table.Rows.Add(1, "John Doe", "john@example.com",
                "{\"age\": 30, \"city\": \"New York\", \"active\": true}",
                DateTime.Now.AddDays(-10));

            table.Rows.Add(2, "Jane Smith", "jane@example.com",
                "{\"age\": 25, \"city\": \"Los Angeles\", \"active\": true, \"tags\": [\"premium\", \"verified\"]}",
                DateTime.Now.AddDays(-5));

            table.Rows.Add(3, "Bob Johnson", "bob@example.com",
                "{\"age\": 35, \"city\": \"Chicago\", \"active\": false, \"preferences\": {\"theme\": \"dark\", \"notifications\": true}}",
                DateTime.Now.AddDays(-2));

            table.Rows.Add(4, "Alice Williams", null,
                "{\"age\": 28, \"city\": \"Houston\"}",
                DateTime.Now);

            table.Rows.Add(5, "Charlie Brown", "charlie@example.com",
                "[{\"id\": 1, \"name\": \"Item 1\"}, {\"id\": 2, \"name\": \"Item 2\"}]",
                DateTime.Now.AddDays(-1));

            return table;
        }
    }
}
