using System;
using System.Threading;

namespace ConsoleRunner
{
    public interface ICommandRunner
    {
        Command Run(string executable, TimeSpan? timeout, CancellationToken cancellationToken = default, params object[] arguments);
    }
}
