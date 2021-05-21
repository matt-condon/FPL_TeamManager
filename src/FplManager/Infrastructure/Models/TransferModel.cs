namespace FplManager.Infrastructure.Models
{
    public class TransferModel
    {
        public TransferModel(EvaluatedFplPlayer playerOut, EvaluatedFplPlayer playerIn, double evalDiff) 
        {
            PlayerOut = playerOut; 
            PlayerIn = playerIn;
            EvalDifference = evalDiff;
        }

        public EvaluatedFplPlayer PlayerOut { get; private set; }
        public EvaluatedFplPlayer PlayerIn { get; private set; }
        public double EvalDifference { get; private set; }
    }
}
