using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Core.Refactoring
{
    public class QualifyIdentifiersRefactoring : IRefactoring
    {
        public string Name => "Qualify Identifiers";
        public string Description => "Adds table qualifiers to column names";

        public bool CanApply(string sql, int position)
        {
            // Can always try to qualify
            return !string.IsNullOrWhiteSpace(sql);
        }

        public async Task<RefactoringResult> ApplyAsync(string sql, int position, RefactoringOptions options = null)
        {
            try
            {
                var tokenizer = new SqlTokenizer();
                var tokens = tokenizer.Tokenize(sql);
                var analyzer = new SqlContextAnalyzer();
                var context = analyzer.AnalyzeContext(sql, sql.Length);

                if (!context.Tables.Any())
                    return RefactoringResult.Failure("No tables found in query");

                var result = new StringBuilder();
                var defaultTable = context.Tables.FirstOrDefault();

                for (int i = 0; i < tokens.Count; i++)
                {
                    var token = tokens[i];

                    // Check if this is a column identifier that needs qualification
                    if (token.Type == SqlTokenType.Identifier &&
                        !IsQualified(tokens, i) &&
                        IsInSelectOrWhereClause(tokens, i))
                    {
                        // Add table qualifier
                        result.Append($"{defaultTable}.");
                    }

                    result.Append(token.Text);
                }

                return await Task.FromResult(RefactoringResult.Success(result.ToString()));
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error in QualifyIdentifiersRefactoring: {ex.Message}", ex);
                return RefactoringResult.Failure(ex.Message);
            }
        }

        private bool IsQualified(System.Collections.Generic.List<SqlToken> tokens, int index)
        {
            // Check if previous token is a dot
            if (index > 0 && tokens[index - 1].Type == SqlTokenType.Punctuation && tokens[index - 1].Text == ".")
                return true;

            // Check if next token is a dot
            if (index < tokens.Count - 1 && tokens[index + 1].Type == SqlTokenType.Punctuation && tokens[index + 1].Text == ".")
                return true;

            return false;
        }

        private bool IsInSelectOrWhereClause(System.Collections.Generic.List<SqlToken> tokens, int index)
        {
            // Simple heuristic - look back for SELECT or WHERE keyword
            for (int i = index - 1; i >= 0; i--)
            {
                if (tokens[i].Type == SqlTokenType.Keyword)
                {
                    var keyword = tokens[i].Text.ToUpperInvariant();
                    if (keyword == "SELECT" || keyword == "WHERE")
                        return true;
                    if (keyword == "FROM" || keyword == "JOIN")
                        return false;
                }
            }

            return false;
        }
    }
}
