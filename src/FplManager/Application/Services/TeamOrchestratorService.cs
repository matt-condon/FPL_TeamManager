using FplClient;
using FplClient.Clients;
using FplClient.Data;
using FplManager.Application.Builders;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Extensions;
using FplManager.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FplManager.Application.Services
{
    public interface ITeamOrchestratorService
    {
        Task ManageTeam(
            int fplTeamId, double transferPercentile, int numberOfTransfers, bool requireTransferApproval,
            bool freeTransfersOnly, bool useWC, int sleepBetweenTransfersMs = 2000
        );
    }

    public class TeamOrchestratorService : ITeamOrchestratorService
    {
        private readonly ITeamBuilder<CurrentTeamPick> _currentTeamBuilder;
        private readonly ITransferWishlistBuilder _wishlistBuilder;
        private readonly IPlayerDictionaryBuilder<FplPlayer> _dictionaryBuilder;
        private readonly IFplPlayerClient _fplPlayerClient;
        private readonly ISetTeamBuilder _setTeamBuilder;
        private readonly ITransferSelectorService _transferSelectorService;
        private readonly TransferApprovalService _transferApprovalService;
        private readonly HttpClient _httpClient;

        public TeamOrchestratorService(
            ITeamBuilder<CurrentTeamPick> currentTeamBuilder, 
            IPlayerDictionaryBuilder<FplPlayer> dictionaryBuilder,
            ITransferWishlistBuilder transferWishlistBuilder,
            IFplPlayerClient fplPlayerClient,
            ISetTeamBuilder setTeamBuilder,
            ITransferSelectorService transferSelectorService,
            HttpClient httpClient
        )
        {
            _currentTeamBuilder = currentTeamBuilder;
            _wishlistBuilder = transferWishlistBuilder;
            _dictionaryBuilder = dictionaryBuilder;
            _fplPlayerClient = fplPlayerClient;
            _setTeamBuilder = setTeamBuilder;
            _transferSelectorService = transferSelectorService;
            _transferApprovalService = new TransferApprovalService();
            _httpClient = httpClient;
        }

        public async Task ManageTeam(int fplTeamId, double transferPercentile, int numberOfTransfers, bool requireTransferApproval, 
            bool freeTransfersOnly, bool useWC, int sleepBetweenTransfersMs = 2000)
        {
            var allPlayers = await GetPlayersAsync();

            for (int i = 0; i < numberOfTransfers; i++)
            {
                var currentTeam = await GetTeamAsync(fplTeamId);

                if (!PlayingWC(currentTeam.Chips, useWC, out bool shouldPlayWC) && freeTransfersOnly && TeamHasNoFreeTransfers(currentTeam))
                    break;

                var fullTeam = _currentTeamBuilder.BuildTeamByPicks(currentTeam.Picks, allPlayers, startingTeamOnly: false);

                var squadTransferList = _wishlistBuilder.BuildSquadTransferList(fullTeam);
                //squadTransferList.PrintSquadTransferList();

                var transferTargetsList = GetTransferTargetList(fullTeam, allPlayers, 100);
                //transferTargetsList.PrintTransferTargetList();

                var transferSelection = _transferSelectorService.SelectTransfer(fullTeam, transferTargetsList, squadTransferList, currentTeam.Transfers.Bank, transferPercentile);
                PrintMyTransferSelection(transferSelection);

                if (!requireTransferApproval || _transferApprovalService.IsTransferApproved())
                {
                    var gameweek = await GetComingGameweek();
                    await MakeTransferFromSelection(transferSelection, fplTeamId, gameweek, shouldPlayWC);
                }

                System.Threading.Thread.Sleep(sleepBetweenTransfersMs);
            }

            var updatedCurrentTeam = await GetTeamAsync(fplTeamId);
            var updatedFullTeam = _currentTeamBuilder.BuildTeamByPicks(updatedCurrentTeam.Picks, allPlayers, startingTeamOnly: false);
            await SelectCurrentTeam(updatedFullTeam, fplTeamId);
            //PrintCurrentTeam(updatedFullTeam);

        }

        private async Task<List<FplPlayer>> GetPlayersAsync()
        {
            var players = await _fplPlayerClient.GetAllPlayers();
            return players.ToList();
        }

        public async Task<MyTeamModel> GetTeamAsync(int fplTeamId)
        {
            try
            {
                var getTeamResponse = await _httpClient.GetAsync($"https://fantasy.premierleague.com/api/my-team/{fplTeamId}/");
                var teamAsString = getTeamResponse.Content.ReadAsStringAsync().Result;
                var teamAsPicks = JsonConvert.DeserializeObject<MyTeamModel>(teamAsString);
                return teamAsPicks;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to get team, error: {e}");
            }

            return null;
        }

        private bool TeamHasNoFreeTransfers(MyTeamModel currentTeam)
        {
            return currentTeam.Transfers.Limit <= currentTeam.Transfers.Made;
        }


        private bool PlayingWC(ICollection<CurrentTeamChips> chips, bool useWC, out bool activatingWC)
        {
            var wcChip = chips.First(c => c.Name == ChipNameConstants.WC);
            activatingWC = useWC && (wcChip.Status.Equals(ChipNameConstants.ChipAvailable) || wcChip.Status.Equals(ChipNameConstants.ChipActive));
            
            if (useWC && !activatingWC)
                Console.WriteLine($"Cannot play WC. Chip not available or active");
            
            return activatingWC;
        }

        private async Task SelectCurrentTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullTeam, int fplTeamId)
        {
            var setTeam = _setTeamBuilder.BuildTeamToBeSet(fullTeam);

            string json = JsonConvert.SerializeObject(setTeam);
            StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var getTeamResponse = await _httpClient.PostAsync(
                $"https://fantasy.premierleague.com/api/my-team/{fplTeamId}/",
                httpContent
            );

            if (getTeamResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Select Current Team Failed. StatusCode: {getTeamResponse.StatusCode}");
            }
        }

        private List<EvaluatedFplPlayer> GetTransferTargetList(
            Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullTeam,
            List<FplPlayer> allPlayers,
            int numberOfPlayers = 200
        )
        {
            var allPlayersDictionary = _dictionaryBuilder.BuildFilteredPlayerDictionary(allPlayers, filterAvailability: false);
            return _wishlistBuilder.BuildTransferTargetWishlist(allPlayersDictionary, fullTeam, numberOfPlayers);
        }

        private async Task<int> GetComingGameweek()
        {
            var client = new FplGameweekClient(_httpClient);
            var fixtures = await client.GetGameweeks();

            return fixtures.First(n => n.IsNext).Id;
        }

        private async Task MakeTransferFromSelection(TransferModel transferSelection, int fplTeamId, int gameweek, bool shouldPlayWC)
        {
            var transferPayload = new TransferPayload()
            {
                TeamId = fplTeamId,
                GameWeek = gameweek,
                Transfers = new TransferModel[] { transferSelection },
                Chip = shouldPlayWC ? ChipNameConstants.WC : null
            };

            string json = JsonConvert.SerializeObject(transferPayload);
            StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var getTeamResponse = await _httpClient.PostAsync(
                $"https://fantasy.premierleague.com/api/transfers/",
                httpContent
            );

            if (getTeamResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Make Transfers From Selection Failed. StatusCode: {getTeamResponse.StatusCode}");
            }
        }

        private static void PrintCurrentTeam(Dictionary<FplPlayerPosition, List<EvaluatedFplPlayer>> fullTeam)
        {
            Console.WriteLine(fullTeam.GetSquadString());
        }

        private void PrintMyTransferSelection(TransferModel transfer)
        {
            Console.WriteLine("In:");
            Console.WriteLine(transfer.PlayerIn.PlayerInfo.GetPartialPlayerString());

            Console.WriteLine("Out:");
            Console.WriteLine(transfer.PlayerOut.PlayerInfo.GetPartialPlayerString());
        }
    }
}
