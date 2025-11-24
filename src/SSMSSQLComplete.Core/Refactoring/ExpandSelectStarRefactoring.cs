using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SSMSSQLComplete.Core.Parsing;
using SSMSSQLComplete.Core.Schema;

namespace SSMSSQLComplete.Core.Refactoring
{
    public class ExpandSelectStarRefactoring : IRefactoring
    {
        private static readonly Regex SelectStarRegex = new Regex(
            @"SELECT\s+\*\s+FROM\s+(\[?[\w]+\]?\.)?(\[?[\w]+\]?)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string Name => "Expand SELECT *";
        public string Description => "Expands SELECT * to explicit column list";

        public bool CanApply(string sql, int position)
        {
            return SelectStarRegex.IsMatch(sql);
        }

        public async Task<RefactoringResult> ApplyAsync(string sql, int position, RefactoringOptions options = null)
        {
            try
            {
                var match = SelectStarRegex.Match(sql);
                if (!match.Success)
                    return RefactoringResult.Failure("No SELECT * statement found");

                // Extract table name
                var tableName = match.Groups[2].Value.Trim('[', ']');

                // Get columns from schema
                var schemaService = SchemaService.Instance;
                var metadata = await schemaService.GetCurrentDatabaseMetadataAsync();

                if (metadata == null)
                    return RefactoringResult.Failure("Schema not loaded");

                var table = metadata.GetTable(tableName);
                if (table == null)
                    return RefactoringResult.Failure($"Table '{tableName}' not found");

                // Build column list
                var columnList = string.Join(",\n    ", table.Columns.Select(c => c.Name));
                var replacement = $"SELECT\n    {columnList}\nFROM {match.Groups[0].Value.Substring(match.Groups[0].Value.IndexOf("FROM", StringComparison.OrdinalIgnoreCase))}";

                // Replace in SQL
                var modifiedSql = SelectStarRegex.Replace(sql, replacement, 1);

                return RefactoringResult.Success(modifiedSql);
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error in ExpandSelectStarRefactoring: {ex.Message}", ex);
                return RefactoringResult.Failure(ex.Message);
            }
        }
    }
}
