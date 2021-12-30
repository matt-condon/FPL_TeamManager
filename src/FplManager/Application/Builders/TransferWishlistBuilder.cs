using FplClient.Data;
using FplManager.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Builders
{
    public interface ITransferWishlistBuilder
    {
        List<EvaluatedFplPlayer> BuildTransferTargetWishlist(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> allPlayers,
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad,
            int numberOfPlayers = 200);

        List<EvaluatedFplPlayer> BuildSquadTransferList(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad);
    }

    public class TransferWishlistBuilder: ITransferWishlistBuilder
    {
        public List<EvaluatedFplPlayer> BuildTransferTargetWishlist(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> allPlayers, 
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad, 
            int numberOfPlayers = 200)
        {

            var unfilteredWishlist = allPlayers.Values
                .SelectMany(p => p)
                .OrderByDescending(p => p.Evaluation);

            var existingSquadIds = existingSquad.Values
                .SelectMany(s => s);

            return unfilteredWishlist
                .Where(u => !existingSquadIds
                    .Any(s => s.PlayerInfo.Id == u.PlayerInfo.Id)
                )
                .Take(numberOfPlayers)
                .ToList();
        }
        
        public List<EvaluatedFplPlayer> BuildSquadTransferList(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad)
        {
            var orderedList = existingSquad.Values
                .SelectMany(p => p)
                .OrderBy(p => p.TransferListViability)
                .ToList();
            return orderedList;
        }
    }
}
