using FplClient.Data;
using FplManager.Application.Services;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    //Builds initial squad
    //TODO: implement to auto-build
    public class SquadBuilder
    {
        private readonly IPlayerEvaluationService _evaluationService;
        private readonly SquadPositionPlayerLimits _playerLimits;

        public SquadBuilder(IPlayerEvaluationService playerEvaluationService)
        {
            _evaluationService = playerEvaluationService;
            _playerLimits = new SquadPositionPlayerLimits();
        }

        public Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> BuildTeamByCost(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> players, bool isFreeHit)
        {
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            var squadBuildSuccessful = false;
            var retries = 100;
            while (!squadBuildSuccessful && retries > 0)
            {
                squad = AttemptToBuildSquad(players, out squadBuildSuccessful, retries, out retries, isFreeHit);
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
        private Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> AttemptToBuildSquad(
                Dictionary<FplPlayerPosition, 
                List<EvaluatedFplPlayer>> playerWishlist, 
                out bool buildSuccesful, 
                int retries, 
                out int retriesRemaining, 
                bool isFreeHit
            )
        {
            var squad = new Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>>();
            var rnd = new Random();
            foreach (var position in playerWishlist) 
            {
                var positionPlayerLimit = _playerLimits.Limits[position.Key];
                var playersForPosition = position.Value.OrderByDescending(x => GetWeightedRandomEval(EvaluatePlayer(x, isFreeHit), rnd)).Take(positionPlayerLimit).ToList();
                squad.Add(position.Key, playersForPosition);
            }

            buildSuccesful = squad.IsValidSquad();
            retriesRemaining = retries - 1;
            
            return squad;

            double EvaluatePlayer(EvaluatedFplPlayer player, bool isFreeHit){

                if (!isFreeHit)
                    return player.Evaluation;
                return _evaluationService.EvaluateFreeHitPlayer(player);
            };

            static double GetWeightedRandomEval(double weightedEval, Random rnd)
            {
                var randomWeight = ((double)rnd.Next(-20, 20) / 100);
                return weightedEval + randomWeight;
            };
        }
    }
}
