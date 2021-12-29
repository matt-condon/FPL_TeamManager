using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using FplManager.Infrastructure.Models;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using FplManager.Configuration;
using FplManager.Application.Services;
using System.Linq;

namespace FplManager.Application
{
    public interface IApplication
    {
        Task Run();
    }

    public class Application : IApplication
    {
        private const bool TestOnly = true;
        private const int NumberOfTransfers = 1;
        private const bool RequireTransferApproval = true;
        private const bool FreeTransfersOnly = true;
        private const bool UseWC = false;

        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<Application> _logger;
        private readonly HttpClient _httpClient;
        private readonly ITeamOrchestratorService _teamOrchestratorService;
        private readonly IEnumerable<AccountModel> _accounts;
        private readonly string _logInUrl;


        public Application(
            IHostApplicationLifetime lifetime,
            ILogger<Application> logger, 
            HttpClient httpClient, 
            ITeamOrchestratorService teamOrchestratorService, 
            IOptions<AppConfig> appConfig)
        {
            _lifetime = lifetime;
            _logger = logger;
            _httpClient = httpClient;
            _teamOrchestratorService = teamOrchestratorService;
            _accounts = appConfig.Value.AccountModel;
            _logInUrl = appConfig.Value.LogInUrl;
        }

        public async Task Run()
        {
            try
            {
                foreach (var account in TestOnly ? _accounts.Take(1) : _accounts)
                {
                    var authConfig = account.AuthModel;
                    var authConfigAsDictionary = GetValues(authConfig);
                    await AuthenticateAsync(_httpClient, _logInUrl, authConfigAsDictionary);

                    await _teamOrchestratorService.ManageTeam(account.FplTeamId, account.TransferPercentile, NumberOfTransfers, RequireTransferApproval, FreeTransfersOnly, UseWC);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Catastrophic Event Occurred leading to App Exit; error message:{ex}");
            }
            finally
            {
                _lifetime.StopApplication();
            }

        }

        public static Dictionary<string, string> GetValues(AuthenticationModel obj)
        {
            return obj
                    .GetType()
                    .GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(obj).ToString());
        }

        //You need to authenticate, then you need the pl_profile cookie for every subsequent request
        public static async Task AuthenticateAsync(HttpClient httpClient, string loginUrl, Dictionary<string, string> auth)
        {
            try
            {
                var uri = new Uri(loginUrl);
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = uri,
                    Method = HttpMethod.Post,
                    Content = new FormUrlEncodedContent(auth)
                };

                var loginResponse = await httpClient.SendAsync(httpRequest);

                if (loginResponse.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Login Failed. StatusCode: {loginResponse.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Login failed, error: {e}");
            }
        }
    }
}
