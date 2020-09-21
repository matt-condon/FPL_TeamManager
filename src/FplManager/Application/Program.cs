using FplClient.Clients;
using FplClient.Data;
using FplManager.Application.Builders;
using FplManager.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
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
            var fplTeamId = 357852;
            /* Bot Team */
            //var fplTeamId = 6184667;
            /* Test Bot Team */
            //var fplTeamId = 6183774;

            var players = await GetPlayersAsync(httpClient);
            var pickSelection = await GetPicksAsync(httpClient, fplTeamId);

            //PrintExistingTeamsAsync(players, pickSelection);
            // PrintNewSquad(players);
            PrintTeamTransferWishlist(players, pickSelection);
        }

        private static void PrintExistingTeamsAsync(IEnumerable<FplPlayer> players, FplEntryPicks pickSelection)
        {
            
            var teamBuilder = new TeamBuilder();
            var firstEleven = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, false);
            Console.WriteLine(firstEleven.GetSquadString());
        }

        private static void PrintNewSquad(IEnumerable<FplPlayer> players)
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
            var teamBuilder = new TeamBuilder();
            var currentTeam = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, startingTeamOnly: false);
            var wishlistBuilder = new TransferWishlistBuilder();
            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);

            var wishlist = wishlistBuilder.BuildTransferWishlist(playerDictionary, currentTeam);
            Console.WriteLine(wishlist.GetWishlistString());
        }

        private static async Task<IEnumerable<FplPlayer>> GetPlayersAsync(HttpClient http)
        {
            var client = new FplPlayerClient(http);
            var players = await client.GetAllPlayers();
            return players;
        }

        private static async Task<FplEntryPicks> GetPicksAsync(HttpClient http, int teamId)
        {
            var client = new FplEntryClient(http);
            return await client.GetPicks(teamId, 2);
        }
    }
}
