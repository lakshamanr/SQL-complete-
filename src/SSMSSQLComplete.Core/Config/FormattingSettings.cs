using SSMSSQLComplete.Core.Formatting;

namespace SSMSSQLComplete.Core.Config
{
    public class FormattingSettings
    {
        public string ActiveProfile { get; set; } = "Standard";
        public KeywordCase KeywordCasing { get; set; } = KeywordCase.Uppercase;
        public bool IndentEnabled { get; set; } = true;
        public int IndentSize { get; set; } = 4;
        public bool AlignColumnLists { get; set; } = true;
    }
}
