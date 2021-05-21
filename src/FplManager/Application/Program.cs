using FplClient.Clients;
using FplClient.Data;
using FplManager.Application.Builders;
using FplManager.Application.Services;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.FplClients;
using FplManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FplManager.Application
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();

            /* My Team */
            //var fplTeamId = 357852;
            //fijife
            //var fplTeamId = 6184667;
            //bobbs
            var fplTeamId = 6440220;
            //ucd
            //var fplTeamId = 6441505;
            //mattoc
            //var fplTeamId = 6183774;

            var gameweek = await GetCurrentGameweek(httpClient);
            var players = await GetPlayersAsync(httpClient);
            var pickSelection = await GetPicksAsync(httpClient, fplTeamId, gameweek);
            var fplEntry = await GetFplEntryAsync(httpClient, fplTeamId);

            //PrintNewSquad(players, isFreeHit: true);

            PrintExistingTeam(players, pickSelection);
            //PrintTeamTransferWishlist(players, pickSelection);
            //PrintSquadTransferList(players, pickSelection);
            PrintTransferSelection(players, pickSelection, fplEntry);
        }

        private static void PrintExistingTeam(IEnumerable<FplPlayer> players, FplEntryPicks pickSelection)
        {
            var teamBuilder = new TeamBuilder();
            var firstEleven = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, true);
            Console.WriteLine(firstEleven.GetSquadString());
        }

        private static void PrintNewSquad(IEnumerable<FplPlayer> players, bool isFreeHit)
        {
            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);

            Console.WriteLine($"Number of players (after filter): {playerDictionary.CountNumberOfPlayers()}");

            var squadBuilder = new SquadBuilder();
            var squad = squadBuilder.BuildTeamByCost(playerDictionary);

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

        private static async Task<IEnumerable<FplPlayer>> GetPlayersAsync(HttpClient http)
        {
            var client = new FplPlayerClient(http);
            var players = await client.GetAllPlayers();
            return players;
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
