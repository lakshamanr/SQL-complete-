using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using SSMSSQLComplete.Core.Completion;
using SSMSSQLComplete.Core.Infrastructure;

namespace SSMSSQLComplete.Editor
{
    internal sealed class SqlCompletionCommandHandler
    {
        private readonly ITextView _textView;
        private readonly ICompletionBroker _completionBroker;
        private readonly ITextStructureNavigatorSelectorService _navigatorService;
        private ICompletionSession _currentSession;
        private readonly CompletionEngine _completionEngine;

        public SqlCompletionCommandHandler(
            ITextView textView,
            ICompletionBroker completionBroker,
            ITextStructureNavigatorSelectorService navigatorService)
        {
            _textView = textView;
            _completionBroker = completionBroker;
            _navigatorService = navigatorService;
            _completionEngine = new CompletionEngine();

            // Subscribe to text changes
            _textView.TextBuffer.Changed += OnTextBufferChanged;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            try
            {
                if (e.Changes.Count == 0)
                    return;

                var change = e.Changes[0];
                var newText = change.NewText;

                // Check if we should trigger completion
                if (ShouldTriggerCompletion(newText))
                {
                    TriggerCompletion();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in OnTextBufferChanged: {ex.Message}", ex);
            }
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // Dismiss completion if caret moves outside the applicable span
            if (_currentSession != null && !_currentSession.IsDismissed)
            {
                var position = e.NewPosition.BufferPosition;
                var applicableSpan = _currentSession.SelectedCompletionSet?.ApplicableTo;

                if (applicableSpan != null)
                {
                    var span = applicableSpan.GetSpan(e.NewPosition.BufferPosition.Snapshot);
                    if (!span.Contains(position.Position))
                    {
                        _currentSession.Dismiss();
                    }
                }
            }
        }

        private bool ShouldTriggerCompletion(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Trigger on dot, space, or alphanumeric characters
            char lastChar = text[text.Length - 1];
            return lastChar == '.' ||
                   lastChar == ' ' ||
                   char.IsLetterOrDigit(lastChar);
        }

        private void TriggerCompletion()
        {
            try
            {
                if (_currentSession != null && !_currentSession.IsDismissed)
                    return;

                var position = _textView.Caret.Position.BufferPosition;
                var snapshot = position.Snapshot;

                _currentSession = _completionBroker.CreateCompletionSession(
                    _textView,
                    snapshot.CreateTrackingPoint(position, PointTrackingMode.Positive),
                    true);

                if (_currentSession != null)
                {
                    _currentSession.Dismissed += OnSessionDismissed;
                    _currentSession.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error triggering completion: {ex.Message}", ex);
            }
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            if (_currentSession != null)
            {
                _currentSession.Dismissed -= OnSessionDismissed;
                _currentSession = null;
            }
        }
    }
}
