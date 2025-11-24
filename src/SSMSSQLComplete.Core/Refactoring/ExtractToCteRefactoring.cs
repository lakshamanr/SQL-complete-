using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Refactoring
{
    public class ExtractToCteRefactoring : IRefactoring
    {
        private static readonly Regex SubqueryRegex = new Regex(
            @"\(\s*SELECT[^)]+\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string Name => "Extract to CTE";
        public string Description => "Extracts subquery to Common Table Expression";

        private string _cteName;

        public ExtractToCteRefactoring(string cteName = null)
        {
            _cteName = cteName ?? "CTE";
        }

        public bool CanApply(string sql, int position)
        {
            return SubqueryRegex.IsMatch(sql);
        }

        public async Task<RefactoringResult> ApplyAsync(string sql, int position, RefactoringOptions options = null)
        {
            try
            {
                var match = SubqueryRegex.Match(sql);
                if (!match.Success)
                    return RefactoringResult.Failure("No subquery found");

                var subquery = match.Value.Trim('(', ')').Trim();

                // Build CTE
                var cte = $"WITH {_cteName} AS\n(\n    {subquery}\n)\n";

                // Replace subquery with CTE reference
                var modifiedSql = sql.Replace(match.Value, _cteName);

                // Prepend CTE definition
                modifiedSql = cte + modifiedSql;

                return await Task.FromResult(RefactoringResult.Success(modifiedSql));
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error in ExtractToCteRefactoring: {ex.Message}", ex);
                return RefactoringResult.Failure(ex.Message);
            }
        }
    }
}
