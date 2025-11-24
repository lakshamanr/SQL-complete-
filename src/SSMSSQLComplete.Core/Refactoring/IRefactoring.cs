using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Refactoring
{
    public interface IRefactoring
    {
        string Name { get; }
        string Description { get; }

        /// <summary>
        /// Checks if this refactoring can be applied to the given SQL
        /// </summary>
        bool CanApply(string sql, int position);

        /// <summary>
        /// Applies the refactoring and returns the modified SQL
        /// </summary>
        Task<RefactoringResult> ApplyAsync(string sql, int position, RefactoringOptions options = null);
    }

    public class RefactoringResult
    {
        public bool Success { get; set; }
        public string ModifiedSql { get; set; }
        public string ErrorMessage { get; set; }
        public int CursorPosition { get; set; }

        public static RefactoringResult Failure(string error)
        {
            return new RefactoringResult { Success = false, ErrorMessage = error };
        }

        public static RefactoringResult Success(string modifiedSql, int cursorPosition = 0)
        {
            return new RefactoringResult
            {
                Success = true,
                ModifiedSql = modifiedSql,
                CursorPosition = cursorPosition
            };
        }
    }

    public class RefactoringOptions
    {
        // Common options for refactorings
        public bool PreserveFormatting { get; set; } = true;
    }
}
