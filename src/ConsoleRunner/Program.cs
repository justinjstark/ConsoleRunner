using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleRunner.CommandRunners.MedallionShell;
using ConsoleRunner.Logging;
using ConsoleRunner.Persistence;
using ConsoleRunner.Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using Quartz.Spi;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        }

        private static async Task Run()
        {
            var serviceProvider = ConfigureServices();

            var schedulerFactory = serviceProvider.GetRequiredService<SchedulerFactory>();

            var scheduler = await schedulerFactory.GetSchedulerAsync();
            
            await scheduler.Start(CancellationToken.None);

            await Console.In.ReadLineAsync();

            await scheduler.Shutdown(CancellationToken.None);
        }

        private static IServiceProvider ConfigureServices()
        {
            //Services
            var services = new ServiceCollection();

            services.AddLogging(config => config.AddConsole());
            services.AddSingleton<SchedulerFactory>();
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddTransient<ICommandRunner, MedallionCommandRunner>();
            services.AddSingleton<ILogProvider, MicrosoftLibLogWrapper>();
            services.AddLogging(config => {
                config.AddConsole();
                //config.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });
            services.AddTransient<Job>();
            services.AddTransient<ICronJobsRepository, Persistence.Fake.CronJobsRepository>();

            return services.BuildServiceProvider();
        }        
    }
}
