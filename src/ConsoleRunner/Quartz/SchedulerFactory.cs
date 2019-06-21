using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Specialized;
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

            return scheduler;
        }
    }
}
