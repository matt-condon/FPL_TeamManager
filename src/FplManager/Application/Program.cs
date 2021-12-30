using FplClient;
using FplClient.Clients;
using FplClient.Data;
using FplManager.Application.Builders;
using FplManager.Application.Services;
using FplManager.Configuration;
using FplManager.HostedServices;
using FplManager.Infrastructure.Models;
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
                    RegisterServices(services);
                    RegisterHostedServices(services);
                });


        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IApplication, Application>();
            services.AddSingleton<ITeamOrchestratorService, TeamOrchestratorService>();
            services.AddSingleton<IPlayerEvaluationService, PlayerEvaluationService>();
            services.AddSingleton<ITransferSelectorService, TransferSelectorService>();
            
            services.AddSingleton<IFplPlayerClient, FplPlayerClient>();

            services.AddSingleton<ITransferWishlistBuilder, TransferWishlistBuilder>();
            services.AddSingleton<ISetTeamBuilder, SetTeamBuilder>();
            services.AddSingleton<ITeamBuilder<CurrentTeamPick>, CurrentTeamBuilder>();
            services.AddSingleton<ITeamBuilder<FplPick>, EntryTeamBuilder>();

            services.AddSingleton<IPlayerDictionaryBuilder<FplPlayer>, FplPlayerDictionaryBuilder>();
            services.AddSingleton<IPlayerDictionaryBuilder<CurrentFplPlayer>, CurrentFplPlayerDictionaryBuilder>();
        }

        private static void RegisterHostedServices(IServiceCollection services)
        {
            services.AddHostedService<FplTeamManager>();
        }
    }
}
