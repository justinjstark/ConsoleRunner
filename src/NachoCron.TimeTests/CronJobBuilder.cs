using NachoCron.TimeTests.Helpers;
using System;
using System.Collections.Generic;

namespace NachoCron.TimeTests
{
    public static class CronJobBuilder
    {
        public static CronJob With(Guid? id = null, string name = null,
            bool enabled = true, string executable = null,
            IEnumerable<string> arguments = null,
            string cronExpression = null,
            TimeSpan? logWarningAfter = null, TimeSpan? logErrorAfter = null,
            TimeSpan? timeout = null, bool startImmediately = false,
            bool skipIfAlreadyRunning = false, bool stopIfApplicationStopping = true)
        {
            return new CronJob
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? Guid.NewGuid().ToString(),
                Enabled = enabled,
                Executable = executable ?? Guid.NewGuid().ToString(),
                Arguments = arguments ?? CommandArguments.CommandDurationInSeconds(0),
                CronExpression = cronExpression ?? CronExpression.Never,
                LogWarningAfter = logWarningAfter,
                LogErrorAfter = logErrorAfter,
                Timeout = timeout,
                StartImmediately = startImmediately,
                SkipIfAlreadyRunning = skipIfAlreadyRunning,
                StopIfApplicationStopping = stopIfApplicationStopping
            };
        }
    }
}
