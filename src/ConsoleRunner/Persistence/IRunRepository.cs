using System;
using System.Threading.Tasks;

namespace ConsoleRunner.Persistence
{
    public interface IRunRepository
    {
        Task WriteRunAsync(Guid cronJobId);

        Task WriteRunResultAsync(Guid cronJobId, int exitCode, string standardOutput, string standardError);
    }
}
