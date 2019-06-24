using ConsoleRunner.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class FakeCronJobsRepository : ICronJobsRepository
    {
        public List<CronJob> CronJobs { get; set; }

        public FakeCronJobsRepository(List<CronJob> cronJobs)
        {
            CronJobs = cronJobs;
        }

        public Task<IEnumerable<CronJob>> GetJobsAsync()
        {
            return Task.FromResult((IEnumerable<CronJob>)CronJobs);
        }
    }
}
