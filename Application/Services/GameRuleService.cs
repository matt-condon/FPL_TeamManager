using FplClient.Data;
using FPLTeamManager.Infrastructure.Constants;
using FPLTeamManager.Infrastructure.Extensions;
using FPLTeamManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FPLTeamManager.Application.Services
{
    public class GameRuleService
    {
        public bool IsSquadValid(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            return MeetsTeamsCriteria(squad) && MeetsCostCriteria(squad);
        }

        private bool MeetsCostCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var squadCost = squad.GetSquadCost();
            return squadCost >= GameRuleConstants.MinTotalCost 
                && squadCost <= GameRuleConstants.MaxTotalCost;
        }

        private bool MeetsTeamsCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var groupedPlayers = squad.Values.SelectMany(list => list).GroupBy(p => p.PlayerInfo.TeamId);
            return !groupedPlayers.Any(g => g.Count() > GameRuleConstants.MaxPlayersPerTeam);
        }
    }
}
