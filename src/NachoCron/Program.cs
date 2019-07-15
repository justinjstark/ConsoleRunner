using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NachoCron.CommandRunners.MedallionShell;
using NachoCron.Logging;
using NachoCron.Persistence;
using NachoCron.Quartz;
using Quartz.Logging;
using Quartz.Spi;

namespace NachoCron
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(logging =>
                {
                    logging.AddEventLog();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();

                    services.AddSingleton<SchedulerFactory>();
                    services.AddTransient<IJobFactory, ScopedJobFactory>();
                    services.AddTransient<ICommandRunner, MedallionCommandRunner>();
                    services.AddSingleton<ILogProvider, MicrosoftLibLogWrapper>();
                    services.AddTransient<Job>();
                    services.AddTransient<ICronJobRepository, Persistence.Fake.FakeRepository>();
                });
    }
}
