using FplClient.Data;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Models
{
    public class TeamPositionPlayerLimits
    {
        public TeamPositionPlayerLimits()
        {
            Limits = new Dictionary<FplPlayerPosition, TeamPositionLimits>
            {
                {
                    FplPlayerPosition.Goalkeeper,
                    new TeamPositionLimits(1, 1)
                },
                {
                    FplPlayerPosition.Defender,
                    new TeamPositionLimits(3, 5)
                },
                {
                    FplPlayerPosition.Midfielder,
                    new TeamPositionLimits(3, 5)
                },
                {
                    FplPlayerPosition.Forward,
                    new TeamPositionLimits(2, 3)
                }
            };
        }

        public Dictionary<FplPlayerPosition, TeamPositionLimits> Limits { get; private set; }

        public class TeamPositionLimits
        {
            public TeamPositionLimits(int min, int max) => (Minimum, Maximum) = (min, max);

            public int Minimum { get; private set; }
            public int Maximum { get; private set; }
        }
    }
}
