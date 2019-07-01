using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.Persistence
{
    public interface ICronJobRepository
    {
        Task<IEnumerable<CronJob>> GetCronJobsAsync();
    }
}
