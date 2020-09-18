using FplClient.Data;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Models
{
    public class SquadPositionPlayerLimits
    {
        public SquadPositionPlayerLimits()
        {
            Limits = new Dictionary<FplPlayerPosition, int>
            {
                {
                    FplPlayerPosition.Goalkeeper,
                    2
                },
                {
                    FplPlayerPosition.Defender,
                    5
                },
                {
                    FplPlayerPosition.Midfielder,
                    5
                },
                {
                    FplPlayerPosition.Forward,
                    3
                }
            };
        }

        public Dictionary<FplPlayerPosition, int> Limits { get; private set; }
    }
}
