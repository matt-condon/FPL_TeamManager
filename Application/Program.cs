using FplClient.Clients;
using FplClient.Data;
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
            foreach (var player in players)
            {
                Console.WriteLine($"{player.GetPartialPlayerString()}");
            }
        }

        private static async Task<IEnumerable<FplPlayer>> GetTopPlayersAsync()
        {
            var client = new FplPlayerClient(new HttpClient());
            var players = await client.GetAllPlayers();
            Console.WriteLine(players.Count);
            return players.OrderByDescending(x => x.TotalPoints).Take(10);
        }
    }
}
