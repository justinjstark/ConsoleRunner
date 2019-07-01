using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.Persistence.Fake
{
    public class FakeRepository : ICronJobRepository
    {
        public async Task<IEnumerable<CronJob>> GetCronJobsAsync()
        {
            var jobs = new List<CronJob>
            {
                new CronJob
                {
                    Id = new Guid("e86bee05-44a1-4bb1-b05a-818c6b2b6bc3"),
                    Name = "I Greet",
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "1", "Hello!" },
                    CronExpression = "*/5 * * * * ? *",
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true
                },
                new CronJob
                {
                    Id = new Guid("602da6d8-59b8-4259-bd16-f5a35195a148"),
                    Name = "I Wave",
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "1", "*waves wildly!" },
                    CronExpression = "*/10 * * * * ? *",
                    StartImmediately = false,
                    SkipIfAlreadyRunning = false
                },
                new CronJob
                {
                    Id = new Guid("5b83a38d-4761-4086-a6a5-d89c5decbe9d"),
                    Name = "I Explode",
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "1", "CRASH" },
                    CronExpression = "*/15 * * * * ? *",
                    StartImmediately = true,
                    SkipIfAlreadyRunning = false
                },
                new CronJob
                {
                    Id = new Guid("a6c39e72-1f03-4e2a-9ea5-aec37f4dbebb"),
                    Name = "EXE Not Found",
                    Enabled = true,
                    Executable = "IDontExist.exe",
                    Arguments = new string[] {},
                    CronExpression = "*/20 * * * * ? *",
                    StartImmediately = true,
                    SkipIfAlreadyRunning = false
                },
                new CronJob
                {
                    Id = new Guid("938ca9b4-a6ef-4387-b165-89740bb3742b"),
                    Name = "I Run Longer Than Specified",
                    Enabled = true,
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "1", "CRASH" },
                    CronExpression = "*/10 * * * * ? *",
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true,
                    LogWarningAfter = TimeSpan.FromSeconds(1),
                    LogErrorAfter = TimeSpan.FromSeconds(2)
                }
            };

            return await Task.FromResult(jobs);
        }
    }
}
