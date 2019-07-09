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
using System.Collections.Concurrent;
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

        private bool _shouldTimeout = false;

        private List<Expression<Func<LogEntry, bool>>> _logEntryPredicates = new List<Expression<Func<LogEntry, bool>>>();

        private List<Expression<Func<Command, bool>>> _commandPredicates = new List<Expression<Func<Command, bool>>>();

        private List<Action<TestResult>> _shoulds = new List<Action<TestResult>>();
        //End Setup Variables

        //Begin Run Variables
        private readonly AsyncProducerConsumerQueue<object> _queue = new AsyncProducerConsumerQueue<object>();

        public readonly ConcurrentBag<LogEntry> LogEntries = new ConcurrentBag<LogEntry>();

        public readonly ConcurrentBag<Command> Commands = new ConcurrentBag<Command>();
        //End Run Variables
        
        public SchedulerBuilder WithCronJobs(params CronJob[] cronJobs)
        {
            _cronJobs.AddRange(cronJobs);

            return this;
        }

        public SchedulerBuilder ShouldLog(params Expression<Func<LogEntry, bool>>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                _logEntryPredicates.Add(predicate);
            }

            return this;
        }

        public SchedulerBuilder Should(Action<TestResult> action)
        {
            _shoulds.Add(action);

            return this;
        }

        public SchedulerBuilder ShouldRunCommand(Expression<Func<Command, bool>> predicate)
        {
            _commandPredicates.Add(predicate);

            return this;
        }

        public SchedulerBuilder ShouldRunCommands(params Expression<Func<Command, bool>>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                _commandPredicates.Add(predicate);
            }

            return this;
        }

        public SchedulerBuilder ShouldRunCommand(string executable)
        {
            return ShouldRunCommand(c => c.Executable == executable);
        }

        public SchedulerBuilder ShouldRunCommands(params string[] executables)
        {
            return ShouldRunCommands(
                executables.Select<string, Expression<Func<Command, bool>>>(
                    e => command => command.Executable == e).ToArray());
        }

        public SchedulerBuilder ShouldTimeout()
        {
            _shouldTimeout = true;

            return this;
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

            bool timedOut = false;
            if(_stopAfter != null)
            {
                timedOut = await StopAfter(task, _stopAfter);
            }
            else
            {
                await task;
            }

            var allConditions = new List<Action>();

            if(_shouldTimeout)
            {
                allConditions.Add(() => timedOut.ShouldBeTrue());
            }
            else
            {
                allConditions.Add(() => timedOut.ShouldBeFalse());
            }

            allConditions.AddRange(_logEntryPredicates.Select<Expression<Func<LogEntry, bool>>, Action>(
                p => () => LogEntries.ShouldContain(p)));

            allConditions.AddRange(_commandPredicates.Select<Expression<Func<Command, bool>>, Action>(
                p => () => Commands.ShouldContain(p)));

            this.ShouldSatisfyAllConditions(allConditions.ToArray());
        }

        private async Task<bool> StopAfter(Task task, TimeSpan timeout)
        {
            return await Task.WhenAny(task, Task.Delay(timeout)) != task;
        }

        private async Task WaitUntilAllConditionsAreMet()
        {
            var compiledLogEntryPredicates = _logEntryPredicates.Select(p => p.Compile());
            var compiledCommandPredicates = _commandPredicates.Select(p => p.Compile());

            while (true)
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

                if (AllConditionsAreMet(e, compiledLogEntryPredicates, compiledCommandPredicates)) break;
            }
        }

        private bool AllConditionsAreMet(object e,
                IEnumerable<Func<LogEntry, bool>> compiledLogEntryPredicates,
                IEnumerable<Func<Command, bool>> compiledCommandPredicates)
        {
            if (!compiledLogEntryPredicates.Any() &&
                !compiledCommandPredicates.Any())
                return false;

            return compiledLogEntryPredicates.All(p => LogEntries.Any(le => p(le)))
                && compiledCommandPredicates.All(p => Commands.Any(c => p(c)));
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
