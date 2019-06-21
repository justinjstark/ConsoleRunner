﻿using ConsoleRunner.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner.Quartz
{
    public static class SchedulerFactory
    {
        public static async Task<IScheduler> GetSchedulerAsync(IServiceProvider serviceProvider)
        {
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
                    { "Job", job }
                });

                var jobDetail = JobBuilder.Create<Job>()
                    .SetJobData(jobDataMap)
                    .WithIdentity((string)job.Id.ToString())
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
