using FplClient.Data;
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
            return (transferDifferential * 0.75) + (ownershipMultiplier * 0.75);
        }

        public double EvaluateCurrentTeamPlayer(EvaluatedFplPlayer player)
        {
            var formDifferential = 1.0 - (1.0 / (player.PlayerInfo.Form + 1.0));
            var expectedPointsDifferential = 1.0 - (1.0 / (player.PlayerInfo?.EpNext + 1.0)) ?? 0.0;
            var costDifferential = 1.0 - (140.0 / ((double?) player.PlayerInfo?.NowCost)) ?? 0.0;
            return (player.Evaluation * 0.3) + (formDifferential * 0.3) + (expectedPointsDifferential * 0.3) + (costDifferential * 0.1);
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
    }
}
