namespace SSMSSQLComplete.Core.Config
{
    public class CompletionSettings
    {
        public bool Enabled { get; set; } = true;
        public bool AutoTrigger { get; set; } = true;
        public int TriggerDelay { get; set; } = 100; // milliseconds
        public bool FuzzyMatching { get; set; } = true;
        public bool ShowKeywords { get; set; } = true;
        public bool ShowTables { get; set; } = true;
        public bool ShowColumns { get; set; } = true;
        public bool ShowSnippets { get; set; } = true;
        public int MaxSuggestions { get; set; } = 50;
    }
}
