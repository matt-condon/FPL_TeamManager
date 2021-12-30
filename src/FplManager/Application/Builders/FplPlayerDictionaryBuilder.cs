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

    public abstract class BasePlayerDictionaryBuilder
    {
        protected readonly IPlayerEvaluationService _playerEvaluationService;
        
        protected BasePlayerDictionaryBuilder(IPlayerEvaluationService playerEvaluationService)
        {
            _playerEvaluationService = playerEvaluationService;
        }
    }

    public class FplPlayerDictionaryBuilder : BasePlayerDictionaryBuilder, IPlayerDictionaryBuilder<FplPlayer>
    {
        public FplPlayerDictionaryBuilder(IPlayerEvaluationService playerEvaluationService) : base(playerEvaluationService) { }

        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<FplPlayer> players, bool filterAvailability = true)
        {
            var availablePlayers = EvaluatePlayers(players, filterAvailability);
            var filteredPlayers = availablePlayers.GroupBy(p => p.PlayerInfo.Position).ToDictionary(p => p.Key, p => p.ToList());
            return filteredPlayers;
        }

        private IEnumerable<EvaluatedFplPlayer> EvaluatePlayers(IEnumerable<FplPlayer> players, bool filterAvailability)
        {
            var filtered = players.Where(p => !filterAvailability || p.Status == PlayerInfoConstants.AvailableStatus)
                .Select(p => new EvaluatedFplPlayer(p, _playerEvaluationService.EvaluatePlayerByTransfersAndOwnership(p)))
                .ToList();

            return filtered;
        }
    }

    public class CurrentFplPlayerDictionaryBuilder : BasePlayerDictionaryBuilder, IPlayerDictionaryBuilder<CurrentFplPlayer>
    {
        public CurrentFplPlayerDictionaryBuilder(IPlayerEvaluationService playerEvaluationService) : base(playerEvaluationService) { }

        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<CurrentFplPlayer> players, bool filterAvailability = true)
        {
            var evaluatedPlayers = EvaluatePlayers(players);
            var filteredPlayers = evaluatedPlayers.GroupBy(p => p.PlayerInfo.Position).ToDictionary(p => p.Key, p => p.ToList());

            return filteredPlayers;
        }

        private IEnumerable<EvaluatedFplPlayer> EvaluatePlayers(IEnumerable<CurrentFplPlayer> players)
        {
            var filtered = players.Select(
                p => new EvaluatedFplPlayer(
                    p, 
                    _playerEvaluationService.EvaluatePlayerByTransfersAndOwnership(p.PlayerInfo),
                    _playerEvaluationService.EvaluateCurrentTeamPlayer(p.PlayerInfo),
                    _playerEvaluationService.EvaluateTransferListViability(p.PlayerInfo)))
                .ToList();

            return filtered;
        }
    }
}
