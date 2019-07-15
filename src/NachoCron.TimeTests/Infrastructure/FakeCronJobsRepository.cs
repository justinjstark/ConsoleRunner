using NachoCron.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NachoCron.TimeTests.Infrastructure
{
    public class FakeCronJobsRepository : ICronJobRepository
    {
        private List<CronJob> _cronJobs { get; set; }

        public FakeCronJobsRepository(List<CronJob> cronJobs)
        {
            _cronJobs = cronJobs;
        }

        public Task<IEnumerable<CronJob>> GetCronJobsAsync()
        {
            return Task.FromResult((IEnumerable<CronJob>)_cronJobs);
        }
    }
}
