using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using ConsoleRunner.Logging;
using ConsoleRunner.Persistence;
using ConsoleRunner.Quartz;
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

            await ScheduleJobsAsync(scheduler, serviceProvider.GetRequiredService<ICronJobsRepository>());

            await scheduler.Start(CancellationToken.None);
            
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
            services.AddTransient<Job>();
            services.AddTransient<ICronJobsRepository, CronJobsRepository>();

            return services.BuildServiceProvider();
        }

        private static async Task ScheduleJobsAsync(IScheduler scheduler, ICronJobsRepository jobsRepository)
        {
            var jobs = await jobsRepository.GetJobsAsync();

            foreach (var job in jobs)
            {
                var jobDataMap = new JobDataMap((IDictionary<string, object>) new Dictionary<string, object>
                {
                    { "Job", job }
                });
                
                var jobDetail = JobBuilder.Create<Job>()
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
}
