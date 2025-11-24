using System.Collections.Generic;
using System.Linq;

namespace SSMSSQLComplete.Core.Completion
{
    public class CompletionRanker
    {
        public IEnumerable<CompletionItem> RankCompletions(
            IEnumerable<CompletionItem> completions,
            CompletionContext context)
        {
            if (completions == null || !completions.Any())
                return Enumerable.Empty<CompletionItem>();

            // Get the word being typed
            var word = GetWordAtPosition(context.Text, context.Position);

            var ranked = completions.Select(c => new
            {
                Item = c,
                Score = CalculateScore(c, word, context)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Item.SortPriority)
            .ThenBy(x => x.Item.Label)
            .Select(x => x.Item);

            return ranked;
        }

        private double CalculateScore(CompletionItem item, string word, CompletionContext context)
        {
            double score = 0;

            // Fuzzy match score
            var fuzzyScore = FuzzyMatcher.CalculateScore(word, item.Label);
            score += fuzzyScore;

            // Context-based scoring
            if (context.SqlContext != null)
            {
                // Boost keywords in appropriate contexts
                if (item.Kind == CompletionItemKind.Keyword)
                {
                    score += 10;
                }

                // Boost tables in FROM/JOIN contexts
                if ((item.Kind == CompletionItemKind.Table || item.Kind == CompletionItemKind.View) &&
                    (context.SqlContext.CurrentClause == "FROM" || context.SqlContext.CurrentClause == "JOIN"))
                {
                    score += 20;
                }

                // Boost columns in SELECT/WHERE contexts
                if (item.Kind == CompletionItemKind.Column &&
                    (context.SqlContext.CurrentClause == "SELECT" ||
                     context.SqlContext.CurrentClause == "WHERE" ||
                     context.SqlContext.CurrentClause == "ORDER BY"))
                {
                    score += 15;
                }
            }

            // Priority boost
            score -= item.SortPriority * 0.1;

            return score;
        }

        private string GetWordAtPosition(string text, int position)
        {
            if (string.IsNullOrEmpty(text) || position <= 0)
                return string.Empty;

            int start = position - 1;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
            {
                start--;
            }
            start++;

            if (start >= position)
                return string.Empty;

            return text.Substring(start, position - start);
        }
    }
}
