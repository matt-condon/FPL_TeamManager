using FplClient.Data;
using FPLTeamManager.Application.Services;
using FPLTeamManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FPLTeamManager.Application.Builders
{
    public class PlayerDictionaryBuilder
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<FplPlayer> players)
        {
            var availablePlayers = EvaluateAvailablePlayers(players);
            var filteredPlayers = availablePlayers.GroupBy(p => p.PlayerInfo.Position).ToDictionary(p => p.Key, p => p.ToList());
            return filteredPlayers;
        }

        private IEnumerable<EvaluatedFplPlayer> EvaluateAvailablePlayers(IEnumerable<FplPlayer> players)
        {
            var availableStatus = "a";
            var playerEvaluationService = new PlayerEvaluationService();
            var filtered = players.Where(p => p.Status == availableStatus)
                .Select(p => new EvaluatedFplPlayer(p, playerEvaluationService.EvaluatePlayer(p)));
            return filtered;
        }
    }
}
