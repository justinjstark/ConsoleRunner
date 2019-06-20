using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace ConsoleRunner
{
    public class ScopedJobFactory : IJobFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();
        private readonly ILogger<ScopedJobFactory> _logger;

        public ScopedJobFactory(IServiceScopeFactory serviceScopeFactory, ILogger<ScopedJobFactory> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _serviceScopeFactory.CreateScope();
            
            IJob job;
            try
            {
                job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            }
            catch(Exception exception)
            {
                _logger.LogError(exception, $"Unable to instantiate object of type {bundle.JobDetail.JobType.Name} using service provider");
                scope.Dispose();
                throw;
            }

            if (_scopes.TryAdd(job, scope)) return job;
            
            scope.Dispose();
            throw new Exception("Failed to track DI scope");
        }

        public void ReturnJob(IJob job)
        {
            if (_scopes.TryRemove(job, out var scope))
            {
                scope.Dispose();
            }
        }
    }
}
