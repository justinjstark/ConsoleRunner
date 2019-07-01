using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeCommandRunner : ICommandRunner
    {
        public readonly AsyncProducerConsumerQueue<object> _queue;

        public FakeCommandRunner(AsyncProducerConsumerQueue<object> queue)
        {
            _queue = queue;
        }

        public Task<ConsoleRunner.Command> RunAsync(string executable, TimeSpan? timeout, CancellationToken cancellationToken = default, params object[] arguments)
        {
            _queue.Enqueue(new Command
            {
                DateTime = DateTime.Now,
                Executable = executable
            });

            var duration = arguments.Length > 0
                ? TimeSpan.FromSeconds(int.Parse((string)arguments[0]))
                : TimeSpan.Zero;

            return Task.FromResult(new ConsoleRunner.Command(
                task: Task.Delay(duration).ContinueWith(t => new CommandResult(
                    exitCode: 0,
                    standardOutput: "",
                    standardError: "")),
                processId: 1
            ));
        }
    }
}
