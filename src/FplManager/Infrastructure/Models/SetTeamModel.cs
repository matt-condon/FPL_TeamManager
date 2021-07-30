using FplClient.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Models
{
    public class SetTeamModel
    {
        public SetTeamModel()
        {
            Picks = new List<SetTeamPick>();
        }

        public void AddPick(EvaluatedFplPlayer player, bool isCaptain = false, bool isViceCaptain = false)
        {
            var playerAsPick = new SetTeamPick()
            {
                TeamPosition = Picks.Count + 1,
                PlayerId = player.PlayerInfo.Id,
                IsCaptain = isCaptain,
                IsViceCaptain = isViceCaptain
            };
            Picks.Add(playerAsPick);
        }

        [JsonProperty("picks")]
        public List<SetTeamPick> Picks { get; set; }
        [JsonProperty("chip")]
        public ICollection<CurrentTeamChips> Chip { get; set; }
    }

    public class SetTeamPick
    {
        [JsonProperty("element")]
        public int PlayerId { get; set; }
        [JsonProperty("is_captain")]
        public bool IsCaptain { get; set; }
        [JsonProperty("is_vice_captain")]
        public bool IsViceCaptain { get; set; }
        [JsonProperty("position")]
        public int TeamPosition { get; set; }
    }
}
