using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Core.Refactoring
{
    public class RenameAliasRefactoring : IRefactoring
    {
        public string Name => "Rename Alias";
        public string Description => "Renames an alias throughout the query";

        private string _oldAlias;
        private string _newAlias;

        public RenameAliasRefactoring(string oldAlias = null, string newAlias = null)
        {
            _oldAlias = oldAlias;
            _newAlias = newAlias;
        }

        public bool CanApply(string sql, int position)
        {
            // Need alias parameters
            return !string.IsNullOrWhiteSpace(_oldAlias) && !string.IsNullOrWhiteSpace(_newAlias);
        }

        public async Task<RefactoringResult> ApplyAsync(string sql, int position, RefactoringOptions options = null)
        {
            try
            {
                var tokenizer = new SqlTokenizer();
                var tokens = tokenizer.Tokenize(sql);

                var result = new StringBuilder();

                foreach (var token in tokens)
                {
                    if (token.Type == SqlTokenType.Identifier &&
                        token.Text.Equals(_oldAlias, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Append(_newAlias);
                    }
                    else
                    {
                        result.Append(token.Text);
                    }
                }

                return await Task.FromResult(RefactoringResult.Success(result.ToString()));
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error in RenameAliasRefactoring: {ex.Message}", ex);
                return RefactoringResult.Failure(ex.Message);
            }
        }
    }
}
