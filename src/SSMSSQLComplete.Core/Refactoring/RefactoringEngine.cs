using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Refactoring
{
    public class RefactoringEngine
    {
        private readonly List<IRefactoring> _refactorings;

        public RefactoringEngine()
        {
            _refactorings = new List<IRefactoring>
            {
                new ExpandSelectStarRefactoring(),
                new QualifyIdentifiersRefactoring()
                // Note: RenameAliasRefactoring and ExtractToCteRefactoring require parameters
                // and should be added via AddRefactoring() method when parameters are known
            };
        }

        public IEnumerable<IRefactoring> GetAvailableRefactorings(string sql, int position)
        {
            return _refactorings.Where(r => r.CanApply(sql, position));
        }

        public async Task<RefactoringResult> ApplyRefactoringAsync(
            string refactoringName,
            string sql,
            int position,
            RefactoringOptions options = null)
        {
            var refactoring = _refactorings.FirstOrDefault(r => r.Name == refactoringName);

            if (refactoring == null)
                return RefactoringResult.Failure($"Refactoring '{refactoringName}' not found");

            if (!refactoring.CanApply(sql, position))
                return RefactoringResult.Failure($"Refactoring '{refactoringName}' cannot be applied");

            return await refactoring.ApplyAsync(sql, position, options);
        }

        public void AddRefactoring(IRefactoring refactoring)
        {
            _refactorings.Add(refactoring);
        }

        /// <summary>
        /// Apply rename alias refactoring with specified parameters
        /// </summary>
        public async Task<RefactoringResult> RenameAliasAsync(
            string sql,
            int position,
            string oldAlias,
            string newAlias,
            RefactoringOptions options = null)
        {
            var refactoring = new RenameAliasRefactoring(oldAlias, newAlias);
            return await refactoring.ApplyAsync(sql, position, options);
        }

        /// <summary>
        /// Apply extract to CTE refactoring with specified parameters
        /// </summary>
        public async Task<RefactoringResult> ExtractToCteAsync(
            string sql,
            int position,
            string cteName,
            RefactoringOptions options = null)
        {
            var refactoring = new ExtractToCteRefactoring(cteName);
            return await refactoring.ApplyAsync(sql, position, options);
        }
    }
}
