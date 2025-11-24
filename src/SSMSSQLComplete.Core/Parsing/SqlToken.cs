namespace SSMSSQLComplete.Core.Parsing
{
    public enum SqlTokenType
    {
        Keyword,
        Identifier,
        StringLiteral,
        NumberLiteral,
        Operator,
        Punctuation,
        Whitespace,
        Comment,
        Unknown
    }

    public class SqlToken
    {
        public SqlTokenType Type { get; set; }
        public string Text { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        public int End => Start + Length;

        public SqlToken(SqlTokenType type, string text, int start)
        {
            Type = type;
            Text = text;
            Start = start;
            Length = text?.Length ?? 0;
        }

        public override string ToString()
        {
            return $"{Type}: {Text} [{Start}-{End}]";
        }
    }
}
