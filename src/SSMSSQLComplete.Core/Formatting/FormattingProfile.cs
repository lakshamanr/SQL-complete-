namespace SSMSSQLComplete.Core.Formatting
{
    public class FormattingProfile
    {
        public string Name { get; set; }
        public KeywordCase KeywordCasing { get; set; }
        public IdentifierCase IdentifierCasing { get; set; }
        public bool IndentEnabled { get; set; }
        public int IndentSize { get; set; }
        public bool AlignColumnLists { get; set; }
        public bool BreakBeforeComma { get; set; }
        public bool NewLineAfterSelect { get; set; }
        public bool NewLineBeforeFrom { get; set; }
        public bool NewLineBeforeJoin { get; set; }
        public bool NewLineBeforeWhere { get; set; }
        public bool NewLineBeforeGroupBy { get; set; }
        public bool NewLineBeforeOrderBy { get; set; }
        public bool NewLineForJoinConditions { get; set; }
        public int MaxLineLength { get; set; }

        public FormattingProfile()
        {
            Name = "Default";
            KeywordCasing = KeywordCase.Uppercase;
            IdentifierCasing = IdentifierCase.AsIs;
            IndentEnabled = true;
            IndentSize = 4;
            AlignColumnLists = true;
            BreakBeforeComma = false;
            NewLineAfterSelect = true;
            NewLineBeforeFrom = true;
            NewLineBeforeJoin = true;
            NewLineBeforeWhere = true;
            NewLineBeforeGroupBy = true;
            NewLineBeforeOrderBy = true;
            NewLineForJoinConditions = true;
            MaxLineLength = 120;
        }

        public static FormattingProfile StandardProfile => new FormattingProfile
        {
            Name = "Standard",
            KeywordCasing = KeywordCase.Uppercase,
            IndentEnabled = true,
            IndentSize = 4
        };

        public static FormattingProfile CompactProfile => new FormattingProfile
        {
            Name = "Compact",
            KeywordCasing = KeywordCase.Uppercase,
            IndentEnabled = false,
            NewLineAfterSelect = false,
            NewLineBeforeFrom = false,
            NewLineBeforeJoin = false,
            NewLineBeforeWhere = false
        };
    }

    public enum KeywordCase
    {
        AsIs,
        Uppercase,
        Lowercase,
        Capitalize
    }

    public enum IdentifierCase
    {
        AsIs,
        Uppercase,
        Lowercase
    }
}
