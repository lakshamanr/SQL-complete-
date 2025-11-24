using System;
using System.Collections.Generic;
using System.Linq;

namespace SSMSSQLComplete.Core.Snippets
{
    public sealed class SnippetManager
    {
        private static readonly Lazy<SnippetManager> _instance =
            new Lazy<SnippetManager>(() => new SnippetManager());

        public static SnippetManager Instance => _instance.Value;

        private readonly ISnippetProvider _provider;
        private readonly SnippetExpander _expander;

        private SnippetManager()
        {
            _provider = new SnippetRepository();
            _expander = new SnippetExpander();
        }

        public IEnumerable<Snippet> GetAllSnippets()
        {
            return _provider.GetAllSnippets();
        }

        public Snippet GetSnippet(string id)
        {
            return _provider.GetSnippet(id);
        }

        public Snippet GetSnippetByShortcut(string shortcut)
        {
            return _provider.GetSnippetByShortcut(shortcut);
        }

        public void AddSnippet(Snippet snippet)
        {
            _provider.AddSnippet(snippet);

            Infrastructure.TelemetryService.Instance.TrackEvent("SnippetAdded", new Dictionary<string, string>
            {
                { "Name", snippet.Name },
                { "Category", snippet.Category }
            });
        }

        public void UpdateSnippet(Snippet snippet)
        {
            _provider.UpdateSnippet(snippet);
        }

        public void DeleteSnippet(string id)
        {
            _provider.DeleteSnippet(id);
        }

        public string ExpandSnippet(string shortcut, Dictionary<string, string> parameterValues = null)
        {
            var snippet = _provider.GetSnippetByShortcut(shortcut);
            if (snippet == null)
                return null;

            return _expander.Expand(snippet, parameterValues);
        }

        public string ExpandSnippet(Snippet snippet, Dictionary<string, string> parameterValues = null)
        {
            return _expander.Expand(snippet, parameterValues);
        }

        public List<SnippetField> GetSnippetFields(Snippet snippet)
        {
            return _expander.ExtractFields(snippet);
        }

        public IEnumerable<string> GetCategories()
        {
            return _provider.GetAllSnippets()
                .Select(s => s.Category)
                .Distinct()
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .OrderBy(c => c);
        }
    }
}
