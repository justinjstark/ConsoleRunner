using ConsoleRunner.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeCronJobsRepository : ICronJobRepository
    {
        public List<CronJob> CronJobs { get; set; }

        public FakeCronJobsRepository(List<CronJob> cronJobs)
        {
            CronJobs = cronJobs;
        }

        public Task<IEnumerable<CronJob>> GetCronJobsAsync()
        {
            return Task.FromResult((IEnumerable<CronJob>)CronJobs);
        }
    }
}
