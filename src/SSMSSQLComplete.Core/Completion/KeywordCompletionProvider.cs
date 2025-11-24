using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Core.Completion
{
    public class KeywordCompletionProvider : ICompletionProvider
    {
        public Task<IEnumerable<CompletionItem>> GetCompletionsAsync(CompletionContext context)
        {
            var keywords = TSqlKeywords.AllKeywords;

            // Filter keywords based on context
            if (context.SqlContext != null)
            {
                keywords = FilterKeywordsByContext(keywords, context.SqlContext);
            }

            var completions = keywords.Select(k => new CompletionItem
            {
                Label = k,
                InsertText = k,
                Kind = CompletionItemKind.Keyword,
                Detail = "T-SQL Keyword",
                SortPriority = 50
            });

            return Task.FromResult(completions);
        }

        public bool CanProvideCompletions(CompletionContext context)
        {
            // Always can provide keyword completions
            return true;
        }

        private IEnumerable<string> FilterKeywordsByContext(IEnumerable<string> keywords, Parsing.SqlContext context)
        {
            // Filter keywords based on current SQL context
            switch (context.CurrentClause?.ToUpperInvariant())
            {
                case "SELECT":
                    // In SELECT clause, prioritize column-related keywords
                    return keywords.Where(k =>
                        k == "DISTINCT" || k == "TOP" || k == "AS" ||
                        k == "FROM" || k == "*" || k == "ALL");

                case "FROM":
                case "JOIN":
                    // In FROM/JOIN, prioritize join and table-related keywords
                    return keywords.Where(k =>
                        k == "INNER" || k == "LEFT" || k == "RIGHT" ||
                        k == "OUTER" || k == "CROSS" || k == "JOIN" ||
                        k == "ON" || k == "WHERE" || k == "AS");

                case "WHERE":
                case "HAVING":
                    // In WHERE/HAVING, prioritize logical and comparison operators
                    return keywords.Where(k =>
                        k == "AND" || k == "OR" || k == "NOT" ||
                        k == "IN" || k == "LIKE" || k == "BETWEEN" ||
                        k == "IS" || k == "NULL" || k == "EXISTS");

                default:
                    return keywords;
            }
        }
    }
}
