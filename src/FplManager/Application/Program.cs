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

            var players = await GetPlayersAsync(httpClient);

            await PrintExistingTeamsAsync(httpClient, players);

            // PrintNewSquad(players);
        }

        private static async Task PrintExistingTeamsAsync(HttpClient http, IEnumerable<FplPlayer> players)
        {
            var fplTeams = new Dictionary<string, int>()
            {
                //{ "testBot", 6184667 },
                //{ "prodBot", 6183774 },
                { "myTeam", 357852 }
            };
            var teamBuilder = new TeamBuilder();
            foreach (var team in fplTeams)
            {
                var pickSelection = await GetPicksAsync(http, team.Value);

                var firstEleven = teamBuilder.BuildTeamByEntryPicks(pickSelection, players, false);

                Console.WriteLine(firstEleven.GetSquadString());
            }
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
