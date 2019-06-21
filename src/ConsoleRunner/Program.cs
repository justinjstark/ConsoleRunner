using System;
using System.Threading;
using System.Threading.Tasks;
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

            var scheduler = await SchedulerFactory.GetSchedulerAsync(serviceProvider);
            
            await scheduler.Start(CancellationToken.None);

            await Console.In.ReadLineAsync();

            await scheduler.Shutdown(CancellationToken.None);
        }

        private static IServiceProvider ConfigureServices()
        {
            //Services
            var services = new ServiceCollection();

            services.AddLogging(config => config.AddConsole());
            services.AddTransient<IJobFactory, ScopedJobFactory>();
            services.AddSingleton<ILogProvider, MicrosoftLogProvider>();
            services.AddLogging(config => {
                config.AddConsole();
                //config.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });
            services.AddTransient<Job>();
            services.AddTransient<ICronJobsRepository, CronJobsRepository>();

            return services.BuildServiceProvider();
        }        
    }
}
