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

            var commandResult = await Execute(
                job: job,
                cancellationToken: job.StopIfApplicationStopping ? context.CancellationToken : CancellationToken.None);

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
