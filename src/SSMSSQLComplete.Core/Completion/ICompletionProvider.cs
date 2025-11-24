using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Completion
{
    public interface ICompletionProvider
    {
        /// <summary>
        /// Gets completion items for the given context
        /// </summary>
        Task<IEnumerable<CompletionItem>> GetCompletionsAsync(CompletionContext context);

        /// <summary>
        /// Determines if this provider can provide completions for the given context
        /// </summary>
        bool CanProvideCompletions(CompletionContext context);
    }

    public class CompletionContext
    {
        public string Text { get; set; }
        public int Position { get; set; }
        public CompletionTrigger Trigger { get; set; }
        public Parsing.SqlContext SqlContext { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
    }
}
