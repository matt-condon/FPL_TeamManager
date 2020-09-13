using FplClient.Clients;
using FplClient.Data;
using FPLTeamManager.Application.Builders;
using FPLTeamManager.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FPLTeamManager.Application
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var players = await GetTopPlayersAsync();

            var dictionaryBuilder = new PlayerDictionaryBuilder();
            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);


            Console.WriteLine($"Number of players (after filter): {CountNumberOfPlayers(playerDictionary)}");

            foreach (var position in playerDictionary)
            {
                Console.WriteLine($"Position: {position.Key}");
                Console.WriteLine("");

                foreach (var player in position.Value)
                {
                    Console.WriteLine($"{player.GetPartialPlayerString()}");
                }
                Console.WriteLine("");
            }
        }

        private static async Task<IEnumerable<FplPlayer>> GetTopPlayersAsync()
        {
            var client = new FplPlayerClient(new HttpClient());
            var players = await client.GetAllPlayers();
            Console.WriteLine($"Number of players: {players.Count}");
            return players.OrderByDescending(x => x.TotalPoints).Take(12);
        }

        private static int CountNumberOfPlayers(Dictionary<FplPlayerPosition, List<FplPlayer>> playerDictionary)
        {
            return playerDictionary.Values
                .SelectMany(list => list)
                .Distinct()
                .Count();
        }
    }
}
