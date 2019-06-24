using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner.CommandRunners.MedallionShell
{
    public class MedallionCommandRunner : ICommandRunner
    {
        public Task<Command> RunAsync(string executable, TimeSpan? timeout, CancellationToken cancellationToken, params object[] arguments)
        {
            var command = Medallion.Shell.Command.Run(
                executable: executable,
                arguments: arguments,
                options =>
                {
                    options.CancellationToken(cancellationToken);
                    if (timeout != null)
                    {
                        options.Timeout(timeout.Value);
                    }
                }
            );

            return Task.FromResult(new Command(
                task: command.Task.ContinueWith(task => new CommandResult(
                    exitCode: task.Result.ExitCode,
                    standardOutput: task.Result.StandardOutput,
                    standardError: task.Result.StandardError)),
                processId: command.ProcessId
            ));
        }
    }
}
