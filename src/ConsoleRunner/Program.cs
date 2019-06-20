using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleRunner.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        }

        private static async Task Run()
        {
            var serviceProvider = ConfigureServices();

            //Quartz.NET
            LogProvider.SetCurrentLogProvider(serviceProvider.GetService<ILogProvider>());
            
            var quartzProperties = new NameValueCollection
            {
                { "quartz.plugin.ShutdownHook.type", "Quartz.Plugin.Management.ShutdownHookPlugin, Quartz.Plugins" },
                { "quartz.threadPool.threadCount", "8" },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.scheduler.instanceName", "ConsoleRunner" }
            };
            var schedulerFactory = new StdSchedulerFactory(quartzProperties);
            var scheduler = await schedulerFactory.GetScheduler(CancellationToken.None);

            scheduler.JobFactory = serviceProvider.GetRequiredService<IJobFactory>();
            
            await scheduler.Start(CancellationToken.None);
            
            await ScheduleJobsAsync(scheduler, serviceProvider.GetRequiredService<IJobsRepository>());

            await Console.In.ReadLineAsync();

            await scheduler.Shutdown(CancellationToken.None);
        }

        private static IServiceProvider ConfigureServices()
        {
            //Services
            var services = new ServiceCollection();

            services.AddLogging(config => config.AddConsole());
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddSingleton<ILogProvider, MicrosoftLogProvider>();
            services.AddTransient<DummyJob>();
            services.AddTransient<IJobsRepository, JobsRepository>();

            return services.BuildServiceProvider();
        }

        private static async Task ScheduleJobsAsync(IScheduler scheduler, IJobsRepository jobsRepository)
        {
            var jobs = await jobsRepository.GetJobsAsync();

            foreach (var job in jobs)
            {
                var jobDataMap = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>
                {
                    { "Job", job }
                });
                
                var jobDetail = JobBuilder.Create<DummyJob>()
                    .SetJobData(jobDataMap)
                    .WithIdentity((string) job.Id.ToString())
                    .Build();

                var triggers = new List<ITrigger>
                {
                    TriggerBuilder.Create()
                        .WithIdentity((string) job.Id.ToString())
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

    public class DummyJob : IJob
    {
        private readonly ILogger<DummyJob> _logger;

        public DummyJob(ILogger<DummyJob> logger)
        {
            _logger = logger;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            var job = (DummyJobSchedule)context.JobDetail.JobDataMap["Job"];

            if (job.SkipIfAlreadyRunning && await JobIsAlreadyRunning(context, job))
            {
                _logger.LogInformation($"{job.Name} is already running. Skipping.");
                return;
            }

            _logger.LogInformation($"{job.Name} starting");

            await Task.Delay(TimeSpan.FromSeconds(job.TaskSeconds));
            
            _logger.LogInformation($"{job.Name} ending");
        }

        private async Task<bool> JobIsAlreadyRunning(IJobExecutionContext context, DummyJobSchedule job)
        {
            return (await context.Scheduler.GetCurrentlyExecutingJobs()).Any(j =>
                j.JobDetail.Equals(context.JobDetail) && !j.JobInstance.Equals(this));
        }
    }

    public class DummyJobSchedule
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TaskSeconds { get; set; }
        public string CronExpression { get; set; }
        public bool StartImmediately { get; set; }
        public bool SkipIfAlreadyRunning { get; set; }
    }

    public interface IJobsRepository
    {
        Task<IEnumerable<DummyJobSchedule>> GetJobsAsync();
    }

    public class JobsRepository : IJobsRepository
    {
        public async Task<IEnumerable<DummyJobSchedule>> GetJobsAsync()
        {
            var jobs = new List<DummyJobSchedule>
            {
                new DummyJobSchedule
                {
                    Id = new Guid("e86bee05-44a1-4bb1-b05a-818c6b2b6bc3"),
                    Name = "Application 1",
                    TaskSeconds = 10,
                    CronExpression = "*/5 * * * * ? *", //Every 5 seconds
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true
                },
                new DummyJobSchedule
                {
                    Id = new Guid("602da6d8-59b8-4259-bd16-f5a35195a148"),
                    Name = "Application 2",
                    TaskSeconds = 5,
                    CronExpression = "*/2 * * * * ? *", //Every 2 seconds
                    StartImmediately = false,
                    SkipIfAlreadyRunning = false
                }
            };

            return await Task.FromResult(jobs);
        }
    }
}
