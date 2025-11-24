using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SSMSSQLComplete.Core.Completion;
using SSMSSQLComplete.Core.Infrastructure;

namespace SSMSSQLComplete.Editor
{
    [Export(typeof(ICompletionSourceProvider))]
    [Microsoft.VisualStudio.Utilities.ContentType("SQL")]
    [Microsoft.VisualStudio.Utilities.ContentType("TSQL")]
    [Microsoft.VisualStudio.Utilities.Name("SQL Completion Source")]
    internal class SqlCompletionSourceProvider : ICompletionSourceProvider
    {
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                () => new SqlCompletionSource(textBuffer));
        }
    }

    internal class SqlCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly CompletionEngine _engine;
        private bool _disposed;

        public SqlCompletionSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _engine = new CompletionEngine();
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            try
            {
                if (_disposed)
                    return;

                var snapshot = _textBuffer.CurrentSnapshot;
                var triggerPoint = session.GetTriggerPoint(snapshot);

                if (!triggerPoint.HasValue)
                    return;

                int position = triggerPoint.Value.Position;
                string text = snapshot.GetText();

                // Get completions from engine asynchronously
                var completionsTask = _engine.GetCompletionsAsync(
                    text,
                    position,
                    new CompletionTrigger { TriggerKind = CompletionTriggerKind.Invoke });

                // Wait for completions (with timeout)
                if (!completionsTask.Wait(TimeSpan.FromSeconds(2)))
                {
                    Logger.Instance.Warn("Completion timed out after 2 seconds");
                    return;
                }

                var completionItems = completionsTask.Result;

                if (!completionItems.Any())
                    return;

                // Convert Core completion items to VS SDK Completion objects
                var vsCompletions = completionItems.Select(item =>
                    new Microsoft.VisualStudio.Language.Intellisense.Completion(
                        displayText: item.Label,
                        insertionText: item.InsertText ?? item.Label,
                        description: item.Detail ?? string.Empty,
                        iconSource: null,
                        iconAutomationText: item.Kind.ToString()
                    )).ToList();

                // Get the word at the trigger point for applicability tracking
                var trackingSpan = GetTrackingSpan(snapshot, position);

                var completionSet = new CompletionSet(
                    moniker: "SQL",
                    displayName: "SQL Completion",
                    applicableTo: trackingSpan,
                    completions: vsCompletions,
                    completionBuilders: null);

                completionSets.Add(completionSet);

                Logger.Instance.Info($"Provided {vsCompletions.Count} completions to VS SDK");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in AugmentCompletionSession: {ex.Message}", ex);
            }
        }

        private ITrackingSpan GetTrackingSpan(ITextSnapshot snapshot, int position)
        {
            // Find the start of the current word
            int start = position;
            while (start > 0 && IsIdentifierChar(snapshot[start - 1]))
            {
                start--;
            }

            // Create tracking span from start of word to current position
            var span = new SnapshotSpan(snapshot, start, position - start);
            return snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
        }

        private bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '@' || c == '#';
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
