using FplClient.Data;
using System.Collections.Generic;
using System.Linq;

namespace FPLTeamManager.Application.Services
{
    public class PlayerFilteringService
    {
        public IEnumerable<FplPlayer> FilterAvailablePlayers(IEnumerable<FplPlayer> players)
        {
            var availableStatus = "a";
            var filtered = players.Where(p => p.Status == availableStatus);
            return filtered;
        }
    }
}
