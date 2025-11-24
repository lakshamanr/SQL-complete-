namespace SSMSSQLComplete.Core.Snippets
{
    public class SnippetParameter
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }

        public SnippetParameter()
        {
        }

        public SnippetParameter(string name, string defaultValue = "", string description = "")
        {
            Name = name;
            DefaultValue = defaultValue;
            Description = description;
        }

        public override string ToString()
        {
            return $"${{{Name}}}";
        }
    }
}
