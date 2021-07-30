using FplClient.Data;

namespace FplManager.Infrastructure.Models
{
    public class EvaluatedFplPlayer
    {
        private const double DefaultEval = -999;

        public EvaluatedFplPlayer(FplPlayer playerInfo, double evaluation) 
        { 
            PlayerInfo = playerInfo;
            Evaluation = evaluation;
            CurrentTeamEvaluation = DefaultEval;
        }

        public FplPlayer PlayerInfo { get; private set; }
        public double Evaluation { get; private set; }
        public double CurrentTeamEvaluation { get; set; }
    }
}
