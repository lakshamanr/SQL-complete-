using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SSMSSQLComplete.Core.Schema;

namespace SSMSSQLComplete.Core.Completion
{
    public class SchemaCompletionProvider : ICompletionProvider
    {
        private readonly SchemaService _schemaService;

        public SchemaCompletionProvider()
        {
            _schemaService = SchemaService.Instance;
        }

        public async Task<IEnumerable<CompletionItem>> GetCompletionsAsync(CompletionContext context)
        {
            var completions = new List<CompletionItem>();

            var metadata = await _schemaService.GetCurrentDatabaseMetadataAsync();
            if (metadata == null)
                return completions;

            // Add table completions
            if (ShouldProvideTableCompletions(context))
            {
                completions.AddRange(metadata.Tables.Select(t => new CompletionItem
                {
                    Label = t.Name,
                    InsertText = t.Name,
                    Kind = CompletionItemKind.Table,
                    Detail = $"Table: {t.Schema}.{t.Name}",
                    Documentation = $"Columns: {t.Columns.Count}",
                    SortPriority = 30
                }));

                completions.AddRange(metadata.Views.Select(v => new CompletionItem
                {
                    Label = v.Name,
                    InsertText = v.Name,
                    Kind = CompletionItemKind.View,
                    Detail = $"View: {v.Schema}.{v.Name}",
                    SortPriority = 35
                }));
            }

            // Add column completions
            if (ShouldProvideColumnCompletions(context))
            {
                foreach (var table in metadata.Tables)
                {
                    completions.AddRange(table.Columns.Select(c => new CompletionItem
                    {
                        Label = c.Name,
                        InsertText = c.Name,
                        Kind = CompletionItemKind.Column,
                        Detail = $"{c.DataType} {(c.IsNullable ? "NULL" : "NOT NULL")}",
                        Documentation = $"Table: {table.Name}\n{(c.IsPrimaryKey ? "Primary Key\n" : "")}{(c.IsForeignKey ? "Foreign Key\n" : "")}",
                        SortPriority = 20
                    }));
                }
            }

            // Add JOIN suggestions based on foreign keys
            if (context.SqlContext?.CurrentClause == "JOIN" && context.SqlContext.Tables.Any())
            {
                var joinSuggestions = GenerateJoinSuggestions(metadata, context.SqlContext.Tables);
                completions.AddRange(joinSuggestions);
            }

            return completions;
        }

        public bool CanProvideCompletions(CompletionContext context)
        {
            // Can provide if we have schema metadata available
            return _schemaService.IsSchemaLoaded;
        }

        private bool ShouldProvideTableCompletions(CompletionContext context)
        {
            if (context.SqlContext == null)
                return false;

            return context.SqlContext.CurrentClause == "FROM" ||
                   context.SqlContext.CurrentClause == "JOIN" ||
                   context.SqlContext.CurrentClause == "UPDATE" ||
                   context.SqlContext.CurrentClause == "INTO";
        }

        private bool ShouldProvideColumnCompletions(CompletionContext context)
        {
            if (context.SqlContext == null)
                return false;

            return context.SqlContext.CurrentClause == "SELECT" ||
                   context.SqlContext.CurrentClause == "WHERE" ||
                   context.SqlContext.CurrentClause == "ORDER BY" ||
                   context.SqlContext.CurrentClause == "GROUP BY" ||
                   context.SqlContext.CurrentClause == "HAVING";
        }

        private IEnumerable<CompletionItem> GenerateJoinSuggestions(
            DatabaseMetadata metadata,
            IEnumerable<string> currentTables)
        {
            var suggestions = new List<CompletionItem>();

            foreach (var tableName in currentTables)
            {
                var table = metadata.Tables.FirstOrDefault(t => t.Name.Equals(tableName, System.StringComparison.OrdinalIgnoreCase));
                if (table == null)
                    continue;

                // Find foreign keys from this table
                foreach (var fk in metadata.ForeignKeys.Where(fk => fk.FromTable == table.Name))
                {
                    var joinText = $"{fk.ToTable} ON {tableName}.{fk.FromColumn} = {fk.ToTable}.{fk.ToColumn}";
                    suggestions.Add(new CompletionItem
                    {
                        Label = $"JOIN {fk.ToTable}",
                        InsertText = joinText,
                        Kind = CompletionItemKind.Join,
                        Detail = "Suggested JOIN based on foreign key",
                        Documentation = $"Foreign Key: {fk.Name}",
                        SortPriority = 10
                    });
                }
            }

            return suggestions;
        }
    }
}
