using System;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using QuartzLogLevel = Quartz.Logging.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleRunner.Logging
{
    public class MicrosoftLibLogWrapper : ILogProvider
    {
        private readonly ILogger<MicrosoftLibLogWrapper> _logger;

        public MicrosoftLibLogWrapper(ILogger<MicrosoftLibLogWrapper> logger)
        {
            _logger = logger;
        }
        
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func == null) return true;

                MicrosoftLogLevel logLevel;
                switch (level)
                {
                    case QuartzLogLevel.Debug:
                        logLevel = MicrosoftLogLevel.Debug;
                        break;
                    case QuartzLogLevel.Trace:
                        logLevel = MicrosoftLogLevel.Trace;
                        break;
                    case QuartzLogLevel.Info:
                        logLevel = MicrosoftLogLevel.Information;
                        break;
                    case QuartzLogLevel.Warn:
                        logLevel = MicrosoftLogLevel.Warning;
                        break;
                    case QuartzLogLevel.Error:
                        logLevel = MicrosoftLogLevel.Error;
                        break;
                    case QuartzLogLevel.Fatal:
                        logLevel = MicrosoftLogLevel.Critical;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }

                if (exception != null)
                {
                    _logger.Log(logLevel, exception, func());
                }
                else
                {
                    _logger.Log(logLevel, func());
                }
                
                return true;
            };
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return NullDisposable.Instance;
        }

        public IDisposable OpenNestedContext(string message)
        {
            return NullDisposable.Instance;
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
