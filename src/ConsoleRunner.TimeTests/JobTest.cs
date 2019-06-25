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
                logEntry => true,
                new CronJob
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "3", "CRASH" },
                    CronExpression = $"0 0 0 1 1 ? {DateTime.Now.Year + 2}", //Never run again
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true,
                    LogWarningAfter = TimeSpan.FromSeconds(1),
                    LogErrorAfter = TimeSpan.FromSeconds(2)
                }
            );

            await logger.TaskCompletionSource.Task;
        }

        private async Task<LogLogger> SetupAsync(Func<LogEntry, bool> searchPredicate, params CronJob[] cronJobs)
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

            var logLogger = serviceProvider.GetRequiredService<LogLogger>();
            logLogger.SearchPredicate = searchPredicate;
            
            await scheduler.Start();

            return logLogger;
        }
    }

    public class LogEntry
    {
        public DateTime Time { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
    
    public class LogLogger : ILogger
    {
        public Func<LogEntry, bool> SearchPredicate;
        
        public void SetSearch(Func<LogEntry, bool> predicate)
        {
            SearchPredicate = predicate;
        }
        
        public TaskCompletionSource<LogEntry> TaskCompletionSource = new TaskCompletionSource<LogEntry>();

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
            var logEntry = new LogEntry
            {
                Time = DateTime.Now,
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            };

            if (SearchPredicate(logEntry))
            {
                TaskCompletionSource.SetResult(logEntry);
            }
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
