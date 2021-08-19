﻿using FplClient.Data;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Models;

namespace FplManager.Application.Services
{
    public class PlayerEvaluationService
    {
        //simple approach for initial team building :: evaluation based on transfers in/out and ownership (collective intelligence)
        public double EvaluatePlayerByTransfersAndOwnership(FplPlayer player)
        {
            var transferDifferential = (player.TransfersInEvent > player.TransfersOutEvent) ? (1 - (1 / EvaluateTransferDifferential(player))) : (EvaluateTransferDifferential(player) - 1);
            var ownershipMultiplier = 1 - 1 / player.OwnershipPercentage;
            var availabilityDifferential = setAvailabiltyDifferential(player.Status);
            return (transferDifferential * 0.75) + (ownershipMultiplier * 0.75) + availabilityDifferential;
        }

        public double EvaluateCurrentTeamPlayer(FplPlayer player)
        {
            var regEval = EvaluatePlayerByTransfersAndOwnership(player);
            var formDifferential = 1.0 - (1.0 / (player.Form + 1.0));
            var expectedPointsDifferential = 1.0 - (1.0 / (player?.EpNext + 1.0)) ?? 0.0;
            var costDifferential = 1.0 - (140.0 / (player?.NowCost)) ?? 0.0;
            var availabilityDifferential = setAvailabiltyDifferential(player.Status);
            var currentTeamEval = (regEval * 0.25) + (formDifferential * 0.3) + (expectedPointsDifferential * 0.9) + (costDifferential * 0.15) + availabilityDifferential;
            return currentTeamEval;
        }

        public double EvaluateFreeHitPlayer(EvaluatedFplPlayer player)
        {
            var formDifferential = 1 - (1 / (player.PlayerInfo.Form + 1));
            var expectedPointsDifferential = 1 - (1 / (player.PlayerInfo?.EpNext + 1)) ?? 0;
            return (player.Evaluation * 0.4) + (formDifferential * 0.3) + (expectedPointsDifferential * 0.6);
        }

        private double EvaluateTransferDifferential(FplPlayer player)
        {
            return (double)(player.TransfersInEvent + 1) / (player.TransfersOutEvent + 1);
        }

        private double setAvailabiltyDifferential(string playerStatus) =>
            playerStatus switch
            {
                PlayerInfoConstants.AvailableStatus => 0,
                PlayerInfoConstants.DoubtfulStatus => -0.5,
                PlayerInfoConstants.InjuredStatus => -1,
                _ => -1
            };
    }
}
