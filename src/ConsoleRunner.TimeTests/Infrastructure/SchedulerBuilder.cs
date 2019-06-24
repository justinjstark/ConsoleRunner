using ConsoleRunner.Logging;
using ConsoleRunner.Persistence;
using ConsoleRunner.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class SchedulerBuilder
    {
        private readonly List<CronJob> _cronJobs = new List<CronJob>();
        private Func<LogEntry, ConcurrentBag<LogEntry>, bool> _logEntriesPredicate;
        private TimeSpan? _timeout;

        public SchedulerBuilder WithCronJobs(params CronJob[] cronJobs)
        {
            _cronJobs.AddRange(cronJobs);

            return this;
        }

        public SchedulerBuilder ShouldWriteLogEntry(Func<LogEntry, bool> logEntryPredicate)
        {
            _logEntriesPredicate = (logEntry, logEntries) => logEntryPredicate(logEntry);

            return this;
        }

        public SchedulerBuilder ShouldWriteErrorLogEntry<T>() where T : Exception
        {
            _logEntriesPredicate = (logEntry, logEntries) => logEntry.Exception is T;

            return this;
        }

        public SchedulerBuilder ShouldWriteErrorLogEntry<T>(Func<LogEntry, bool> logEntryPredicate) where T : Exception
        {
            _logEntriesPredicate = (logEntry, logEntries) => logEntry.Exception is T && logEntryPredicate(logEntry);

            return this;
        }

        public SchedulerBuilder ShouldWriteLogEntries(Func<ConcurrentBag<LogEntry>, bool> logEntriesPredicate)
        {
            _logEntriesPredicate = (logEntry, logEntries) => logEntriesPredicate(logEntries);

            return this;
        }

        public SchedulerBuilder WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;

            return this;
        }

        public async Task<ConcurrentBag<LogEntry>> RunAsync()
        {
            var fakeCronJobsRepository = new FakeCronJobsRepository(_cronJobs);

            var logger = new FakeLogger(_logEntriesPredicate);

            var serviceProvider = BuildServiceProvider(logger, fakeCronJobsRepository);

            await StartScheduler(serviceProvider);

            await logger.TaskCompletionSource.Task.WithTimeout(_timeout.Value);

            return logger.LogEntries;
        }

        private IServiceProvider BuildServiceProvider(ILogger logger, ICronJobsRepository cronJobsRepository)
        {
            var services = new ServiceCollection();

            services.AddLogging(config => config.AddProvider(new FakeLogProvider(logger)));
            services.AddSingleton<SchedulerFactory>();
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddTransient<ICommandRunner, FakeCommandRunner>();
            services.AddSingleton<ILogProvider, MicrosoftLibLogWrapper>();
            services.AddTransient<Job>();
            services.AddTransient<ICronJobsRepository>(sp => cronJobsRepository);

            return services.BuildServiceProvider();
        }

        private async Task StartScheduler(IServiceProvider serviceProvider)
        {
            var schedulerFactory = serviceProvider.GetRequiredService<SchedulerFactory>();

            var scheduler = await schedulerFactory.GetSchedulerAsync();

            var schedulerTask = scheduler.Start();

            if (_timeout != null)
            {
                schedulerTask = schedulerTask.WithTimeout(_timeout.Value);
            }

            await schedulerTask;
        }
    }
}
