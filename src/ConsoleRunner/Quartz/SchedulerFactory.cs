using ConsoleRunner.Persistence;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner.Quartz
{
    public class SchedulerFactory
    {
        private readonly ILogProvider _logProvider;
        private readonly IJobFactory _jobFactory;
        private readonly ICronJobsRepository _cronJobsRepository;

        public SchedulerFactory(ILogProvider logProvider, IJobFactory jobFactory, ICronJobsRepository cronJobsRepository)
        {
            _logProvider = logProvider;
            _jobFactory = jobFactory;
            _cronJobsRepository = cronJobsRepository;
        }

        public async Task<IScheduler> GetSchedulerAsync()
        {
            //Quartz.NET
            LogProvider.SetCurrentLogProvider(_logProvider);

            var quartzProperties = new NameValueCollection
            {
                { "quartz.plugin.ShutdownHook.type", "Quartz.Plugin.Management.ShutdownHookPlugin, Quartz.Plugins" },
                { "quartz.threadPool.threadCount", "8" },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.scheduler.instanceName", "ConsoleRunner" }
            };
            var schedulerFactory = new StdSchedulerFactory(quartzProperties);
            var scheduler = await schedulerFactory.GetScheduler(CancellationToken.None);

            scheduler.JobFactory = _jobFactory;

            await ScheduleJobsAsync(scheduler, _cronJobsRepository);

            return scheduler;
        }

        private static async Task ScheduleJobsAsync(IScheduler scheduler, ICronJobsRepository jobsRepository)
        {
            var jobs = (await jobsRepository.GetJobsAsync())
                .Where(cj => cj.Enabled);

            foreach (var job in jobs)
            {
                var jobDataMap = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>
                {
                    { "CronJob", job }
                });

                var jobDetail = JobBuilder.Create<Job>()
                    .SetJobData(jobDataMap)
                    .WithIdentity(job.Id.ToString())
                    .Build();

                var triggers = new List<ITrigger>
                {
                    TriggerBuilder.Create()
                        .WithIdentity(job.Id.ToString())
                        .WithCronSchedule(job.CronExpression)
                        .Build()
                };

                if (job.StartImmediately)
                {
                    triggers.Add(
                    TriggerBuilder.Create()
                        .WithIdentity($"{job.Id.ToString()} Immediate")
                        .StartNow()
                            .Build()
                    );
                }

                await scheduler.ScheduleJob(jobDetail, new ReadOnlyCollection<ITrigger>(triggers), false, CancellationToken.None);
            }
        }
    }
}
