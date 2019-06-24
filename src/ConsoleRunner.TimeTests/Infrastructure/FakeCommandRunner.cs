using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeCommandRunner : ICommandRunner
    {
        public Task<Command> RunAsync(string executable, TimeSpan? timeout, CancellationToken cancellationToken = default, params object[] arguments)
        {
            var duration = arguments.Length > 0
                ? TimeSpan.FromSeconds(int.Parse((string)arguments[0]))
                : TimeSpan.Zero;

            return Task.FromResult(new Command(
                task: Task.Delay(duration).ContinueWith(t => new CommandResult(
                    exitCode: 0,
                    standardOutput: "",
                    standardError: "")),
                processId: 1
            ));
        }
    }
}
