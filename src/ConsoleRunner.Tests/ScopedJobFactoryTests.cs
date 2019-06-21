using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleRunner.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using Shouldly;
using Xunit;

namespace ConsoleRunner.Tests
{
    public class ScopedJobFactoryTests
    {
        public readonly IServiceProvider ServiceProvider;

        public ScopedJobFactoryTests()
        {
            ServiceProvider = ConfigureServices();
        }
        
        [Fact]
        public async Task scoped_dependencies()
        {
            var (job1, job2) = await RunTwoJobs();
            
            job1.ScopedDependency1.ShouldBeSameAs(job1.ScopedDependency2);
            job1.ScopedDependency1.ShouldNotBeSameAs(job2.ScopedDependency1);
        }
        
        [Fact]
        public async Task transient_dependencies()
        {
            var (job1, job2) = await RunTwoJobs();
            
            job1.TransientDependency1.ShouldNotBeSameAs(job1.TransientDependency2);
            job1.TransientDependency1.ShouldNotBeSameAs(job2.TransientDependency1);
        }
        
        [Fact]
        public async Task singleton_dependencies()
        {
            var (job1, job2) = await RunTwoJobs();
            
            job1.SingletonDependency1.ShouldBeSameAs(job1.SingletonDependency2);
            job1.SingletonDependency1.ShouldBeSameAs(job2.SingletonDependency2);
        }

        [Fact]
        public async Task jobs_are_disposed()
        {
            var (job1, job2) = await RunTwoJobs();

            job1.Disposed.ShouldBeTrue();
            job2.Disposed.ShouldBeTrue();
        }
        
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddScoped<ScopedDependency>();
            
            services.AddTransient<TransientDependency>();
            
            services.AddSingleton<SingletonDependency>();
            
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddTransient<Job>();

            services.AddLogging(config => config.AddConsole());

            return services.BuildServiceProvider();
        }

        private async Task<(Job, Job)> RunTwoJobs()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler(CancellationToken.None);

            scheduler.JobFactory = ServiceProvider.GetRequiredService<IJobFactory>();
            
            var jobTracker = new JobTracker();

            scheduler.ListenerManager.AddJobListener(jobTracker, EverythingMatcher<JobKey>.AllJobs());
            
            await scheduler.Start();

            var jobDetail1 = JobBuilder.Create<Job>()
                .Build();
            
            var nowTrigger1 = TriggerBuilder.Create()
                .StartNow()
                .Build();
            
            var jobDetail2 = JobBuilder.Create<Job>()
                .Build();

            var nowTrigger2 = TriggerBuilder.Create()
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(jobDetail1, nowTrigger1);
            await scheduler.ScheduleJob(jobDetail2, nowTrigger2);
            
            await jobTracker.WaitForJobs(2);

            var jobsRun = jobTracker.JobsRun.ToList();
            var job1 = (Job) jobsRun[0].JobInstance;
            var job2 = (Job) jobsRun[1].JobInstance;

            return (job1, job2);
        }
    }

    public class ScopedDependency
    {
    }

    public class TransientDependency
    {
    }

    public class SingletonDependency
    {
    }

    public class Job : IJob, IDisposable
    {
        public readonly ScopedDependency ScopedDependency1;
        public readonly ScopedDependency ScopedDependency2;
        public readonly TransientDependency TransientDependency1;
        public readonly TransientDependency TransientDependency2;
        public readonly SingletonDependency SingletonDependency1;
        public readonly SingletonDependency SingletonDependency2;
        public bool Disposed = false;

        public Job(ScopedDependency scopedDependency1, ScopedDependency scopedDependency2, TransientDependency transientDependency1, TransientDependency transientDependency2, SingletonDependency singletonDependency1, SingletonDependency singletonDependency2)
        {
            ScopedDependency1 = scopedDependency1;
            ScopedDependency2 = scopedDependency2;
            TransientDependency1 = transientDependency1;
            TransientDependency2 = transientDependency2;
            SingletonDependency1 = singletonDependency1;
            SingletonDependency2 = singletonDependency2;
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public class JobTracker : IJobListener
    {
        public readonly ConcurrentBag<IJobExecutionContext> JobsRun = new ConcurrentBag<IJobExecutionContext>();

        public async Task WaitForJobs(int count)
        {
            while (JobsRun.Count < count)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
        
        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException,
            CancellationToken cancellationToken = new CancellationToken())
        {
            JobsRun.Add(context);
            
            return Task.CompletedTask;
        }

        public string Name => "JobTracker";
    }
}
