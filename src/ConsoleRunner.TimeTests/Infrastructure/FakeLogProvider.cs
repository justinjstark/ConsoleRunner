using Microsoft.Extensions.Logging;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeLogProvider : ILoggerProvider
    {
        public readonly ILogger Logger;

        public FakeLogProvider(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return Logger;
        }

        public void Dispose()
        {
        }
    }
}
