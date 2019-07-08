using ConsoleRunner.TimeTests.Helpers;
using ConsoleRunner.TimeTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleRunner.TimeTests
{
    public class JobTests
    {
        [Fact]
        public async Task log_warning_when_running_long()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: true,
                cronExpression: CronExpression.Never,                
                arguments: CommandArguments.CommandDurationInSeconds(3),
                logWarningAfter: TimeSpan.FromSeconds(1)
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopWhen(le => le.LogLevel == LogLevel.Warning && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedWarning, cronJob.Name))
                .RunAsync();

            testResult.ShouldSatisfyAllConditions(
                () => testResult.TimedOut.ShouldBeFalse(),
                () => testResult.LogEntries.ShouldContain(le => le.LogLevel == LogLevel.Warning && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedWarning, cronJob.Name, 1))
            );
        }

        [Fact]
        public async Task log_error_when_running_long()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: true,
                cronExpression: CronExpression.Never,
                arguments: CommandArguments.CommandDurationInSeconds(3),
                logErrorAfter: TimeSpan.FromSeconds(2)
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopWhen(le => le.LogLevel == LogLevel.Error && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedError, cronJob.Name))
                .RunAsync();

            testResult.ShouldSatisfyAllConditions(
                () => testResult.TimedOut.ShouldBeFalse(),
                () => testResult.LogEntries.ShouldContain(le => le.LogLevel == LogLevel.Error && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedError, cronJob.Name))
            );
        }

        [Fact]
        public async Task start_immediately()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: true,
                cronExpression: CronExpression.Never
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopWhen(c => c.Executable == cronJob.Executable)
                .RunAsync();

            testResult.ShouldSatisfyAllConditions(
                () => testResult.TimedOut.ShouldBeFalse(),
                () => testResult.Commands.ShouldContain(c => c.Executable == cronJob.Executable)
            );
        }

        [Fact]
        public async Task do_not_start_immediately()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: false,
                cronExpression: CronExpression.Never
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopAfter(TimeSpan.FromSeconds(2))
                .RunAsync();

            testResult.TimedOut.ShouldBeTrue();
        }

        [Fact]
        public async Task run_two_jobs()
        {
            var cronJob1 = CronJobBuilder.With(
                startImmediately: false,
                cronExpression: CronExpression.EveryOneSecond
            );

            var cronJob2 = CronJobBuilder.With(
                startImmediately: false,
                cronExpression: CronExpression.EveryTwoSeconds
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob1, cronJob2)
                .StopWhen(r => r.Commands.Any(c => c.Executable == cronJob1.Executable)
                    && r.Commands.Any(c => c.Executable == cronJob2.Executable))
                .RunAsync();

            testResult.ShouldSatisfyAllConditions(
                () => testResult.TimedOut.ShouldBeFalse(),
                () => testResult.Commands.ShouldContain(c => c.Executable == cronJob1.Executable),
                () => testResult.Commands.ShouldContain(c => c.Executable == cronJob2.Executable)
            );
        }

        [Fact]
        public async Task run_a_job_multiple_times()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: false,
                cronExpression: CronExpression.EveryOneSecond
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopWhen(tr => tr.Commands.Count(c => c.Executable == cronJob.Executable) > 1)
                .RunAsync();

            testResult.ShouldSatisfyAllConditions(
                () => testResult.TimedOut.ShouldBeFalse(),
                () => testResult.Commands.Count(c => c.Executable == cronJob.Executable).ShouldBeGreaterThan(1)
            );
        }

        [Fact]
        public async Task skip_already_running_job()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: true,
                cronExpression: CronExpression.EveryOneSecond,
                arguments: CommandArguments.CommandDurationInSeconds(5),
                skipIfAlreadyRunning: true
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopAfter(TimeSpan.FromSeconds(3))
                .RunAsync();

            testResult.Commands
                .Where(c => c.Executable == cronJob.Executable)
                .Count()
                .ShouldBe(1);
        }

        [Fact]
        public async Task run_the_same_job_concurrently()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: true,
                cronExpression: CronExpression.EveryOneSecond,
                arguments: CommandArguments.CommandDurationInSeconds(5),
                skipIfAlreadyRunning: false
            );

            var testResult = await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .StopWhen(tr => tr.Commands.Count(c => c.Executable == cronJob.Executable) > 1)
                .RunAsync();

            testResult.ShouldSatisfyAllConditions(
                () => testResult.TimedOut.ShouldBeFalse(),
                () => testResult.Commands.Count(c => c.Executable == cronJob.Executable).ShouldBeGreaterThan(1)
            );
        }
    }
}
