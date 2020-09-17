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
            var players = await GetPlayersAsync();

            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);

            Console.WriteLine($"Number of players (after filter): {playerDictionary.CountNumberOfPlayers()}");

            var squadBuilder = new SquadBuilder();
            var squad = squadBuilder.BuildTeamByCost(playerDictionary);

            Console.WriteLine($"Number of players in squad: {squad.CountNumberOfPlayers()}");
            Console.WriteLine(squad.GetSquadString());
            Console.WriteLine($"Squad Cost: {squad.GetSquadCost()}");
        }

        private static async Task<IEnumerable<FplPlayer>> GetPlayersAsync()
        {
            var client = new FplPlayerClient(new HttpClient());
            var players = await client.GetAllPlayers();
            return players;
        }
    }
}
