using System;
using System.Threading;

namespace ConsoleRunner.MedallionShell
{
    /*
     * Decoupling this from Job was meant to make unit testing easier.
     * It might be better to keep it tightly coupled and unit test with a fake EXE.
     * Mocking out the runner would not allow us to test for things like a missing
     * EXE anyway, the exception for which is not enforced by the interface contract.
     * Stupid exceptions.
     */
    public class MedallionCommandRunner : ICommandRunner
    {
        public Command Run(string executable, TimeSpan? timeout, CancellationToken cancellationToken, params object[] arguments)
        {
            Console.WriteLine(string.Join(", ", arguments));

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

            return new Command
            (
                task: command.Task.ContinueWith(task => new CommandResult
                (
                    exitCode: task.Result.ExitCode,
                    standardOutput: task.Result.StandardOutput,
                    standardError: task.Result.StandardError
                )),
                processId: command.ProcessId
            );
        }
    }
}
