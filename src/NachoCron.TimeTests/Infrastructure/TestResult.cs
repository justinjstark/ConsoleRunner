using System.Collections.Concurrent;

namespace NachoCron.TimeTests.Infrastructure
{
    public class TestResult
    {
        public ConcurrentBag<LogEntry> LogEntries { get; set; } = new ConcurrentBag<LogEntry>();

        public ConcurrentBag<Command> Commands { get; set; } = new ConcurrentBag<Command>();

        public bool TimedOut { get; set; }
    }
}
