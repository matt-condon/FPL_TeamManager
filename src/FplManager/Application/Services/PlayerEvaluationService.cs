using FplClient.Data;

namespace FplManager.Application.Services
{
    public class PlayerEvaluationService
    {
        //simple approach for initial team building :: evaluation based on transfers in/out and ownership (collective intelligence)
        public double EvaluatePlayerByTransfersAndOwnership(FplPlayer player)
        {
            var transferDifferential = (player.TransfersInEvent > player.TransfersOutEvent) ? (1 - (1 / EvaluateTransferDifferential(player))) : (EvaluateTransferDifferential(player) - 1);
            var ownershipMultiplier = 1 - 1 / player.OwnershipPercentage;
            return (transferDifferential * 0.75) + (ownershipMultiplier * 0.25);
        }

        private double EvaluateTransferDifferential(FplPlayer player)
        {
            return (double)(player.TransfersInEvent + 1) / (player.TransfersOutEvent + 1);
        }
    }
}
