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

        public EvaluatedFplPlayer(CurrentFplPlayer currentFplPlayer, double evaluation, double currentTeamEval)
        {
            PlayerInfo = currentFplPlayer.PlayerInfo;
            SellingPrice = currentFplPlayer.SellingPrice;
            Evaluation = evaluation;
            CurrentTeamEvaluation = currentTeamEval;
        }

        public double Evaluation { get; private set; }
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
