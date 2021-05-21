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
        public bool IsValidSquad(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad, int availableFunds = SquadRuleConstants.MaxTotalCost)
        {
            return MeetsTeamsCriteria(squad) && MeetsCostCriteria(squad, availableFunds);
        }

        private bool MeetsCostCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad, int availableFunds)
        {
            var squadCost = squad.GetSquadCost();
            return squadCost >= SquadRuleConstants.MinTotalCost
                && squadCost <= availableFunds;
        }

        private bool MeetsTeamsCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var groupedPlayers = squad.Values.SelectMany(list => list).GroupBy(p => p.PlayerInfo.TeamId);
            return !groupedPlayers.Any(g => g.Count() > SquadRuleConstants.MaxPlayersPerTeam);
        }
    }
}
