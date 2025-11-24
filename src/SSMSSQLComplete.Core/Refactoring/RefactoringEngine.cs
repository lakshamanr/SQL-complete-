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
                new QualifyIdentifiersRefactoring(),
                new RenameAliasRefactoring(),
                new ExtractToCteRefactoring()
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
    }
}
