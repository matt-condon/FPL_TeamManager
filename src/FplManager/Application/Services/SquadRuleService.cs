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
        public bool IsValidSquad(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad, int currentCost = SquadRuleConstants.MaxTotalCost, int inBank = 0)
        {
            return MeetsTeamsCriteria(squad) && MeetsCostCriteria(squad, currentCost, inBank);
        }

        private bool MeetsCostCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad, int currentCost, int inBank)
        {
            var newCost = squad.GetSquadCost();
            var availableFunds = currentCost + inBank;
            return MeetsMinCostCriteria() && newCost <= availableFunds;

            bool MeetsMinCostCriteria() => newCost >= SquadRuleConstants.MinTotalCost || newCost > currentCost;
        }

        private bool MeetsTeamsCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var groupedPlayers = squad.Values.SelectMany(list => list).GroupBy(p => p.PlayerInfo.TeamId);
            return !groupedPlayers.Any(g => g.Count() > SquadRuleConstants.MaxPlayersPerTeam);
        }
    }
}
