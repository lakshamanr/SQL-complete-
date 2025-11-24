using System.Collections.Generic;

namespace SSMSSQLComplete.Core.Snippets
{
    public class Snippet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Shortcut { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public List<SnippetParameter> Parameters { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }

        public Snippet()
        {
            Parameters = new List<SnippetParameter>();
            Id = System.Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"{Shortcut}: {Name}";
        }
    }
}
