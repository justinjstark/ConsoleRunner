using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeLogger : ILogger
    {
        public readonly AsyncProducerConsumerQueue<object> _queue;

        public FakeLogger(AsyncProducerConsumerQueue<object> queue)
        {
            _queue = queue;
        }        

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
            _queue.Enqueue(new LogEntry
            {
                Time = DateTime.Now,
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
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
