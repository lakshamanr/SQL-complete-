using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Infrastructure
{
    public sealed class TelemetryService
    {
        private static readonly Lazy<TelemetryService> _instance =
            new Lazy<TelemetryService>(() => new TelemetryService());

        public static TelemetryService Instance => _instance.Value;

        private readonly BlockingCollection<TelemetryEvent> _eventQueue;
        private readonly Task _processingTask;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isEnabled;

        private TelemetryService()
        {
            _eventQueue = new BlockingCollection<TelemetryEvent>();
            _cancellationTokenSource = new CancellationTokenSource();
            _isEnabled = LoadTelemetrySettings();

            // Start background processing
            _processingTask = Task.Run(() => ProcessEvents(_cancellationTokenSource.Token));
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                SaveTelemetrySettings(value);
            }
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            if (!_isEnabled)
                return;

            try
            {
                _eventQueue.Add(new TelemetryEvent
                {
                    Name = eventName,
                    Properties = properties ?? new Dictionary<string, string>(),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                // Ignore telemetry failures
            }
        }

        public void TrackException(Exception exception, Dictionary<string, string> properties = null)
        {
            if (!_isEnabled)
                return;

            var props = properties ?? new Dictionary<string, string>();
            props["ExceptionType"] = exception.GetType().Name;
            props["ExceptionMessage"] = exception.Message;

            TrackEvent("Exception", props);
        }

        private void ProcessEvents(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var evt in _eventQueue.GetConsumingEnumerable(cancellationToken))
                {
                    try
                    {
                        // In a real implementation, this would send to a telemetry backend
                        // For now, we just log it
                        Logger.Instance.Debug($"Telemetry: {evt.Name} - {string.Join(", ", evt.Properties)}");
                    }
                    catch
                    {
                        // Ignore individual processing failures
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when shutting down
            }
        }

        public void Flush()
        {
            _eventQueue.CompleteAdding();
            try
            {
                _processingTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore
            }
        }

        private bool LoadTelemetrySettings()
        {
            try
            {
                var settingsManager = Config.SettingsManager.Instance;
                var telemetrySettings = settingsManager.GetTelemetrySettings();
                return telemetrySettings.Enabled;
            }
            catch
            {
                return true; // Default to enabled
            }
        }

        private void SaveTelemetrySettings(bool enabled)
        {
            try
            {
                var settingsManager = Config.SettingsManager.Instance;
                var telemetrySettings = settingsManager.GetTelemetrySettings();
                telemetrySettings.Enabled = enabled;
                settingsManager.SaveTelemetrySettings(telemetrySettings);
            }
            catch
            {
                // Ignore save failures
            }
        }

        private class TelemetryEvent
        {
            public string Name { get; set; }
            public Dictionary<string, string> Properties { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
