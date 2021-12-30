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

    public abstract class TeamBuilderBase<T>
    {
        protected readonly IPlayerDictionaryBuilder<T> _playerDictionaryBuilder;
        protected readonly TeamPositionPlayerLimits _teamPositionLimits;

        protected TeamBuilderBase(IPlayerDictionaryBuilder<T> playerDictionaryBuilder)
        {
            _playerDictionaryBuilder = playerDictionaryBuilder;
            _teamPositionLimits = new TeamPositionPlayerLimits();
        }

        protected Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildStartingTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> squad)
        {
            var startingTeam = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();

            foreach (var position in squad)
            {
                var playersInPosition = position.Value
                    .OrderByDescending(p => p.CurrentTeamEvaluation)
                    .Take(_teamPositionLimits.Limits[position.Key].Minimum).ToList();

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
                if (startingTeam.Values.Sum(c => c.Count) == 11)
                {
                    break;
                }
            }

            return startingTeam;

            bool TeamHasMaxInPosition(FplPlayerPosition position)
                => _teamPositionLimits.Limits[position].Maximum.Equals(startingTeam[position].Count);
        }
    }

    public class EntryTeamBuilder : TeamBuilderBase<FplPlayer>, ITeamBuilder<FplPick>
    {
        public EntryTeamBuilder(IPlayerDictionaryBuilder<FplPlayer> playerDictionaryBuilder) : base(playerDictionaryBuilder) { }

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
            var picksAsPlayers = allPlayers.Where(p => entryPicks.Any(s => s.PlayerId == p.Id));
            return _playerDictionaryBuilder.BuildFilteredPlayerDictionary(picksAsPlayers, filterAvailability: false);
        }
    }


    public class CurrentTeamBuilder : TeamBuilderBase<CurrentFplPlayer>, ITeamBuilder<CurrentTeamPick> {
        public CurrentTeamBuilder(IPlayerDictionaryBuilder<CurrentFplPlayer> playerDictionaryBuilder) : base(playerDictionaryBuilder) { }

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
            var picksAsPlayers = allPlayers.Join(entryPicks, p => p.Id, d => d.PlayerId, (fplPlayer, currentPlayer)
            => new CurrentFplPlayer(fplPlayer, currentPlayer.SellingPrice)).ToList();

            return _playerDictionaryBuilder.BuildFilteredPlayerDictionary(picksAsPlayers, filterAvailability: false);
        }
    }
}
