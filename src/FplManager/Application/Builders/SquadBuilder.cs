using FplClient.Data;
using FplManager.Application.Services;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public class SquadBuilder
    {
        private GameRuleService _gameRuleService = new GameRuleService();

        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByCost(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> players)
        {
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            var squadBuildSuccessful = false;
            var retries = 10;
            while (!squadBuildSuccessful && retries > 0)
            {
                squad = AttemptToBuildSquad(players, out squadBuildSuccessful, retries, out retries);
                Console.WriteLine($"SquadValue = {squad.GetSquadCost()}");
            }

            if (retries == 0 && !squadBuildSuccessful)
            {
                Console.WriteLine($"Ran out of attempts to build squad");
                return new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            }

            return squad;
        }

        //Initial implementation uses weighted randomness to attempt to build squad within cost range
        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> AttemptToBuildSquad(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> playerWishlist, out bool buildSuccesful, int retries, out int retriesRemaining)
        {
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            var playerLimits = new PositionPlayerLimits();
            var rnd = new Random();
            foreach (var position in playerWishlist) 
            {
                var positionPlayerLimit = playerLimits.Limits[position.Key];
                var playersForPosition = position.Value.OrderByDescending(x => GetWeightedRandomEval(x.Evaluation, rnd)).Take(positionPlayerLimit).ToList();
                squad.Add(position.Key, playersForPosition);
            }

            buildSuccesful = _gameRuleService.IsSquadValid(squad);
            retriesRemaining = retries - 1;
            
            return squad;

            static double GetWeightedRandomEval(double weightedEval, Random rnd){
                var randomWeight = ((double)rnd.Next(-20, 20) / 100);
                return weightedEval + randomWeight;
            };
        }
    }
}
