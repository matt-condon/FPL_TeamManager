using FplClient.Data;
using FplManager.Application.Services;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public class PlayerDictionaryBuilder
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<FplPlayer> players, bool filterAvailability = true, bool assignCurrentTeamEvaluation = false)
        {
            var availablePlayers = EvaluatePlayers(players, filterAvailability, assignCurrentTeamEvaluation);
            var filteredPlayers = availablePlayers.GroupBy(p => p.PlayerInfo.Position).ToDictionary(p => p.Key, p => p.ToList());
            return filteredPlayers;
        }

        private IEnumerable<EvaluatedFplPlayer> EvaluatePlayers(IEnumerable<FplPlayer> players, bool filterAvailability, bool assignCurrentTeamEvaluation)
        {
            var availableStatus = "a";
            var playerEvaluationService = new PlayerEvaluationService();
            var filtered = players.Where(p => !filterAvailability || p.Status == availableStatus)
                .Select(p => new EvaluatedFplPlayer(p, playerEvaluationService.EvaluatePlayerByTransfersAndOwnership(p)))
                .ToList();

            if (assignCurrentTeamEvaluation)
            {
                filtered.ForEach(p => p.CurrentTeamEvaluation = playerEvaluationService.EvaluateCurrentTeamPlayer(p));
            }

            return filtered;
        }
    }
}
