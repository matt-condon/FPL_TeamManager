using FplClient.Data;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Services
{
    public class SquadRuleService
    {
        public bool IsValidSquad(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            return MeetsTeamsCriteria(squad) && MeetsCostCriteria(squad);
        }

        private bool MeetsCostCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var squadCost = squad.GetSquadCost();
            return squadCost >= SquadRuleConstants.MinTotalCost 
                && squadCost <= SquadRuleConstants.MaxTotalCost;
        }

        private bool MeetsTeamsCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var groupedPlayers = squad.Values.SelectMany(list => list).GroupBy(p => p.PlayerInfo.TeamId);
            return !groupedPlayers.Any(g => g.Count() > SquadRuleConstants.MaxPlayersPerTeam);
        }
    }
}
