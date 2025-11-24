using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Core.Formatting
{
    public class SqlFormatter
    {
        private readonly FormattingProfile _profile;
        private readonly SqlTokenizer _tokenizer;

        public SqlFormatter(FormattingProfile profile = null)
        {
            _profile = profile ?? FormattingProfile.StandardProfile;
            _tokenizer = new SqlTokenizer();
        }

        public string Format(string sql)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrWhiteSpace(sql))
                    return sql;

                // Tokenize
                var tokens = _tokenizer.Tokenize(sql);

                // Filter out comments for now (can be added back later)
                var filteredTokens = tokens
                    .Where(t => t.Type != SqlTokenType.Comment)
                    .ToList();

                // Format
                var formatted = FormatTokens(filteredTokens);

                stopwatch.Stop();
                Infrastructure.Logger.Instance.Info($"SQL formatted in {stopwatch.ElapsedMilliseconds}ms");

                return formatted;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error formatting SQL: {ex.Message}", ex);
                return sql; // Return original on error
            }
        }

        private string FormatTokens(List<SqlToken> tokens)
        {
            var result = new StringBuilder();
            int indentLevel = 0;
            bool needsNewLine = false;
            bool afterSelect = false;
            int columnCount = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var nextToken = i + 1 < tokens.Count ? tokens[i + 1] : null;
                var prevToken = i > 0 ? tokens[i - 1] : null;

                // Skip whitespace tokens - we'll add our own
                if (token.Type == SqlTokenType.Whitespace)
                    continue;

                // Handle keywords
                if (token.Type == SqlTokenType.Keyword)
                {
                    var keyword = token.Text.ToUpperInvariant();
                    var formattedKeyword = ApplyKeywordCasing(token.Text);

                    // Major clause keywords
                    if (keyword == "SELECT")
                    {
                        if (result.Length > 0)
                            result.AppendLine();
                        result.Append(GetIndent(indentLevel));
                        result.Append(formattedKeyword);
                        afterSelect = true;
                        columnCount = 0;

                        if (_profile.NewLineAfterSelect)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel + 1));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        continue;
                    }
                    else if (keyword == "FROM")
                    {
                        afterSelect = false;
                        if (_profile.NewLineBeforeFrom)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                    else if (keyword == "WHERE")
                    {
                        if (_profile.NewLineBeforeWhere)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                    else if (keyword == "JOIN" || keyword == "INNER" || keyword == "LEFT" || keyword == "RIGHT" || keyword == "OUTER" || keyword == "CROSS")
                    {
                        if (keyword == "JOIN" && _profile.NewLineBeforeJoin)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                    else if (keyword == "ON")
                    {
                        if (_profile.NewLineForJoinConditions)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel + 1));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                    else if (keyword == "GROUP")
                    {
                        if (_profile.NewLineBeforeGroupBy)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                    else if (keyword == "ORDER")
                    {
                        if (_profile.NewLineBeforeOrderBy)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                    else
                    {
                        result.Append(formattedKeyword);
                        result.Append(" ");
                        continue;
                    }
                }

                // Handle punctuation
                if (token.Type == SqlTokenType.Punctuation)
                {
                    if (token.Text == ",")
                    {
                        result.Append(token.Text);

                        if (afterSelect && _profile.AlignColumnLists)
                        {
                            result.AppendLine();
                            result.Append(GetIndent(indentLevel + 1));
                        }
                        else
                        {
                            result.Append(" ");
                        }
                        continue;
                    }
                    else if (token.Text == "(")
                    {
                        indentLevel++;
                        result.Append(token.Text);
                        continue;
                    }
                    else if (token.Text == ")")
                    {
                        indentLevel--;
                        result.Append(token.Text);
                        continue;
                    }
                    else
                    {
                        result.Append(token.Text);
                        continue;
                    }
                }

                // Default: append token
                result.Append(token.Text);

                // Add space if needed
                if (nextToken != null &&
                    nextToken.Type != SqlTokenType.Punctuation &&
                    nextToken.Type != SqlTokenType.Whitespace &&
                    token.Type != SqlTokenType.Punctuation)
                {
                    result.Append(" ");
                }
            }

            return result.ToString().Trim();
        }

        private string ApplyKeywordCasing(string keyword)
        {
            switch (_profile.KeywordCasing)
            {
                case KeywordCase.Uppercase:
                    return keyword.ToUpperInvariant();
                case KeywordCase.Lowercase:
                    return keyword.ToLowerInvariant();
                case KeywordCase.Capitalize:
                    return char.ToUpperInvariant(keyword[0]) + keyword.Substring(1).ToLowerInvariant();
                default:
                    return keyword;
            }
        }

        private string GetIndent(int level)
        {
            if (!_profile.IndentEnabled || level < 0)
                return string.Empty;

            return new string(' ', level * _profile.IndentSize);
        }
    }
}
