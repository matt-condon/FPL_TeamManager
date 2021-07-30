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
        static TeamBuilder _teamBuilder;

        static async Task Main(string[] args)
        {
            _teamBuilder = new TeamBuilder();


            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            var httpClient = new HttpClient(handler);

            //var account = "FIJIFE@GMAIL.COM";
            //var fplTeamId = 1445605;

            //var account = "bobbsbobbs@gmail.com";
            //var fplTeamId = 1223804;

            //var account = "matt.condon@ucdconnect.ie";
            //var fplTeamId = 1224412;

            var account = "mattoconduin@gmail.com";
            var fplTeamId = 1828488;

            await AuthenticateAsync(httpClient, account, "PASSWORD_HERE");
            var currentTeam = await GetTeamAsync(httpClient, fplTeamId);
            var allPlayers = await GetPlayersAsync(httpClient);
            var fullTeam = _teamBuilder.BuildTeamByMyTeamModel(currentTeam.Picks, allPlayers, startingTeamOnly: false);

            await SelectCurrentTeam(fullTeam, httpClient, fplTeamId);
            PrintCurrentTeam(fullTeam);

            var transferList = GetSquadTransferList(fullTeam);
            PrintSquadTransferList(transferList);

            //var gameweek = await GetCurrentGameweek(httpClient);
            //var pickSelection = await GetPicksAsync(httpClient, fplTeamId, gameweek);
            //var fplEntry = await GetFplEntryAsync(httpClient, fplTeamId);

            //PrintNewSquad(players);


            //PrintExistingTeam(players, pickSelection);
            //PrintTeamTransferWishlist(players, pickSelection);
            //PrintSquadTransferList(allPlayers, pickSelection);
            //PrintTransferSelection(players, pickSelection, fplEntry);
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

        public static async Task<MyTeamModel> GetTeamAsync(HttpClient httpClient, int fplTeamId)
        {
            try
            {
                var getTeamResponse = await httpClient.GetAsync($"https://fantasy.premierleague.com/api/my-team/{fplTeamId}/");
                var teamAsString = getTeamResponse.Content.ReadAsStringAsync().Result;
                var teamAsPicks = JsonConvert.DeserializeObject<MyTeamModel>(teamAsString);
                return teamAsPicks;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to get team, error: {e}");
            }

            return null;
        }

        private static void PrintExistingTeam(IEnumerable<FplPlayer> players, FplEntryPicks pickSelection)
        {
            var firstEleven = _teamBuilder.BuildTeamByEntryPicks(pickSelection, players, true);
            Console.WriteLine(firstEleven.GetSquadString());
        }

        private static void PrintCurrentTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullTeam)
        {
            Console.WriteLine(fullTeam.GetSquadString());
        }
        
        private static async Task SelectCurrentTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullTeam, HttpClient httpClient, int fplTeamId)
        {
            var setTeam = _teamBuilder.BuildTeamToBeSet(fullTeam);

            string json = JsonConvert.SerializeObject(setTeam);
            StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var getTeamResponse = await httpClient.PostAsync(
                $"https://fantasy.premierleague.com/api/my-team/{fplTeamId}/",
                httpContent
            );

            if (getTeamResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Select Current Team Failed. StatusCode: {getTeamResponse.StatusCode}");
            }
        }

        private static void PrintNewSquad(IEnumerable<FplPlayer> players, bool isFreeHit = false)
        {
            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);

            Console.WriteLine($"Number of players (after filter): {playerDictionary.CountNumberOfPlayers()}");

            var squadBuilder = new SquadBuilder();
            var squad = squadBuilder.BuildTeamByCost(playerDictionary, isFreeHit);

            Console.WriteLine(squad.GetSquadString());
            Console.WriteLine($"Squad Cost: {squad.GetSquadCost()}");
        }
        
        private static void PrintTeamTransferWishlist(IEnumerable<FplPlayer> players, FplEntryPicks pickSelection)
        {
            Console.WriteLine("Transfer Wishlist:");
            Console.WriteLine("------------------------------------------");

            var teamBuilder = new TeamBuilder();
            var currentTeam = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, startingTeamOnly: false);
            var wishlistBuilder = new TransferWishlistBuilder();
            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);

            var wishlist = wishlistBuilder.BuildTransferTargetWishlist(playerDictionary, currentTeam);
            Console.WriteLine(wishlist.GetWishlistString());
        }

        private static List<EvaluatedFplPlayer> GetSquadTransferList(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullTeam)
        {
            var wishlistBuilder = new TransferWishlistBuilder();
            return wishlistBuilder.BuildSquadTransferList(fullTeam);
        }

        private static void PrintSquadTransferList(IEnumerable<FplPlayer> players, FplEntryPicks pickSelection)
        {
            Console.WriteLine("Squad Transfer List:");
            Console.WriteLine("------------------------------------------");

            var teamBuilder = new TeamBuilder();
            var currentTeam = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, startingTeamOnly: false);
            var wishlistBuilder = new TransferWishlistBuilder();
            var wishlist = wishlistBuilder.BuildSquadTransferList(currentTeam);
            Console.WriteLine(wishlist.GetWishlistString());
        }

        private static void PrintSquadTransferList(List<EvaluatedFplPlayer> wishlist)
        {
            Console.WriteLine("Squad Transfer List:");
            Console.WriteLine("------------------------------------------");

            Console.WriteLine(wishlist.GetWishlistString());
        }

        private static void PrintTransferSelection(IEnumerable<FplPlayer> players, FplEntryPicks pickSelection, FplEntryExtension fplEntry)
        {
            var teamBuilder = new TeamBuilder();
            var currentTeam = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, startingTeamOnly: false);
            var transferSelectorService = new TransferSelectorService();
            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);
            var inBank = fplEntry.InBankLastGW;

            var transfer = transferSelectorService.SelectTransfer(currentTeam, playerDictionary, inBank);
            Console.WriteLine("In:");
            Console.WriteLine(transfer.PlayerIn.PlayerInfo.GetPartialPlayerString());

            Console.WriteLine("Out:");
            Console.WriteLine(transfer.PlayerOut.PlayerInfo.GetPartialPlayerString());
        }

        private static async Task<int> GetCurrentGameweek(HttpClient httpClient)
        {
            var client = new FplGameweekClient(httpClient);
            var fixtures = await client.GetGameweeks();

            //var currentDate = DateTime.Now;
            return fixtures.Where(n => n.IsCurrent).First().Id;
        }

        private static async Task<List<FplPlayer>> GetPlayersAsync(HttpClient http)
        {
            var client = new FplPlayerClient(http);
            var players = await client.GetAllPlayers();
            return players.ToList();
        }

        private static async Task<FplEntryPicks> GetPicksAsync(HttpClient http, int teamId, int gameWeek)
        {
            var client = new FplEntryClient(http);
            return await client.GetPicks(teamId, gameWeek);
        }
        
        private static async Task<FplEntryExtension> GetFplEntryAsync(HttpClient http, int teamId)
        {
            var client = new CustomFplEntryClient(http);
            return await client.Get(teamId);
        }
    }
}
