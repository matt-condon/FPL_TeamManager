using FplClient.Data;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Infrastructure.Extensions
{
    public static class SquadExtensions
    {
        public static int CountNumberOfPlayers(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            return squad.Values
                .SelectMany(list => list)
                .Distinct()
                .Count();
        }

        public static int GetSquadCost(this Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            return squad.Values
                .SelectMany(list => list)
                .Sum(p => p.PlayerInfo.NowCost);
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
    }
}
