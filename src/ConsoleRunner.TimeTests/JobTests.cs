using ConsoleRunner.TimeTests.Helpers;
using ConsoleRunner.TimeTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
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

            await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .ShouldLog(le => le.LogLevel == LogLevel.Warning && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedWarning, cronJob.Name))
                .RunAsync();
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

            await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .ShouldLog(le => le.LogLevel == LogLevel.Error && le.Message == string.Format(Resources.JobIsRunningLongerThanExpectedError, cronJob.Name))
                .RunAsync();
        }

        [Fact]
        public async Task start_immediately()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: true,
                cronExpression: CronExpression.Never
            );

            await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .ShouldRunCommand(cronJob.Executable)
                .RunAsync();
        }

        [Fact]
        public async Task do_not_start_immediately()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: false,
                cronExpression: CronExpression.Never
            );

            await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                .ShouldTimeout()
                .RunAsync();
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

            await new SchedulerBuilder()
                .WithCronJobs(cronJob1, cronJob2)
                .ShouldRunCommands(cronJob1.Executable, cronJob2.Executable)
                .RunAsync();
        }

        [Fact]
        public async Task run_a_job_multiple_times()
        {
            var cronJob = CronJobBuilder.With(
                startImmediately: false,
                cronExpression: CronExpression.EveryOneSecond
            );

            await new SchedulerBuilder()
                .WithCronJobs(cronJob)
                //StopWhen
                .Should(result => result.Commands.Count.ShouldBeGreaterThan(1))
                .RunAsync();
            //return TestResult

            //Assert here ???
        }

        //[Fact]
        //public async Task skip_already_running_job()
        //{
        //    var cronJob = CronJobBuilder.With(
        //        startImmediately: true,
        //        cronExpression: CronExpression.EveryOneSecond,
        //        arguments: CommandArguments.CommandDurationInSeconds(5),
        //        skipIfAlreadyRunning: true
        //    );

        //    var scheduler = new SchedulerBuilder()
        //        .WithCronJobs(cronJob)
        //        .StopAfter(TimeSpan.FromSeconds(3), false);

        //    await scheduler.RunAsync();

        //    scheduler.Commands.Where(c => c.Executable == cronJob.Executable)
        //        .Count().ShouldBe(1);
        //}

        //[Fact]
        //public async Task run_the_same_job_concurrently()
        //{
        //    var cronJob = CronJobBuilder.With(
        //        startImmediately: true,
        //        cronExpression: CronExpression.EveryOneSecond,
        //        arguments: CommandArguments.CommandDurationInSeconds(5),
        //        skipIfAlreadyRunning: false
        //    );

        //    await new SchedulerBuilder()
        //        .WithCronJobs(cronJob)
        //        .StopAfter(TimeSpan.FromSeconds(3), false)
        //        .StopOnMultipleCommands(cronJob.Executable)
        //        .RunAsync();
        //}
    }
}
