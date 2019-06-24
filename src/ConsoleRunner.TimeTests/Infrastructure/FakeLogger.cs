using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeLogger : ILogger
    {
        public Func<LogEntry, ConcurrentBag<LogEntry>, bool> LogEntriesPredicate;
        public ConcurrentBag<LogEntry> LogEntries = new ConcurrentBag<LogEntry>();

        public FakeLogger(Func<LogEntry, ConcurrentBag<LogEntry>, bool> logEntriesPredicate)
        {
            LogEntriesPredicate = logEntriesPredicate;
        }        

        public TaskCompletionSource<LogEntry> TaskCompletionSource = new TaskCompletionSource<LogEntry>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logEntry = new LogEntry
            {
                Time = DateTime.Now,
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            };

            LogEntries.Add(logEntry);
            
            if (LogEntriesPredicate != null && LogEntriesPredicate(logEntry, LogEntries))
            {
                TaskCompletionSource.TrySetResult(logEntry);
            }
        }

        private class NullDisposable : IDisposable
        {
            internal static readonly IDisposable Instance = new NullDisposable();

            public void Dispose()
            {
            }
        }
    }
}
