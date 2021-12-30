using FplClient.Data;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Infrastructure.Extensions
{
    public static class SquadExtensions
    {
        public static bool IsValidSquad(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad, int currentCost = SquadRuleConstants.MaxTotalCost, int inBank = 0)
        {
            return squad.MeetsTeamsCriteria() && squad.MeetsCostCriteria(currentCost, inBank);
        }

        public static int GetSquadCost(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            return squad.Values
                .SelectMany(list => list)
                .Sum(p => GetCost(p));

            int GetCost(EvaluatedFplPlayer player) => (player.SellingPrice != 999) ? player.SellingPrice : player.PlayerInfo.NowCost;
        }

        public static string GetSquadString(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var squadString = string.Empty;
            foreach (var position in squad)
            {
                squadString = squadString.ConcatWithNewLine($"Position: {position.Key}");
                squadString = squadString.ConcatWithNewLine("----------------------------------");

                foreach (var player in position.Value)
                {
                    squadString = squadString.ConcatWithNewLine($"{player.PlayerInfo.GetPartialPlayerString()}");
                    squadString = squadString.ConcatWithNewLine($"Transfer Evaluation: {player.Evaluation}");
                    squadString = squadString.ConcatWithNewLine($"Selection Evaluation: {player.CurrentTeamEvaluation}");
                    squadString = squadString.ConcatWithNewLine("");
                }
            }
            return squadString;
        }

        private static bool MeetsCostCriteria(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad, int currentCost, int inBank)
        {
            var newCost = squad.GetSquadCost();
            var availableFunds = currentCost + inBank;
            return MeetsMinCostCriteria() && newCost <= availableFunds;

            bool MeetsMinCostCriteria() => newCost >= SquadRuleConstants.MinTotalCost || newCost > currentCost;
        }

        private static bool MeetsTeamsCriteria(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var groupedPlayers = squad.Values.SelectMany(list => list).GroupBy(p => p.PlayerInfo.TeamId);
            return !groupedPlayers.Any(g => g.Count() > SquadRuleConstants.MaxPlayersPerTeam);
        }
    }
}
