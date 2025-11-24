using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SSMSSQLComplete.Core.Infrastructure
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly string _logFilePath;
        private readonly BlockingCollection<LogEntry> _logQueue;
        private readonly Task _logTask;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Logger()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SSMSSQLComplete",
                "Logs");

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            _logFilePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");
            _logQueue = new BlockingCollection<LogEntry>();
            _cancellationTokenSource = new CancellationTokenSource();

            // Start background logging task
            _logTask = Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void Error(string message, Exception ex = null)
        {
            var fullMessage = ex != null ? $"{message}\n{ex}" : message;
            Log(LogLevel.Error, fullMessage);
        }

        public void Debug(string message)
        {
#if DEBUG
            Log(LogLevel.Debug, message);
#endif
        }

        private void Log(LogLevel level, string message)
        {
            try
            {
                _logQueue.Add(new LogEntry
                {
                    Level = level,
                    Message = message,
                    Timestamp = DateTime.Now
                });
            }
            catch
            {
                // Ignore logging failures
            }
        }

        private void ProcessLogQueue(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var entry in _logQueue.GetConsumingEnumerable(cancellationToken))
                {
                    try
                    {
                        var logLine = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}";
                        File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                    }
                    catch
                    {
                        // Ignore individual write failures
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
            _logQueue.CompleteAdding();
            try
            {
                _logTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore
            }
        }

        private class LogEntry
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }
    }
}
