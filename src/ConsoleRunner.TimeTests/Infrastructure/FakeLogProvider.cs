using Microsoft.Extensions.Logging;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeLogProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public FakeLogProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
