using System.Collections.Generic;

namespace ConsoleRunner.TimeTests.Helpers
{
    public static class CommandArguments
    {
        public static IEnumerable<string> CommandDurationInSeconds(int seconds) => new[] { seconds.ToString() };
    }
}
