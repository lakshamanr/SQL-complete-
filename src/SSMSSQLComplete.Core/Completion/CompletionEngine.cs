using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Core.Completion
{
    public class CompletionEngine
    {
        private readonly List<ICompletionProvider> _providers;
        private readonly CompletionRanker _ranker;
        private readonly SqlContextAnalyzer _contextAnalyzer;

        public CompletionEngine()
        {
            _providers = new List<ICompletionProvider>
            {
                new KeywordCompletionProvider(),
                new SchemaCompletionProvider()
            };
            _ranker = new CompletionRanker();
            _contextAnalyzer = new SqlContextAnalyzer();
        }

        public async Task<IEnumerable<CompletionItem>> GetCompletionsAsync(
            string text,
            int position,
            CompletionTrigger trigger)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Analyze SQL context
                var sqlContext = _contextAnalyzer.AnalyzeContext(text, position);

                // Create completion context
                var context = new CompletionContext
                {
                    Text = text,
                    Position = position,
                    Trigger = trigger,
                    SqlContext = sqlContext
                };

                // Gather completions from all providers
                var completionTasks = _providers
                    .Where(p => p.CanProvideCompletions(context))
                    .Select(p => p.GetCompletionsAsync(context));

                var completionSets = await Task.WhenAll(completionTasks);
                var allCompletions = completionSets.SelectMany(c => c).ToList();

                // Rank and filter completions
                var rankedCompletions = _ranker.RankCompletions(allCompletions, context);

                stopwatch.Stop();
                Infrastructure.Logger.Instance.Info(
                    $"Completion generated {rankedCompletions.Count()} items in {stopwatch.ElapsedMilliseconds}ms");

                return rankedCompletions;
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error getting completions: {ex.Message}", ex);
                return Enumerable.Empty<CompletionItem>();
            }
        }

        public void AddProvider(ICompletionProvider provider)
        {
            _providers.Add(provider);
        }

        public void RemoveProvider(ICompletionProvider provider)
        {
            _providers.Remove(provider);
        }
    }
}
