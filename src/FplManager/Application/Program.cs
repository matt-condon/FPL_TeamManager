using FplClient.Clients;
using FplClient.Data;
using FplManager.Application.Builders;
using FplManager.Application.Services;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.FplClients;
using FplManager.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FplManager.Application
{
    class Program
    {
        private static TeamOrchestratorService _teamOrchestratorService;

        static async Task Main(string[] args)
        {
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            var httpClient = new HttpClient(handler);

            _teamOrchestratorService = new TeamOrchestratorService(httpClient);

            //var account = "fijife@gmail.com";
            //var fplTeamId = 1445605;

            //var account = "bobbsbobbs@gmail.com";
            //var fplTeamId = 1223804;

            //var account = "matt.condon@ucdconnect.ie";
            //var fplTeamId = 1224412;

            var account = "mattoconduin@gmail.com";
            var fplTeamId = 1828488;

            await AuthenticateAsync(httpClient, account, "PASSWORD_HERE");

            await _teamOrchestratorService.ManageTeam(fplTeamId, numberOfTransfers: 0);
        }

        //You need to authenticate, then you need the pl_profile cookie for every subsequent request
        public static async Task AuthenticateAsync(HttpClient httpClient, string login, string password)
        {
            try
            {
                var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("login", login),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("app", "plfpl-web"),
                    new KeyValuePair<string, string>("redirect_uri", "https://fantasy.premierleague.com/")
                };

                var uri = new Uri("https://users.premierleague.com/accounts/login/");
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = uri,
                    Method = HttpMethod.Post,
                    Content = new FormUrlEncodedContent(nvc)
                };

                await httpClient.SendAsync(httpRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Login failed for account: {login}, error: {e}");
            }
        }
    }
}
