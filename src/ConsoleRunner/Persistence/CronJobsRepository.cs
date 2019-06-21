using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRunner.Persistence
{
    public class CronJobsRepository : ICronJobsRepository
    {
        public async Task<IEnumerable<CronJob>> GetJobsAsync()
        {
            var jobs = new List<CronJob>
            {
                new CronJob
                {
                    Id = new Guid("e86bee05-44a1-4bb1-b05a-818c6b2b6bc3"),
                    Name = "Application 1",
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "Application 1 says hello!", "10" },
                    CronExpression = "*/5 * * * * ? *", //Every 5 seconds
                    StartImmediately = true,
                    SkipIfAlreadyRunning = true
                },
                new CronJob
                {
                    Id = new Guid("602da6d8-59b8-4259-bd16-f5a35195a148"),
                    Name = "Application 2",
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "Application 2 waves wildly!", "2" },
                    CronExpression = "*/10 * * * * ? *", //Every 10 seconds
                    StartImmediately = false,
                    SkipIfAlreadyRunning = false
                },
                new CronJob
                {
                    Id = new Guid("5b83a38d-4761-4086-a6a5-d89c5decbe9d"),
                    Name = "Application 3",
                    Executable = "ExampleExes/ConsoleRunner.ExampleExe.exe",
                    Arguments = new string[] { "CRASH" },
                    CronExpression = "*/15 * * * * ? *", //Every 10 seconds
                    StartImmediately = true,
                    SkipIfAlreadyRunning = false
                }
            };

            return await Task.FromResult(jobs);
        }
    }
}
