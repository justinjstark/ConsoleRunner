using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner
{
    public interface ICommandRunner
    {
        Task<Command> RunAsync(string executable, TimeSpan? timeout, CancellationToken cancellationToken = default, params object[] arguments);
    }
}
