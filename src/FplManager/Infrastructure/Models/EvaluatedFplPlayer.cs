using FplClient.Data;

namespace FplManager.Infrastructure.Models
{
    public class EvaluatedFplPlayer
    {
        public EvaluatedFplPlayer(FplPlayer playerInfo, double evaluation) 
        { 
            PlayerInfo = playerInfo;
            Evaluation = evaluation;
        }

        public FplPlayer PlayerInfo { get; private set; }
        public double Evaluation { get; private set; }
    }
}
