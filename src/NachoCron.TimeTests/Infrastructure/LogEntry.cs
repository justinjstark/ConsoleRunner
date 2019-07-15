using Microsoft.Extensions.Logging;
using System;

namespace NachoCron.TimeTests.Infrastructure
{
    public class LogEntry
    {
        public DateTime Time { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
