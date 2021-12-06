using Newtonsoft.Json;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Models
{
    public class TransferPayload
    {
        [JsonProperty("chip")]
        public string Chip { get; set; }
        [JsonProperty("entry")]
        public int TeamId { get; set; }
        [JsonProperty("event")]
        public int GameWeek { get; set; }
        [JsonProperty("transfers")]
        public IEnumerable<TransferModel> Transfers { get; set; }
    }
    
    public class TransferModel
    {
        public TransferModel(EvaluatedFplPlayer playerOut, EvaluatedFplPlayer playerIn, double evalDiff, int sellingPrice, int purchasePrice) 
        {
            PlayerOut = playerOut; 
            PlayerIn = playerIn;
            EvalDifference = evalDiff;
            SellingPrice = sellingPrice;
            PurchasePrice = purchasePrice;
            IdOut = playerOut.PlayerInfo.Id;
            IdIn = playerIn.PlayerInfo.Id;
        }

        [JsonProperty("element_out")]
        public int IdOut { get; private set; }
        [JsonProperty("element_in")]
        public int IdIn { get; private set; }
        [JsonProperty("purchase_price")]
        public int PurchasePrice { get; set; }
        [JsonProperty("selling_price")]
        public int SellingPrice { get; set; }
        [JsonIgnore]
        public double EvalDifference { get; set; }
        [JsonIgnore]
        public EvaluatedFplPlayer PlayerOut { get; set; }
        [JsonIgnore]
        public EvaluatedFplPlayer PlayerIn { get; set; }
    }

    public class TransferChip
    {
        [JsonProperty("chip")]
        public string Chip { get; set; }
        [JsonProperty("entry")]
        public int Entry { get; set; }
        [JsonProperty("event")]
        public int Event { get; set; }
    }
}
