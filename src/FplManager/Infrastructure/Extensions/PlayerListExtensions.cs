using FplManager.Infrastructure.Models;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Extensions
{
    public static class PlayerListExtensions
    {
        public static string GetWishlistString(this List<EvaluatedFplPlayer> wishList)
        {
            var wishListString = string.Empty;
            foreach (var player in wishList)
            {
                wishListString = wishListString.ConcatWithNewLine($"Position: {player.PlayerInfo.Position}");
                wishListString = wishListString.ConcatWithNewLine($"Team: {player.PlayerInfo.TeamCode}");
                wishListString = wishListString.ConcatWithNewLine($"{player.PlayerInfo.GetPartialPlayerString()}");
                wishListString = wishListString.ConcatWithNewLine($"Player Evaluation: {player.Evaluation}");
                wishListString = wishListString.ConcatWithNewLine("");
            }
            return wishListString;
        }
    }
}
