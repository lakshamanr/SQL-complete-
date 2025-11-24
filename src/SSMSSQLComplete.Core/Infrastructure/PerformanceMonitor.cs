using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SSMSSQLComplete.Core.Infrastructure
{
    public class PerformanceMonitor : IDisposable
    {
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly Dictionary<string, double> _metrics;

        private PerformanceMonitor(string operationName)
        {
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
            _metrics = new Dictionary<string, double>();
        }

        public static PerformanceMonitor Start(string operationName)
        {
            return new PerformanceMonitor(operationName);
        }

        public void RecordMetric(string metricName, double value = 0)
        {
            _metrics[metricName] = value == 0 ? _stopwatch.Elapsed.TotalMilliseconds : value;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var duration = _stopwatch.Elapsed.TotalMilliseconds;

            // Log performance data
            Logger.Instance.Debug($"Performance [{_operationName}]: {duration:F2}ms");

            // Track telemetry
            var properties = new Dictionary<string, string>
            {
                { "Operation", _operationName },
                { "Duration", duration.ToString("F2") }
            };

            foreach (var metric in _metrics)
            {
                properties[metric.Key] = metric.Value.ToString("F2");
            }

            TelemetryService.Instance.TrackEvent("Performance", properties);

            // Warn if operation is slow
            if (duration > 1000) // More than 1 second
            {
                Logger.Instance.Warning($"Slow operation detected: {_operationName} took {duration:F2}ms");
            }
        }
    }
}
