using FplManager.Infrastructure.Models;
using System;
using System.Collections.Generic;

namespace FplManager.Infrastructure.Extensions
{
    public static class PlayerListExtensions
    {
        public static void PrintSquadTransferList(this List<EvaluatedFplPlayer> wishList)
        {
            Console.WriteLine("Squad Transfer List:");
            Console.WriteLine("------------------------------------------");

            Console.WriteLine(wishList.GetWishlistString());
        }

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


        public static void PrintTransferTargetList(this List<EvaluatedFplPlayer> transferTargets)
        {
            Console.WriteLine("Transfer Tagets Wishlist:");
            Console.WriteLine("------------------------------------------");

            Console.WriteLine(transferTargets.GetWishlistString());
        }
    }
}
