using FplClient.Data;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FplManager.Application.Services
{
    public class TransferSelectorService
    {
        private readonly SquadRuleService _squadRuleService;
        private readonly Random _getRandom;
        public TransferSelectorService()
        {
            _squadRuleService = new SquadRuleService();
            _getRandom = new Random();
        }

        public TransferModel SelectTransfer(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad,
            List<EvaluatedFplPlayer> transferTargetsWishList,
            List<EvaluatedFplPlayer> squadTransferList,
            int inBank,
            double transferPercentile
        )
        {
            var possibleTransfers = transferTargetsWishList
                .SelectMany(w => squadTransferList
                .Where(s => IsValidTransfer(s, w, existingSquad, inBank))
                .Select(s => new TransferModel(s, w, w.Evaluation - s.Evaluation, s.SellingPrice, w.PlayerInfo.NowCost)))
                .OrderByDescending(t => t.EvalDifference);

            /* for debugging purposes */
            //GetNamedTransfers(possibleTransfers);
            
            var transferSelection = GetTransferSelection(possibleTransfers, transferPercentile);

            return possibleTransfers.ToArray()[transferSelection];
        }

        private bool IsValidTransfer(
            EvaluatedFplPlayer playerOut,
            EvaluatedFplPlayer playerIn,
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad,
            int inBank)
        {
            return PositionsAreMatching(playerOut, playerIn)
                && AreSquadRulesValid(playerOut, playerIn, existingSquad, inBank)
                && playerIn.PlayerInfo.Status == PlayerInfoConstants.AvailableStatus;
        }

        private bool PositionsAreMatching(EvaluatedFplPlayer playerOut, EvaluatedFplPlayer playerIn)
        {
            return playerOut.PlayerInfo.Position == playerIn.PlayerInfo.Position;
        }

        private bool AreSquadRulesValid(
            EvaluatedFplPlayer playerOut,
            EvaluatedFplPlayer playerIn,
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> existingSquad,
            int inBank)
        {
            var currentSquadCost = existingSquad.GetSquadCost();
            var position = playerOut.PlayerInfo.Position;
            var squadPositionValues = existingSquad.Where(p => p.Key == position)
                .SelectMany(p => p.Value)
                .Where(p => p.PlayerInfo.Id != playerOut.PlayerInfo.Id).ToList();
            squadPositionValues.Add(playerIn);

            var newSquad = existingSquad.Where(p => p.Key != position)
                .ToDictionary(p => p.Key, p => p.Value);
            newSquad.Add(position, squadPositionValues);
            var transferIsValid = _squadRuleService.IsValidSquad(newSquad, currentSquadCost, inBank);
            return transferIsValid;
        }

        private int GetTransferSelection(IOrderedEnumerable<TransferModel> possibleTransfers, double transferPercentile)
        {
            var possibleTransferCount = possibleTransfers.Count();
            var transfersToConsider = (int)(possibleTransferCount * transferPercentile);
            var transferSelection = _getRandom.Next(0, transfersToConsider);
            return transferSelection;
        }

        private dynamic GetNamedTransfers(IOrderedEnumerable<TransferModel> possibleTransfers){
            var playerOutNames = possibleTransfers.Select(w => new 
            {
                In = w.PlayerIn.PlayerInfo.SecondName,
                Out = w.PlayerOut.PlayerInfo.SecondName
            });
            return playerOutNames;
        }
    }
}
