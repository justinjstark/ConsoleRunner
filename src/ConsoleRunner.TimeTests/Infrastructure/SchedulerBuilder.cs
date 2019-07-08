using ConsoleRunner.Logging;
using ConsoleRunner.Persistence;
using ConsoleRunner.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Quartz.Logging;
using Quartz.Spi;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ConsoleRunner.TimeTests.Infrastructure
{

    public class SchedulerBuilder
    {
        //Begin Setup Variables
        private readonly List<CronJob> _cronJobs = new List<CronJob>();
        
        private TimeSpan _stopAfter = TimeSpan.FromSeconds(5);

        private List<Expression<Func<LogEntry, bool>>> _logEntryPredicates = new List<Expression<Func<LogEntry, bool>>>();

        private List<Func<LogEntry, bool>> _compiledLogEntryPredicates = new List<Func<LogEntry, bool>>();

        private List<Expression<Func<Command, bool>>> _commandPredicates = new List<Expression<Func<Command, bool>>>();

        private List<Func<Command, bool>> _compiledCommandPredicates = new List<Func<Command, bool>>();
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

        public SchedulerBuilder ShouldLog(Expression<Func<LogEntry, bool>> predicate)
        {
            _logEntryPredicates.Add(predicate);
            _compiledLogEntryPredicates.Add(predicate.Compile());

            return this;
        }

        public SchedulerBuilder ShouldFireCommand(Expression<Func<Command, bool>> predicate)
        {
            _commandPredicates.Add(predicate);
            _compiledCommandPredicates.Add(predicate.Compile());

            return this;
        }

        public SchedulerBuilder ShouldFireCommand(string executable)
        {
            return ShouldFireCommand(c => c.Executable == executable);
        }

        public SchedulerBuilder StopAfter(TimeSpan timeout)
        {
            _stopAfter = timeout;

            return this;
        }

        public async Task RunAsync()
        {
            var serviceProvider = BuildServiceProvider();

            await StartScheduler(serviceProvider);

            var task = WaitUntilAllConditionsAreMet();

            if(_stopAfter != null)
            {
                task = task.WithTimeout(_stopAfter);
            }

            var timedOut = await StopAfter(task, _stopAfter);

            var allConditions = new List<Action>
            {
                () => timedOut.ShouldBeFalse()
            }
            .Union(_logEntryPredicates.Select<Expression<Func<LogEntry, bool>>, Action>(
                p => () => LogEntries.ShouldContain(p)))
            .Union(_commandPredicates.Select<Expression<Func<Command, bool>>, Action>(
                p => () => Commands.ShouldContain(p)));

            this.ShouldSatisfyAllConditions(allConditions.ToArray());
        }

        private async Task<bool> StopAfter(Task task, TimeSpan timeout)
        {
            return await Task.WhenAny(task, Task.Delay(timeout)) != task;
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
            return _compiledLogEntryPredicates.All(p => p(LogEntries))
                && _compiledCommandPredicates.All(p => p(Commands));
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
