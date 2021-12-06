using FplClient.Data;

namespace FplManager.Infrastructure.Models
{
    public class EvaluatedFplPlayer : CurrentFplPlayer
    {
        private const int DefaultSellingPrice = 999;

        public EvaluatedFplPlayer(FplPlayer playerInfo, double evaluation, double currentTeamEval = DefaultEval)
        {
            PlayerInfo = playerInfo;
            Evaluation = evaluation;
            CurrentTeamEvaluation = currentTeamEval;
            SellingPrice = DefaultSellingPrice;
        }

        public EvaluatedFplPlayer(CurrentFplPlayer currentFplPlayer, double evaluation, double currentTeamEval, double transferListViability = 0)
        {
            PlayerInfo = currentFplPlayer.PlayerInfo;
            SellingPrice = currentFplPlayer.SellingPrice;
            Evaluation = evaluation;
            CurrentTeamEvaluation = currentTeamEval;
            TransferListViability = transferListViability;
        }

        public double Evaluation { get; private set; }
        public double TransferListViability { get; set; }
    }

    public class CurrentFplPlayer
    {
        protected const double DefaultEval = -999;

        public CurrentFplPlayer() { }

        public CurrentFplPlayer(FplPlayer playerInfo, int sellingPrice, double currentTeamEval = DefaultEval)
        {
            PlayerInfo = playerInfo;
            SellingPrice = sellingPrice;
            CurrentTeamEvaluation = currentTeamEval;
        }
        
        public FplPlayer PlayerInfo { get; protected set; }
        public int SellingPrice { get; set; }
        public double CurrentTeamEvaluation { get; set; }
    }
}
