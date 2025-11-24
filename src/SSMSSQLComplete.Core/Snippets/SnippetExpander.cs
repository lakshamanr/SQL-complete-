using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SSMSSQLComplete.Core.Snippets
{
    public class SnippetExpander
    {
        private static readonly Regex ParameterRegex = new Regex(@"\$\{(\w+)\}", RegexOptions.Compiled);

        public string Expand(Snippet snippet, Dictionary<string, string> parameterValues = null)
        {
            if (snippet == null)
                return string.Empty;

            var code = snippet.Code;

            if (parameterValues == null || parameterValues.Count == 0)
            {
                // Use default values
                foreach (var param in snippet.Parameters)
                {
                    var placeholder = $"${{{param.Name}}}";
                    code = code.Replace(placeholder, param.DefaultValue);
                }
            }
            else
            {
                // Use provided values
                foreach (var kvp in parameterValues)
                {
                    var placeholder = $"${{{kvp.Key}}}";
                    code = code.Replace(placeholder, kvp.Value);
                }
            }

            return code;
        }

        public List<SnippetField> ExtractFields(Snippet snippet)
        {
            var fields = new List<SnippetField>();

            if (snippet == null)
                return fields;

            var matches = ParameterRegex.Matches(snippet.Code);
            int fieldIndex = 0;

            foreach (Match match in matches)
            {
                var paramName = match.Groups[1].Value;
                var param = snippet.Parameters.Find(p => p.Name == paramName);

                fields.Add(new SnippetField
                {
                    Index = fieldIndex++,
                    Name = paramName,
                    DefaultValue = param?.DefaultValue ?? string.Empty,
                    Start = match.Index,
                    Length = match.Length
                });
            }

            return fields;
        }
    }

    public class SnippetField
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
    }
}
