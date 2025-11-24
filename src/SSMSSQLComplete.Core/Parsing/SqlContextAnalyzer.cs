using System.Collections.Generic;
using System.Linq;

namespace SSMSSQLComplete.Core.Parsing
{
    public class SqlContextAnalyzer
    {
        private readonly SqlTokenizer _tokenizer;

        public SqlContextAnalyzer()
        {
            _tokenizer = new SqlTokenizer();
        }

        public SqlContext AnalyzeContext(string sql, int position)
        {
            var context = new SqlContext();

            if (string.IsNullOrEmpty(sql))
                return context;

            // Tokenize the SQL
            var tokens = _tokenizer.Tokenize(sql);

            // Find tokens before the position
            var relevantTokens = tokens.Where(t => t.Start < position).ToList();
            if (!relevantTokens.Any())
                return context;

            // Get the previous non-whitespace token
            var previousToken = relevantTokens
                .Where(t => t.Type != SqlTokenType.Whitespace && t.Type != SqlTokenType.Comment)
                .LastOrDefault();

            context.PreviousToken = previousToken;

            // Check if we're after a dot
            if (previousToken?.Type == SqlTokenType.Punctuation && previousToken.Text == ".")
            {
                context.AfterDot = true;

                // Find the table/alias before the dot
                var beforeDot = relevantTokens
                    .Where(t => t.Type == SqlTokenType.Identifier && t.End == previousToken.Start)
                    .LastOrDefault();

                if (beforeDot != null)
                {
                    context.TableQualifier = beforeDot.Text;
                }
            }

            // Determine current clause
            context.CurrentClause = DetermineCurrentClause(relevantTokens);

            // Extract table names and aliases
            ExtractTablesAndAliases(relevantTokens, context);

            // Extract current word being typed
            var currentToken = relevantTokens.LastOrDefault(t => t.End <= position);
            if (currentToken != null && currentToken.Type == SqlTokenType.Identifier)
            {
                context.CurrentWord = currentToken.Text;
            }

            // Calculate parenthesis level
            context.ParenthesisLevel = CalculateParenthesisLevel(relevantTokens);
            context.InSubquery = context.ParenthesisLevel > 0;

            return context;
        }

        private string DetermineCurrentClause(List<SqlToken> tokens)
        {
            // Find the most recent clause keyword
            var clauseKeywords = new[] { "SELECT", "FROM", "WHERE", "JOIN", "GROUP", "ORDER", "HAVING", "INTO", "UPDATE", "INSERT" };

            var lastClause = tokens
                .Where(t => t.Type == SqlTokenType.Keyword && clauseKeywords.Contains(t.Text.ToUpperInvariant()))
                .LastOrDefault();

            if (lastClause == null)
                return "SELECT"; // Default

            var clause = lastClause.Text.ToUpperInvariant();

            // Handle multi-word clauses
            if (clause == "GROUP" || clause == "ORDER")
            {
                var nextToken = tokens
                    .SkipWhile(t => t != lastClause)
                    .Skip(1)
                    .FirstOrDefault(t => t.Type == SqlTokenType.Keyword);

                if (nextToken?.Text.ToUpperInvariant() == "BY")
                {
                    clause += " BY";
                }
            }

            // Handle JOIN types
            if (clause == "JOIN")
            {
                var prevToken = tokens
                    .TakeWhile(t => t != lastClause)
                    .Where(t => t.Type == SqlTokenType.Keyword)
                    .LastOrDefault();

                if (prevToken != null)
                {
                    var prevKeyword = prevToken.Text.ToUpperInvariant();
                    if (prevKeyword == "INNER" || prevKeyword == "LEFT" || prevKeyword == "RIGHT" ||
                        prevKeyword == "OUTER" || prevKeyword == "CROSS")
                    {
                        clause = $"{prevKeyword} {clause}";
                    }
                }
            }

            return clause;
        }

        private void ExtractTablesAndAliases(List<SqlToken> tokens, SqlContext context)
        {
            // Simple extraction - look for identifiers after FROM or JOIN keywords
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if (token.Type == SqlTokenType.Keyword &&
                    (token.Text.ToUpperInvariant() == "FROM" || token.Text.ToUpperInvariant() == "JOIN"))
                {
                    // Next identifier should be a table name
                    for (int j = i + 1; j < tokens.Count; j++)
                    {
                        if (tokens[j].Type == SqlTokenType.Whitespace || tokens[j].Type == SqlTokenType.Comment)
                            continue;

                        if (tokens[j].Type == SqlTokenType.Identifier)
                        {
                            string tableName = tokens[j].Text;

                            // Skip schema qualifier
                            if (j + 1 < tokens.Count && tokens[j + 1].Text == ".")
                            {
                                j += 2;
                                if (j < tokens.Count && tokens[j].Type == SqlTokenType.Identifier)
                                {
                                    tableName = tokens[j].Text;
                                }
                            }

                            context.Tables.Add(tableName);

                            // Check for alias
                            for (int k = j + 1; k < tokens.Count; k++)
                            {
                                if (tokens[k].Type == SqlTokenType.Whitespace || tokens[k].Type == SqlTokenType.Comment)
                                    continue;

                                if (tokens[k].Type == SqlTokenType.Keyword && tokens[k].Text.ToUpperInvariant() == "AS")
                                {
                                    k++;
                                    while (k < tokens.Count && (tokens[k].Type == SqlTokenType.Whitespace || tokens[k].Type == SqlTokenType.Comment))
                                        k++;
                                }

                                if (tokens[k].Type == SqlTokenType.Identifier)
                                {
                                    context.Aliases[tokens[k].Text] = tableName;
                                }

                                break;
                            }

                            break;
                        }

                        break;
                    }
                }
            }
        }

        private int CalculateParenthesisLevel(List<SqlToken> tokens)
        {
            int level = 0;

            foreach (var token in tokens)
            {
                if (token.Type == SqlTokenType.Punctuation)
                {
                    if (token.Text == "(")
                        level++;
                    else if (token.Text == ")")
                        level--;
                }
            }

            return level;
        }
    }
}
