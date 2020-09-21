using FplClient.Data;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public class TransferWishlistBuilder
    {
        //TODO: Build wishlist based on form, transfers in/out and fixtures
        public List<EvaluatedFplPlayer> BuildTransferWishlist(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> allPlayers, 
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad, 
            int numberOfPlayers = 20)
        {

            var unfilteredWishlist = allPlayers.Values
                .SelectMany(p => p)
                .OrderByDescending(p => p.Evaluation)
                .Take(numberOfPlayers);

            var existingSquadIds = existingSquad.Values
                .SelectMany(s => s);

            return unfilteredWishlist
                .Where(u => !existingSquadIds
                    .Any(s => s.PlayerInfo.Id == u.PlayerInfo.Id)
                ).ToList();
        }
    }
}
