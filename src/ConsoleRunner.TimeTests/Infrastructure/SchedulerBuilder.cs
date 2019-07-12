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
        private readonly List<CronJob> _cronJobs = new List<CronJob>();
        
        private TimeSpan _stopAfter = TimeSpan.FromSeconds(5);
        
        private Func<TestResult, bool> _stopWhens;
        
        private readonly AsyncProducerConsumerQueue<object> _queue = new AsyncProducerConsumerQueue<object>();

        public readonly TestResult TestResult = new TestResult();
        
        public SchedulerBuilder WithCronJobs(params CronJob[] cronJobs)
        {
            _cronJobs.AddRange(cronJobs);

            return this;
        }

        public SchedulerBuilder StopWhen(Func<TestResult, bool> predicate)
        {
            _stopWhens = predicate;

            return this;
        }

        public SchedulerBuilder StopWhen(Func<LogEntry, bool> predicate)
        {
            _stopWhens = testResult => testResult.LogEntries.Any(predicate);

            return this;
        }

        public SchedulerBuilder StopWhen(Func<Command, bool> predicate)
        {
            _stopWhens = testResult => testResult.Commands.Any(predicate);

            return this;
        }

        public SchedulerBuilder StopAfter(TimeSpan timeout)
        {
            _stopAfter = timeout;

            return this;
        }

        public async Task<TestResult> RunAsync()
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

            if(timedOut)
            {
                TestResult.TimedOut = true;
            }

            return TestResult;
        }

        private async Task<bool> StopAfter(Task task, TimeSpan timeout)
        {
            return await Task.WhenAny(task, Task.Delay(timeout)) != task;
        }

        private async Task WaitUntilAllConditionsAreMet()
        {
            do
            {
                var e = await _queue.DequeueAsync();

                if (e is LogEntry)
                {
                    TestResult.LogEntries.Add((LogEntry)e);
                }
                else if (e is Command)
                {
                    TestResult.Commands.Add((Command)e);
                }
            }
            while (_stopWhens == null || !_stopWhens(TestResult));
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
