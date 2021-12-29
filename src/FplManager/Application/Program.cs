using FplManager.Application.Services;
using FplManager.Configuration;
using FplManager.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace FplManager.Application
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddJsonFile($"Configuration/appsettings.json", false)
                   .AddEnvironmentVariables();
                })
                .ConfigureServices((builderContext, services) =>
                {
                    services.AddOptions();
                    services.AddHttpClient();

                    services.Configure<AppConfig>(builderContext.Configuration);

                    services.AddSingleton<IApplication, Application>();
                    services.AddSingleton<ITeamOrchestratorService, TeamOrchestratorService>();
                    services.AddHostedService<FplTeamManager>();
                });
    }
}
