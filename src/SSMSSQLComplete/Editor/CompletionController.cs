using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using SSMSSQLComplete.Core.Completion;
using SSMSSQLComplete.Core.Infrastructure;

namespace SSMSSQLComplete.Editor
{
    internal sealed class CompletionController
    {
        private readonly ITextView _textView;
        private readonly ICompletionBroker _broker;
        private readonly CompletionEngine _engine;
        private ICompletionSession _session;

        public CompletionController(ITextView textView, ICompletionBroker broker)
        {
            _textView = textView;
            _broker = broker;
            _engine = new CompletionEngine();
        }

        public void TriggerCompletion(int position, CompletionTrigger trigger)
        {
            try
            {
                using (var perfMon = PerformanceMonitor.Start("TriggerCompletion"))
                {
                    if (_session != null && !_session.IsDismissed)
                    {
                        _session.Filter();
                        return;
                    }

                    var snapshot = _textView.TextBuffer.CurrentSnapshot;
                    var trackingPoint = snapshot.CreateTrackingPoint(position, PointTrackingMode.Positive);

                    _session = _broker.CreateCompletionSession(_textView, trackingPoint, true);

                    if (_session != null)
                    {
                        _session.Dismissed += OnSessionDismissed;
                        _session.Committed += OnSessionCommitted;
                        _session.Start();

                        perfMon.RecordMetric("CompletionTime");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in TriggerCompletion: {ex.Message}", ex);
            }
        }

        public void DismissCompletion()
        {
            if (_session != null && !_session.IsDismissed)
            {
                _session.Dismiss();
            }
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            if (_session != null)
            {
                _session.Dismissed -= OnSessionDismissed;
                _session.Committed -= OnSessionCommitted;
                _session = null;
            }
        }

        private void OnSessionCommitted(object sender, EventArgs e)
        {
            var session = sender as ICompletionSession;
            if (session?.SelectedCompletionSet?.SelectionStatus?.Completion != null)
            {
                var completion = session.SelectedCompletionSet.SelectionStatus.Completion;
                TelemetryService.Instance.TrackEvent("CompletionCommitted", new Dictionary<string, string>
                {
                    { "DisplayText", completion.DisplayText },
                    { "Type", completion.GetType().Name }
                });
            }
        }
    }
}
