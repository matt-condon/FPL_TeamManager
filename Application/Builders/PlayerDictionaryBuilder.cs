using FplClient.Data;
using FPLTeamManager.Application.Services;
using System.Collections.Generic;
using System.Linq;

namespace FPLTeamManager.Application.Builders
{
    public class PlayerDictionaryBuilder
    {
        public Dictionary<FplPlayerPosition, List<FplPlayer>> BuildFilteredPlayerDictionary(IEnumerable<FplPlayer> players)
        {
            var filteringService = new PlayerFilteringService();
            var availablePlayers = filteringService.FilterAvailablePlayers(players);
            var filteredPlayers = availablePlayers.GroupBy(p => p.Position).ToDictionary(p => p.Key, p => p.ToList());
            return filteredPlayers;
        }
    }
}
