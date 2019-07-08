using System.Collections.Generic;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class TestResult
    {
        public List<LogEntry> LogEntries { get; set; }

        public List<Command> Commands { get; set; }

        public bool TimedOut { get; set; }
    }
}
