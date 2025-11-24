using System.Collections.Generic;
using System.Text;

namespace SSMSSQLComplete.Core.Parsing
{
    public class SqlTokenizer
    {
        public List<SqlToken> Tokenize(string sql)
        {
            var tokens = new List<SqlToken>();
            if (string.IsNullOrEmpty(sql))
                return tokens;

            int position = 0;
            while (position < sql.Length)
            {
                char current = sql[position];

                // Whitespace
                if (char.IsWhiteSpace(current))
                {
                    int start = position;
                    while (position < sql.Length && char.IsWhiteSpace(sql[position]))
                        position++;
                    tokens.Add(new SqlToken(SqlTokenType.Whitespace, sql.Substring(start, position - start), start));
                    continue;
                }

                // Comment -- (single line)
                if (current == '-' && position + 1 < sql.Length && sql[position + 1] == '-')
                {
                    int start = position;
                    while (position < sql.Length && sql[position] != '\n')
                        position++;
                    tokens.Add(new SqlToken(SqlTokenType.Comment, sql.Substring(start, position - start), start));
                    continue;
                }

                // Comment /* */ (multi-line)
                if (current == '/' && position + 1 < sql.Length && sql[position + 1] == '*')
                {
                    int start = position;
                    position += 2;
                    while (position < sql.Length - 1)
                    {
                        if (sql[position] == '*' && sql[position + 1] == '/')
                        {
                            position += 2;
                            break;
                        }
                        position++;
                    }
                    tokens.Add(new SqlToken(SqlTokenType.Comment, sql.Substring(start, position - start), start));
                    continue;
                }

                // String literal
                if (current == '\'')
                {
                    int start = position;
                    position++;
                    while (position < sql.Length)
                    {
                        if (sql[position] == '\'')
                        {
                            position++;
                            // Check for escaped quote ''
                            if (position < sql.Length && sql[position] == '\'')
                            {
                                position++;
                                continue;
                            }
                            break;
                        }
                        position++;
                    }
                    tokens.Add(new SqlToken(SqlTokenType.StringLiteral, sql.Substring(start, position - start), start));
                    continue;
                }

                // Number literal
                if (char.IsDigit(current))
                {
                    int start = position;
                    while (position < sql.Length && (char.IsDigit(sql[position]) || sql[position] == '.'))
                        position++;
                    tokens.Add(new SqlToken(SqlTokenType.NumberLiteral, sql.Substring(start, position - start), start));
                    continue;
                }

                // Identifier or Keyword
                if (char.IsLetter(current) || current == '_' || current == '@' || current == '#')
                {
                    int start = position;
                    while (position < sql.Length &&
                           (char.IsLetterOrDigit(sql[position]) || sql[position] == '_' || sql[position] == '@' || sql[position] == '#'))
                        position++;

                    string word = sql.Substring(start, position - start);
                    var tokenType = TSqlKeywords.IsKeyword(word) ? SqlTokenType.Keyword : SqlTokenType.Identifier;
                    tokens.Add(new SqlToken(tokenType, word, start));
                    continue;
                }

                // Bracketed identifier [table name]
                if (current == '[')
                {
                    int start = position;
                    position++;
                    while (position < sql.Length && sql[position] != ']')
                        position++;
                    if (position < sql.Length)
                        position++; // Include closing bracket
                    tokens.Add(new SqlToken(SqlTokenType.Identifier, sql.Substring(start, position - start), start));
                    continue;
                }

                // Operators
                if (IsOperatorChar(current))
                {
                    int start = position;
                    while (position < sql.Length && IsOperatorChar(sql[position]))
                        position++;
                    tokens.Add(new SqlToken(SqlTokenType.Operator, sql.Substring(start, position - start), start));
                    continue;
                }

                // Punctuation
                if (IsPunctuationChar(current))
                {
                    tokens.Add(new SqlToken(SqlTokenType.Punctuation, current.ToString(), position));
                    position++;
                    continue;
                }

                // Unknown
                tokens.Add(new SqlToken(SqlTokenType.Unknown, current.ToString(), position));
                position++;
            }

            return tokens;
        }

        private bool IsOperatorChar(char c)
        {
            return c == '=' || c == '>' || c == '<' || c == '!' || c == '+' || c == '-' || c == '*' || c == '/' || c == '%';
        }

        private bool IsPunctuationChar(char c)
        {
            return c == '(' || c == ')' || c == ',' || c == ';' || c == '.';
        }
    }
}
