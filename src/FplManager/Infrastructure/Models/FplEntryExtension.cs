using FplClient.Data;
using Newtonsoft.Json;

namespace FplManager.Infrastructure.Models
{
    public class FplEntryExtension : FplEntry
    {
        [JsonProperty("last_deadline_bank")]
        public int InBankLastGW { get; set; }
    }
}
