using System.Collections.Generic;

namespace SSMSSQLComplete.Core.Completion
{
    public class CompletionItem
    {
        public string Label { get; set; }
        public string InsertText { get; set; }
        public string Detail { get; set; }
        public string Documentation { get; set; }
        public CompletionItemKind Kind { get; set; }
        public int SortPriority { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public CompletionItem()
        {
            Metadata = new Dictionary<string, object>();
            SortPriority = 100;
        }

        public CompletionItem(string label, CompletionItemKind kind) : this()
        {
            Label = label;
            InsertText = label;
            Kind = kind;
        }

        public CompletionItem(string label, string insertText, CompletionItemKind kind) : this()
        {
            Label = label;
            InsertText = insertText;
            Kind = kind;
        }

        public override string ToString()
        {
            return $"{Label} ({Kind})";
        }
    }
}
