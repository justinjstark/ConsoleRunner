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
            var cronJob = (CronJob)context.MergedJobDataMap["CronJob"];

            if (cronJob.SkipIfAlreadyRunning && await JobIsAlreadyRunning(context))
            {
                _logger.LogWarning($"{cronJob.Name} is already running. Skipping.");
                return;
            }

            _logger.LogDebug($"{cronJob.Name} starting");

            CommandResult commandResult;
            try
            {
                commandResult = await ExecuteWithMonitors(context, cronJob);                       
            }
            catch (TimeoutException exception)
            {
                _logger.LogError(exception, $"{cronJob.Name} timed out.");
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
                _logger.LogError(exception, $"{cronJob.Name} threw an exception.");
                return;
            }

            if (commandResult.Success)
            {
                var message = $"{cronJob.Name} ran successfully";
                if (!string.IsNullOrWhiteSpace(commandResult.StandardOutput))
                {
                    message += $"\nStandard Output:\n{commandResult.StandardOutput.Trim('\n')}";
                }
                _logger.LogInformation(message);
            }
            else
            {
                var message = $"{cronJob.Name} returned an non-zero exit code [{commandResult.ExitCode}].";
                if (!string.IsNullOrWhiteSpace(commandResult.StandardError))
                {
                    message += $"\nStandard Error:\n{commandResult.StandardError.Trim('\n')}";
                }
                _logger.LogError(message);
            }            
        }

        private async Task<CommandResult> ExecuteWithMonitors(IJobExecutionContext context, CronJob cronJob)
        {
            var commandTask = Execute(
                    cronJob: cronJob,
                    cancellationToken: cronJob.StopIfApplicationStopping ? context.CancellationToken : CancellationToken.None);

            var (monitorTasks, monitorCancellationTokenSource) = CreateMonitorTasks(context, cronJob);
            
            await Task.WhenAll(monitorTasks.Append(commandTask));

            monitorCancellationTokenSource.Cancel();

            return commandTask.Result;
        }

        private (Task[], CancellationTokenSource) CreateMonitorTasks(IJobExecutionContext context, CronJob cronJob)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var tasks = new Task[]
            {
                CreateDurationMonitorTask(
                    duration: cronJob.LogWarningAfter,
                    action: () => _logger.LogWarning($"{cronJob.Name} is running longer than specified."),
                    cancellationToken: cancellationTokenSource.Token),
                CreateDurationMonitorTask(
                    duration: cronJob.LogErrorAfter,
                    action: () => _logger.LogError($"{cronJob.Name} is running longer than specified."),
                    cancellationToken: cancellationTokenSource.Token)
            };

            return (tasks, cancellationTokenSource);
        }

        private Task CreateDurationMonitorTask(TimeSpan? duration, Action action, CancellationToken cancellationToken)
        {
            return duration.HasValue
                ? Task.Delay(duration.Value, cancellationToken)
                    .ContinueWith(t => action(),
                        TaskContinuationOptions.NotOnCanceled)
                : Task.CompletedTask;
        }

        private async Task<CommandResult> Execute(CronJob cronJob, CancellationToken cancellationToken)
        {
            var command = Command.Run(cronJob.Executable, cronJob.Arguments, options =>
            {
                options.CancellationToken(cancellationToken);
                if (cronJob.Timeout != null)
                {
                    options.Timeout(cronJob.Timeout.Value);
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
