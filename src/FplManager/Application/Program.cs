using FplManager.Application.Services;
using FplManager.Configuration;
using FplManager.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FplManager.Application
{
    public class Program
    {
        private const bool TestOnly = false;
        private const int NumberOfTransfers = 1;
        private const bool RequireTransferApproval = true;
        private const bool FreeTransfersOnly = true;

        private static TeamOrchestratorService _teamOrchestratorService;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("./Configuration/appSettings.json", true, true);
            var config = builder.Build();
            var appConfig = config.Get<AppConfig>();
            var accounts = appConfig.AccountModel;
            
            accounts = TestOnly ? accounts.Take(1) : accounts;

            var httpClient = new HttpClient();

            _teamOrchestratorService = new TeamOrchestratorService(httpClient);

            foreach (var account in accounts)
            {
                var authConfig = account.AuthModel;
                var authConfigAsDictionary = GetValues(authConfig);
                await AuthenticateAsync(httpClient, appConfig.LogInUrl, authConfigAsDictionary);

                await _teamOrchestratorService.ManageTeam(account.FplTeamId, account.TransferPercentile , NumberOfTransfers, RequireTransferApproval, FreeTransfersOnly);
            }
        }

        public static Dictionary<string, string> GetValues(AuthenticationModel obj) 
        {
            return obj
                    .GetType()
                    .GetProperties()
                    .ToDictionary(p=>p.Name, p=> p.GetValue(obj).ToString());
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
