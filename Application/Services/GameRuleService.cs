using FplClient.Data;
using FPLTeamManager.Infrastructure.Extensions;
using FPLTeamManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FPLTeamManager.Application.Services
{
    public class GameRuleService
    {
        private const int MaxCost = 1000;
        private const int MinCost = 950;
        private const int NumberOfPlayersPerTeam = 3;

        public bool IsSquadValid(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            return MeetsTeamsCriteria(squad) && MeetsCostCriteria(squad);
        }

        private bool MeetsCostCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var squadCost = squad.GetSquadCost();
            return squadCost >= MinCost && squadCost <= MaxCost;
        }

        private bool MeetsTeamsCriteria(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var groupedPlayers = squad.Values.SelectMany(list => list).GroupBy(p => p.PlayerInfo.TeamId);
            return groupedPlayers.Any(g => g.Count() > NumberOfPlayersPerTeam);
        }
    }
}
