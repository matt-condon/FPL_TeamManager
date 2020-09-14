using FplClient.Data;

namespace FPLTeamManager.Application.Services
{
    public class PlayerEvaluationService
    {
        public double EvaluatePlayer(FplPlayer player)
        {
            var evaluation = player.PointsPerGame + player.ValueSeason;
            var playerValue = player.NowCost / 10.0;
            var transferDifferential = (player.TransfersInEvent > player.TransfersOutEvent) ? (1 - (1 / EvaluateTransferDifferential(player))) : (EvaluateTransferDifferential(player) - 1);
            var ownerMultiplier = 1 - 1 / player.OwnershipPercentage;
            return (transferDifferential * 0.75) + (ownerMultiplier * 0.25);
        }

        private double EvaluateTransferDifferential(FplPlayer player)
        {
            return (double)(player.TransfersInEvent + 1) / (player.TransfersOutEvent + 1);
        }
    }
}
