using System.Collections.Generic;

namespace NachoCron.TimeTests.Helpers
{
    public static class CommandArguments
    {
        public static IEnumerable<string> CommandDurationInSeconds(int seconds) => new[] { seconds.ToString() };
    }
}
