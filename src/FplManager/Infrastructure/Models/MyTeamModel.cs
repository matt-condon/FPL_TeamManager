using FplClient.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Models
{
    public class MyTeamModel
    {
        [JsonProperty("picks")]
        public ICollection<CurrentTeamPick> Picks { get; set; }
        [JsonProperty("chips")]
        public ICollection<CurrentTeamChips> Chips { get; set; }
        [JsonProperty("transfers")]
        public CurrentTeamTransfers Transfers { get; set; }
    }

    public class CurrentTeamPick : FplPick
    {
        [JsonProperty("position")]
        public int Position { get; set; }
        [JsonProperty("selling_price")]
        public int SellingPrice { get; set; }
        [JsonProperty("purchase_price")]
        public int PurchasePrice { get; set; }
    }
    
    public class CurrentTeamChips
    {
        [JsonProperty("status_for_entry")]
        public string Status { get; set; }
        [JsonProperty("name")]
        public string Name{ get; set; }
    }
    
    public class CurrentTeamTransfers
    {
        [JsonProperty("cost")]
        public int Cost { get; set; }
        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("made")]
        public int Made { get; set; }
        [JsonProperty("bank")]
        public int Bank { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
