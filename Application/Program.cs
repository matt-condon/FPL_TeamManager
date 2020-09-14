using FplClient.Clients;
using FplClient.Data;
using FPLTeamManager.Application.Builders;
using FPLTeamManager.Application.Services;
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
            var playerEvaluationService = new PlayerEvaluationService();

            var playerDictionary = dictionaryBuilder.BuildFilteredPlayerDictionary(players);

            Console.WriteLine($"Number of players (after filter): {CountNumberOfPlayers(playerDictionary)}");


            foreach (var position in playerDictionary)
            {
                Console.WriteLine($"Position: {position.Key}");
                Console.WriteLine("");
                var orderedPlayers = position.Value.OrderByDescending(x => playerEvaluationService.EvaluatePlayer(x)).Take(10);

                foreach (var player in orderedPlayers)
                {
                    Console.WriteLine($"{player.GetPartialPlayerString()}");
                    var playerVal = playerEvaluationService.EvaluatePlayer(player);
                    Console.WriteLine($"Player Evaluation: {playerVal}");
                }
                Console.WriteLine("");
            }
        }

        private static async Task<IEnumerable<FplPlayer>> GetTopPlayersAsync()
        {
            var client = new FplPlayerClient(new HttpClient());
            var players = await client.GetAllPlayers();
            return players;
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
