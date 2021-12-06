using FplClient.Data;
using FplManager.Application.Services;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public interface IPlayerDictionaryBuilder<T>
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<T> players, bool filterAvailability = true);
    }

    public class FplPlayerDictionaryBuilder : IPlayerDictionaryBuilder<FplPlayer>
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<FplPlayer> players, bool filterAvailability = true)
        {
            var availablePlayers = EvaluatePlayers(players, filterAvailability);
            var filteredPlayers = availablePlayers.GroupBy(p => p.PlayerInfo.Position).ToDictionary(p => p.Key, p => p.ToList());
            return filteredPlayers;
        }

        private IEnumerable<EvaluatedFplPlayer> EvaluatePlayers(IEnumerable<FplPlayer> players, bool filterAvailability)
        {
            var playerEvaluationService = new PlayerEvaluationService();
            var filtered = players.Where(p => !filterAvailability || p.Status == PlayerInfoConstants.AvailableStatus)
                .Select(p => new EvaluatedFplPlayer(p, playerEvaluationService.EvaluatePlayerByTransfersAndOwnership(p)))
                .ToList();

            return filtered;
        }
    }

    public class CurrentFplPlayerDictionaryBuilder : IPlayerDictionaryBuilder<CurrentFplPlayer>
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<CurrentFplPlayer> players, bool filterAvailability = true)
        {
            var evaluatedPlayers = EvaluatePlayers(players);
            var filteredPlayers = evaluatedPlayers.GroupBy(p => p.PlayerInfo.Position).ToDictionary(p => p.Key, p => p.ToList());

            return filteredPlayers;
        }

        private IEnumerable<EvaluatedFplPlayer> EvaluatePlayers(IEnumerable<CurrentFplPlayer> players)
        {
            var playerEvaluationService = new PlayerEvaluationService();
            var filtered = players.Select(
                p => new EvaluatedFplPlayer(
                    p, 
                    playerEvaluationService.EvaluatePlayerByTransfersAndOwnership(p.PlayerInfo),
                    playerEvaluationService.EvaluateCurrentTeamPlayer(p.PlayerInfo),
                    playerEvaluationService.EvaluateTransferListViability(p.PlayerInfo)))
                .ToList();

            return filtered;
        }
    }
}
