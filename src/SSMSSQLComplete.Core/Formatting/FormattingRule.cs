using System;

namespace SSMSSQLComplete.Core.Formatting
{
    public abstract class FormattingRule
    {
        public abstract string Apply(string sql, FormattingProfile profile);

        protected string ApplyKeywordCasing(string keyword, KeywordCase casing)
        {
            switch (casing)
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

        protected string GetIndent(int level, FormattingProfile profile)
        {
            if (!profile.IndentEnabled)
                return string.Empty;

            return new string(' ', level * profile.IndentSize);
        }
    }

    public class KeywordCasingRule : FormattingRule
    {
        public override string Apply(string sql, FormattingProfile profile)
        {
            var tokenizer = new Parsing.SqlTokenizer();
            var tokens = tokenizer.Tokenize(sql);

            var result = new System.Text.StringBuilder();

            foreach (var token in tokens)
            {
                if (token.Type == Parsing.SqlTokenType.Keyword)
                {
                    result.Append(ApplyKeywordCasing(token.Text, profile.KeywordCasing));
                }
                else
                {
                    result.Append(token.Text);
                }
            }

            return result.ToString();
        }
    }
}
