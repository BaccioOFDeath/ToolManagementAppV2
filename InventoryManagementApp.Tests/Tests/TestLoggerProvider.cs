using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace InventoryManagementApp.Tests
{
    // In-memory logger used to capture log output in unit tests.
    public record LogEntry(LogLevel Level, string Message, Exception? Exception);

    public class ListLoggerProvider : ILoggerProvider
    {
        private readonly List<LogEntry> _logs;
        public ListLoggerProvider(List<LogEntry> logs) => _logs = logs;
        public ILogger CreateLogger(string categoryName) => new ListLogger(_logs);
        public void Dispose() { }

        private class ListLogger : ILogger
        {
            private readonly List<LogEntry> _logs;
            public ListLogger(List<LogEntry> logs) => _logs = logs;
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _logs.Add(new LogEntry(logLevel, formatter(state, exception), exception));
            }

            private class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose() { }
            }
        }
    }
}
