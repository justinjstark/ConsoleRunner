using System.Collections.Generic;
using System.Threading.Tasks;

namespace NachoCron.Persistence
{
    public interface ICronJobRepository
    {
        Task<IEnumerable<CronJob>> GetCronJobsAsync();
    }
}
