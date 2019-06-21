using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.Persistence
{
    public interface ICronJobsRepository
    {
        Task<IEnumerable<CronJob>> GetJobsAsync();
    }
}
