using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SSMSSQLComplete.Core.Infrastructure;

namespace SSMSSQLComplete.Core.Results
{
    /// <summary>
    /// Service for capturing and displaying query results
    /// </summary>
    public sealed class ResultsCaptureService
    {
        private static readonly Lazy<ResultsCaptureService> _instance =
            new Lazy<ResultsCaptureService>(() => new ResultsCaptureService());

        public static ResultsCaptureService Instance => _instance.Value;

        private ResultsCaptureService()
        {
        }

        /// <summary>
        /// Execute query and capture results
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string connectionString, string query)
        {
            var dataTable = new DataTable();

            try
            {
                using (var perfMon = PerformanceMonitor.Start("ExecuteQuery"))
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.CommandTimeout = 30;

                            using (var adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                            }
                        }
                    }

                    perfMon.RecordMetric("RowCount", dataTable.Rows.Count);
                    perfMon.RecordMetric("ColumnCount", dataTable.Columns.Count);
                }

                Logger.Instance.Info($"Query executed: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns");

                return dataTable;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error executing query: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Detect if a column contains JSON data
        /// </summary>
        public bool IsJsonColumn(DataColumn column, DataTable table)
        {
            if (table.Rows.Count == 0)
                return false;

            var columnName = column.ColumnName.ToLower();

            // Check column name hints
            if (columnName.Contains("json") ||
                columnName.Contains("data") ||
                columnName.Contains("metadata") ||
                columnName.Contains("payload") ||
                columnName.Contains("content"))
            {
                // Verify by checking actual data
                var sampleSize = Math.Min(5, table.Rows.Count);
                var jsonCount = 0;

                for (int i = 0; i < sampleSize; i++)
                {
                    var value = table.Rows[i][column]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value) && IsJsonString(value))
                    {
                        jsonCount++;
                    }
                }

                return jsonCount > 0; // At least one row has JSON
            }

            return false;
        }

        /// <summary>
        /// Check if string is valid JSON
        /// </summary>
        private bool IsJsonString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            if ((text.StartsWith("{") && text.EndsWith("}")) ||
                (text.StartsWith("[") && text.EndsWith("]")))
            {
                try
                {
                    JToken.Parse(text);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Get column data types summary
        /// </summary>
        public string GetColumnTypeSummary(DataTable table)
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("Column Types:");

            foreach (DataColumn column in table.Columns)
            {
                var isJson = IsJsonColumn(column, table);
                var typeName = column.DataType.Name;

                if (isJson)
                    typeName += " (JSON detected)";

                summary.AppendLine($"  {column.ColumnName}: {typeName}");
            }

            return summary.ToString();
        }
    }
}
