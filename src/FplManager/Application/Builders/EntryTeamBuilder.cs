using FplClient.Data;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public interface ITeamBuilder<T>
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByPicks(ICollection<T> picks, IEnumerable<FplPlayer> allPlayers, bool startingTeamOnly = true);
    }

    public abstract class TeamBuilderBase
    {
        protected Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildStartingTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var teamPositionLimits = new TeamPositionPlayerLimits();
            var startingTeam = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();

            foreach (var position in squad)
            {
                var playersInPosition = position.Value
                    .OrderByDescending(p => p.CurrentTeamEvaluation)
                    .Take(teamPositionLimits.Limits[position.Key].Minimum).ToList();

                startingTeam.Add(position.Key, playersInPosition);
            }

            var benchPlayers = squad.Select(s => s.Value)
                    .SelectMany(s => s)
                    .OrderByDescending(s => s.CurrentTeamEvaluation)
                    .Where(s => !startingTeam.Values.Any(p => p.Any(r => r.PlayerInfo.Id == s.PlayerInfo.Id)));

            foreach (var benchPlayer in benchPlayers)
            {
                if (!TeamHasMaxInPosition(benchPlayer.PlayerInfo.Position))
                {
                    startingTeam[benchPlayer.PlayerInfo.Position].Add(benchPlayer);
                }
                if (startingTeam.Values.Sum(c => c.Count()) == 11)
                {
                    break;
                }
            }

            return startingTeam;

            bool TeamHasMaxInPosition(FplPlayerPosition position)
                => teamPositionLimits.Limits[position].Maximum.Equals(startingTeam[position].Count);
        }
    }

    public class EntryTeamBuilder : TeamBuilderBase, ITeamBuilder<FplPick>
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByPicks(ICollection<FplPick> picks, IEnumerable<FplPlayer> allPlayers, bool startingTeamOnly = true)
        {
            var fullSquad = BuildFullSquadModel(picks, allPlayers);

            if (!startingTeamOnly)
            {
                return fullSquad;
            }

            return BuildStartingTeam(fullSquad);
        }

        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFullSquadModel(ICollection<FplPick> entryPicks, IEnumerable<FplPlayer> allPlayers)
        {
            var dictionaryBuilder = new FplPlayerDictionaryBuilder();

            var picksAsPlayers = allPlayers.Where(p => entryPicks.Any(s => s.PlayerId == p.Id)).ToList();
            return dictionaryBuilder.BuildFilteredPlayerDictionary(picksAsPlayers, filterAvailability: false);
        }
    }


    public class CurrentTeamBuilder : TeamBuilderBase, ITeamBuilder<CurrentTeamPick> { 
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByPicks(ICollection<CurrentTeamPick> picks, IEnumerable<FplPlayer> allPlayers, bool startingTeamOnly = true)
        {
            var fullSquad = BuildFullSquadModel(picks, allPlayers);
            
            if (!startingTeamOnly)
            {
                return fullSquad;
            }

            return BuildStartingTeam(fullSquad);
        }
        
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByPlayerList(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullSquad)
        {
            return BuildStartingTeam(fullSquad);
        }

        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFullSquadModel(ICollection<CurrentTeamPick> entryPicks, IEnumerable<FplPlayer> allPlayers)
        {
            var dictionaryBuilder = new CurrentFplPlayerDictionaryBuilder();

            var picksAsPlayers = allPlayers.Join(entryPicks, p => p.Id, d => d.PlayerId, (fplPlayer, currentPlayer)
            => new CurrentFplPlayer(fplPlayer, currentPlayer.SellingPrice)).ToList();

            return dictionaryBuilder.BuildFilteredPlayerDictionary(picksAsPlayers, filterAvailability: false);
        }
    }
}
