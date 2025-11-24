using System.Collections.Generic;

namespace SSMSSQLComplete.Core.Snippets
{
    public interface ISnippetProvider
    {
        IEnumerable<Snippet> GetAllSnippets();
        Snippet GetSnippet(string id);
        Snippet GetSnippetByShortcut(string shortcut);
        void AddSnippet(Snippet snippet);
        void UpdateSnippet(Snippet snippet);
        void DeleteSnippet(string id);
        void SaveSnippets();
    }
}
