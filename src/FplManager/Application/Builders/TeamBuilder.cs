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
            var fullSquad = BuildFullSquadModel(entryPicks.Picks, allPlayers);

            if (!startingTeamOnly)
            {
                return fullSquad;
            }

            return BuildStartingTeam(fullSquad);
        }
        
        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByMyTeamModel(ICollection<CurrentTeamPick> teamPicks, IEnumerable<FplPlayer> allPlayers, bool startingTeamOnly = true)
        {
            var convertedPicks = teamPicks.ToList<FplPick>();
            var fullSquad = BuildFullSquadModel(convertedPicks, allPlayers);
            
            if (!startingTeamOnly)
            {
                return fullSquad;
            }

            return BuildStartingTeam(fullSquad);
        }
        
        public SetTeamModel BuildSetTeamByMyTeamModel(ICollection<CurrentTeamPick> teamPicks, IEnumerable<FplPlayer> allPlayers)
        {
            var convertedPicks = teamPicks.ToList<FplPick>();
            var fullSquad = BuildFullSquadModel(convertedPicks, allPlayers);
            
            return BuildTeamToBeSet(fullSquad);
        }

        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByPlayerList(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullSquad)
        {
            return BuildStartingTeam(fullSquad);
        }

        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildFullSquadModel(ICollection<FplPick> entryPicks, IEnumerable<FplPlayer> allPlayers)
        {
            var dictionaryBuilder = new PlayerDictionaryBuilder();

            var picksAsPlayers = allPlayers.Where(p => entryPicks.Any(s => s.PlayerId == p.Id)).ToList();
            return dictionaryBuilder.BuildFilteredPlayerDictionary(picksAsPlayers, filterAvailability: false, assignCurrentTeamEvaluation: true);
        }

        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildStartingTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var teamPositionLimits = new TeamPositionPlayerLimits();
            var startingTeam = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            
            foreach (var position in squad)
            {
                var playersInPosition = position.Value
                    .OrderByDescending(p => p.CurrentTeamEvaluation).ToList()
                    .Take(teamPositionLimits.Limits[position.Key].Minimum).ToList();

                startingTeam.Add(position.Key, playersInPosition);
            }

            var benchPlayers = squad.Select(s => s.Value)
                    .SelectMany(s => s)
                    .OrderByDescending(s => s.CurrentTeamEvaluation)
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

        private SetTeamModel BuildTeamToBeSet(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var teamPositionLimits = new TeamPositionPlayerLimits();
            var startingTeam = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            var setTeam = new SetTeamModel();

            var allPlayers = squad.Select(s => s.Value)
                .SelectMany(s => s)
                .OrderByDescending(s => s.CurrentTeamEvaluation)
                .ToArray();

            var captainId = allPlayers[0].PlayerInfo.Id; 
            var viceCaptainId = allPlayers[1].PlayerInfo.Id;


            foreach (var position in squad)
            {
                var playersInPosition = position.Value
                    .OrderByDescending(p => p.CurrentTeamEvaluation).ToList()
                    .Take(teamPositionLimits.Limits[position.Key].Minimum).ToList();

                startingTeam.Add(position.Key, playersInPosition);
            }

            var remainingPlayers = squad.Select(s => s.Value)
                    .SelectMany(s => s)
                    .OrderByDescending(s => s.CurrentTeamEvaluation)
                    .Where(s => !startingTeam.Values.Any(p => p.Any(r => r.PlayerInfo.Id == s.PlayerInfo.Id)))
                    .ToList();

            var benchPlayers = new List<EvaluatedFplPlayer>();
            foreach (var remainingPlayer in remainingPlayers)
            {
                if (!TeamHasMaxInPosition(remainingPlayer.PlayerInfo.Position) && startingTeam.Values.Sum(c => c.Count) != 11)
                {
                    startingTeam[remainingPlayer.PlayerInfo.Position].Add(remainingPlayer);
                }
                else
                {
                    benchPlayers.Add(remainingPlayer);
                }
            }

            var sortedTeam = startingTeam.Select(t => t.Value)
                .SelectMany(p => p)
                .OrderBy(s => GetPositionInt(s.PlayerInfo.Position))
                .ToList();
            sortedTeam.ForEach(s => setTeam.AddPick(s, s.PlayerInfo.Id == captainId, s.PlayerInfo.Id == viceCaptainId));

            benchPlayers.Select(p => p)
                .OrderBy(s => GetPositionInt(s.PlayerInfo.Position))
                .ToList()
                .ForEach(s => setTeam.AddPick(s));

            return setTeam;

            int GetPositionInt(FplPlayerPosition pos)
            {
                var res = (int)pos;
                return res;
            }

            bool TeamHasMaxInPosition(FplPlayerPosition position)
                => teamPositionLimits.Limits[position].Maximum.Equals(startingTeam[position].Count());
        }
    }
}
