using FplClient.Data;
using FplManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public class SetTeamBuilder
    {
        private const char CaptainChar = 'C';
        private const char ViceCaptainChar = 'V';

        public SetTeamModel BuildTeamToBeSet(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squadAsDictionary)
        {
            var teamPositionLimits = new TeamPositionPlayerLimits();
            var startingTeam = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            var setTeam = new SetTeamModel();

            var captainAndVice = GetCaptainAndVice(squadAsDictionary);
            
            foreach (var position in squadAsDictionary)
            {
                var playersInPosition = position.Value
                    .OrderByDescending(p => p.CurrentTeamEvaluation)
                    .Take(teamPositionLimits.Limits[position.Key].Minimum).ToList();

                startingTeam.Add(position.Key, playersInPosition);
            }

            var remainingPlayers = squadAsDictionary.Select(s => s.Value)
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
            sortedTeam.ForEach(s => setTeam.AddPick(
                s, 
                s.PlayerInfo.Id == captainAndVice[CaptainChar], 
                s.PlayerInfo.Id == captainAndVice[ViceCaptainChar]
             ));

            var subKeeper = benchPlayers.FirstOrDefault(p => p.PlayerInfo.Position.Equals(FplPlayerPosition.Goalkeeper));
            setTeam.AddPick(subKeeper);

            benchPlayers.Remove(subKeeper);

            benchPlayers.Select(p => p)
                .OrderByDescending(s => s.CurrentTeamEvaluation)
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

        private Dictionary<char, int> GetCaptainAndVice(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squadAsDictionary)
        {
            var playersToConsider = 5;
            var capAndVice = new Dictionary<char, int>();

            var initialCandidates = squadAsDictionary.Select(s => s.Value)
                .SelectMany(s => s)
                .OrderByDescending(s => s.CurrentTeamEvaluation)
                .Take(playersToConsider);

            var requiredEval = initialCandidates.Sum(s => s.CurrentTeamEvaluation) / playersToConsider;
            var finalCandidates = initialCandidates.Where(i => i.CurrentTeamEvaluation >= requiredEval)
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            while (finalCandidates.Count < 2)
            {
                var candidateToAdd = initialCandidates.FirstOrDefault(i => !finalCandidates.Any(f => f.PlayerInfo.Id == i.PlayerInfo.Id));
                finalCandidates.Add(candidateToAdd);
            }

            capAndVice.Add(CaptainChar, finalCandidates[0].PlayerInfo.Id);
            capAndVice.Add(ViceCaptainChar, finalCandidates[1].PlayerInfo.Id);

            return capAndVice;
        }
    }
}
