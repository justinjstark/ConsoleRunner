using ConsoleRunner.Logging;
using ConsoleRunner.MedallionShell;
using ConsoleRunner.Persistence;
using ConsoleRunner.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleRunner.TimeTests
{
    public class JobTest
    {
        [Fact]
        public async Task Test1()
        {
            var logger = await SetupAsync(
                new CronJob
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "3", "CRASH" },
                    CronExpression = null, //Only run once
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true,
                    LogWarningAfter = TimeSpan.FromSeconds(1),
                    LogErrorAfter = TimeSpan.FromSeconds(2)
                }
            );

            while (true)
            {
                await logger.Semaphore.WaitAsync();
            }
        }

        private async Task<LogLogger> SetupAsync(params CronJob[] cronJobs)
        {
            var MockCronJobsRepository = new Mock<ICronJobsRepository>();

            MockCronJobsRepository.Setup(cjr => cjr.GetJobsAsync())
                .ReturnsAsync(() => cronJobs);

            var services = new ServiceCollection();

            services.AddLogging(config => config.AddConsole());
            services.AddSingleton<SchedulerFactory>();
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddTransient<ICommandRunner, MedallionCommandRunner>();
            services.AddSingleton<ILogProvider, MicrosoftLibLogWrapper>();
            services.AddSingleton<ILogger, LogLogger>();
            services.AddSingleton<LogLogger>();
            services.AddTransient<Job>();
            services.AddTransient<ICronJobsRepository>(sp => MockCronJobsRepository.Object);

            var serviceProvider = services.BuildServiceProvider();

            var schedulerFactory = serviceProvider.GetRequiredService<SchedulerFactory>();

            var scheduler = await schedulerFactory.GetSchedulerAsync();

            await scheduler.Start();

            return serviceProvider.GetRequiredService<LogLogger>();
        }
    }

    public class LogLogger : ILogger
    {
        public class LogEntry
        {
            public DateTime Time { get; set; }
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; }
        }

        public SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Handler(new LogEntry
            {
                Time = DateTime.Now,
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
        }

        private class NullDisposable : IDisposable
        {
            internal static readonly IDisposable Instance = new NullDisposable();

            public void Dispose()
            {
            }
        }
    }
}
