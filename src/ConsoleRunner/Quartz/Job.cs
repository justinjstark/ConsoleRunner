using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ConsoleRunner.Quartz
{
    public class Job : IJob
    {
        private readonly ILogger<Job> _logger;

        public Job(ILogger<Job> logger)
        {
            _logger = logger;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            var job = (CronJob)context.MergedJobDataMap["CronJob"];

            if (job.SkipIfAlreadyRunning && await JobIsAlreadyRunning(context))
            {
                _logger.LogWarning($"{job.Name} is already running. Skipping.");
                return;
            }

            _logger.LogDebug($"{job.Name} starting");

            CommandResult commandResult = null;
            try
            {
                commandResult = await Execute(
                job: job,
                cancellationToken: job.StopIfApplicationStopping ? context.CancellationToken : CancellationToken.None);
            }
            catch (TimeoutException exception)
            {
                _logger.LogError(exception, $"{job.Name} timed out.");
                return;
            }
            catch (Exception exception)
            {
                /*
                 * https://www.quartz-scheduler.net/documentation/best-practices.html
                 * A Job’s execute method should contain a try-catch block that handles
                 * all possible exceptions.
                 * 
                 * If a job throws an exception, Quartz will typically immediately
                 * re-execute it (and it will likely throw the same exception again).
                 * It’s better if the job catches all exception it may encounter, handle
                 * them, and reschedule itself, or other jobs to work around the issue.
                 */
                _logger.LogError(exception, $"{job.Name} threw an exception");
                return;
            }

            if (commandResult.Success && !string.IsNullOrWhiteSpace(commandResult.StandardOutput))
            {
                _logger.LogInformation($"{job.Name} {commandResult.StandardOutput.Trim('\n')}");
            }
            else
            {
                _logger.LogError($"{job.Name} EXIT CODE {commandResult.ExitCode}\n{commandResult.StandardError.Trim('\n')}");
            }

            _logger.LogDebug($"{job.Name} ending");
        }

        private async Task<CommandResult> Execute(CronJob job, CancellationToken cancellationToken)
        {
            var command = Command.Run(job.Executable, job.Arguments, options =>
            {
                options.CancellationToken(cancellationToken);
                if (job.Timeout != null)
                {
                    options.Timeout(job.Timeout.Value);
                }
            });

            return await command.Task;
        }

        private async Task<bool> JobIsAlreadyRunning(IJobExecutionContext context)
        {
            return (await context.Scheduler.GetCurrentlyExecutingJobs()).Any(j =>
                j.JobDetail.Equals(context.JobDetail) && !j.JobInstance.Equals(this));
        }
    }
}
