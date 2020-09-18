using FplClient.Data;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public class TeamBuilder
    {
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByEntryPicks(FplEntryPicks entryPicks, IEnumerable<FplPlayer> allPlayers, bool startingTeamOnly = true)
        {
            var fullSquad = BuildFullSquadModel(entryPicks, allPlayers);

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

        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFullSquadModel(FplEntryPicks entryPicks, IEnumerable<FplPlayer> allPlayers)
        {
            var dictionaryBuilder = new PlayerDictionaryBuilder();

            var picksAsPlayers = allPlayers.Where(p => entryPicks.Picks.Any(s => s.PlayerId == p.Id));
            return dictionaryBuilder.BuildFilteredPlayerDictionary(picksAsPlayers);
        }

        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildStartingTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var teamPositionLimits = new TeamPositionPlayerLimits();
            var startingTeam = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            
            foreach (var position in squad)
            {
                var playersInPosition = position.Value
                    .OrderByDescending(p => p.PlayerInfo.EpNext).ToList()
                    .Take(teamPositionLimits.Limits[position.Key].Minimum).ToList();

                startingTeam.Add(position.Key, playersInPosition);
            }

            var benchPlayers = squad.Select(s => s.Value)
                    .SelectMany(s => s)
                    .OrderByDescending(s => s.PlayerInfo.EpNext)
                    .Where(s => !startingTeam.Values.Any(p => p.Any(r => r.PlayerInfo.Id == s.PlayerInfo.Id)));

            foreach(var benchPlayer in benchPlayers)
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
                => teamPositionLimits.Limits[position].Maximum.Equals(startingTeam[position].Count());
        }
    }
}
