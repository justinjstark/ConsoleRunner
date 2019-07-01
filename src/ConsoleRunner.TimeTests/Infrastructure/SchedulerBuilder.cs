using ConsoleRunner.Logging;
using ConsoleRunner.Persistence;
using ConsoleRunner.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{
    public class SchedulerBuilder
    {
        //Begin Setup Variables
        private readonly List<CronJob> _cronJobs = new List<CronJob>();
        
        private TimeSpan? _timeout;

        private bool _throwOnTimeout = true;

        private Func<LogEntry, bool> _logEntryPredicate;

        private Func<List<LogEntry>, bool> _logEntriesPredicate;

        private Func<Command, bool> _commandPredicate;

        private Func<List<Command>, bool> _commandsPredicate;
        //End Setup Variables

        //Begin Run Variables
        private readonly AsyncProducerConsumerQueue<object> _queue = new AsyncProducerConsumerQueue<object>();

        public readonly List<LogEntry> LogEntries = new List<LogEntry>();        

        public readonly List<Command> Commands = new List<Command>();
        //End Run Variables
        
        public SchedulerBuilder WithCronJobs(params CronJob[] cronJobs)
        {
            _cronJobs.AddRange(cronJobs);

            return this;
        }

        public SchedulerBuilder ShouldWriteErrorLogEntry<T>() where T : Exception
        {
            _logEntryPredicate = logEntry => logEntry.Exception is T;

            return this;
        }

        public SchedulerBuilder ShouldWriteLogEntry(Func<LogEntry, bool> predicate)
        {
            _logEntryPredicate = predicate;

            return this;
        }

        public SchedulerBuilder ShouldRunCommand(Func<Command, bool> predicate)
        {
            _commandPredicate = predicate;

            return this;
        }

        public SchedulerBuilder ShouldRunCommands(Func<List<Command>, bool> predicate)
        {
            _commandsPredicate = predicate;

            return this;
        }

        public SchedulerBuilder ShouldRunAllCommands(params string[] executables)
        {
            _commandsPredicate = commandsRun => executables.Except(commandsRun.Select(cr => cr.Executable)).Count() == 0;

            return this;
        }

        public SchedulerBuilder WithTimeout(TimeSpan timeout, bool throwOnTimeout = true)
        {
            _timeout = timeout;
            _throwOnTimeout = throwOnTimeout;

            return this;
        }

        public async Task RunAsync()
        {
            var serviceProvider = BuildServiceProvider();

            await StartScheduler(serviceProvider);

            var task = WaitUntilAllConditionsAreMet();

            if(_timeout != null)
            {
                task = task.WithTimeout(_timeout.Value, _throwOnTimeout);
            }

            await task;
        }

        private async Task WaitUntilAllConditionsAreMet()
        {
            while(true)
            {
                var e = await _queue.DequeueAsync();

                if(e is LogEntry)
                {
                    LogEntries.Add((LogEntry)e);
                }
                else if(e is Command)
                {
                    Commands.Add((Command)e);
                }

                if (AllConditionsAreMet(e)) break;
            }
        }

        private bool AllConditionsAreMet(object e)
        {
            if(_logEntryPredicate == null && _logEntriesPredicate == null
                && _commandPredicate == null && _commandsPredicate == null)
            {
                return false;
            }

            var logEntryCondition = _logEntryPredicate == null ||
                (e is LogEntry && _logEntryPredicate((LogEntry)e));

            var logEntriesCondition = _logEntriesPredicate == null ||
                _logEntriesPredicate(LogEntries);

            var commandCondition = _commandPredicate == null ||
                (e is Command && _commandPredicate((Command)e));

            var commandsCondition = _commandsPredicate == null ||
                _commandsPredicate(Commands);

            return logEntryCondition && logEntriesCondition
                && commandCondition && commandsCondition;
        }

        private IServiceProvider BuildServiceProvider()
        {
            var logger = new FakeLogger(_queue);
            var commandRunner = new FakeCommandRunner(_queue);

            var cronJobsRepository = new FakeCronJobsRepository(_cronJobs);

            var services = new ServiceCollection();

            services.AddLogging(config => config.AddProvider(new FakeLogProvider(logger)));
            services.AddSingleton<SchedulerFactory>();
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddTransient<ICommandRunner, FakeCommandRunner>(sp => commandRunner);
            services.AddSingleton<ILogProvider, MicrosoftLibLogWrapper>();
            services.AddTransient<Job>();
            services.AddTransient<ICronJobRepository>(sp => cronJobsRepository);

            return services.BuildServiceProvider();
        }

        private async Task StartScheduler(IServiceProvider serviceProvider)
        {
            var schedulerFactory = serviceProvider.GetRequiredService<SchedulerFactory>();

            var scheduler = await schedulerFactory.GetSchedulerAsync();

            await scheduler.Start();
        }
    }
}
