using ConsoleRunner.TimeTests.Infrastructure;
using Shouldly;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleRunner.TimeTests
{
    public class JobTests
    {
        private readonly ITestOutputHelper _output;

        public JobTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task long_running_jobs_log_warnings()
        {
            var jobName = "Test";

            await new SchedulerBuilder()
                .WithCronJobs(new CronJob
                {
                    Id = Guid.NewGuid(),
                    Name = jobName,
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "3", "CRASH" },
                    CronExpression = $"0 0 0 1 1 ? {DateTime.Now.Year + 2}",
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true,
                    LogWarningAfter = TimeSpan.FromSeconds(1),
                    LogErrorAfter = TimeSpan.FromSeconds(2)
                })                
                .WithTimeout(TimeSpan.FromSeconds(5))
                .ShouldWriteLogEntry(le => le.LogLevel == LogLevel.Warning && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedWarning, jobName))
                .RunAsync();
        }

        [Fact]
        public async Task long_running_jobs_log_errors()
        {
            var jobName = "Test";

            await new SchedulerBuilder()
                .WithCronJobs(new CronJob
                {
                    Id = Guid.NewGuid(),
                    Name = jobName,
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "3", "CRASH" },
                    CronExpression = $"0 0 0 1 1 ? {DateTime.Now.Year + 2}",
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true,
                    LogWarningAfter = TimeSpan.FromSeconds(1),
                    LogErrorAfter = TimeSpan.FromSeconds(2)
                })
                .WithTimeout(TimeSpan.FromSeconds(5))
                .ShouldWriteLogEntry(le => le.LogLevel == LogLevel.Error && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedError, jobName))
                .RunAsync();
        }
    }
}
