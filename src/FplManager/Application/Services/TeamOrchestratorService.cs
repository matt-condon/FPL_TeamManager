using FplClient.Clients;
using FplClient.Data;
using FplManager.Application.Builders;
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
    public class TeamOrchestratorService
    {
        private readonly CurrentTeamBuilder _currentTeamBuilder;
        private readonly SetTeamBuilder _setTeamBuilder;
        private readonly TransferWishlistBuilder _wishlistBuilder;
        private readonly FplPlayerDictionaryBuilder _dictionaryBuilder;
        private readonly TransferSelectorService _transferSelectorService;
        private readonly TransferApprovalService _transferApprovalService;
        private readonly HttpClient _httpClient;
        private readonly FplPlayerClient _fplPlayerClient;

        public TeamOrchestratorService(HttpClient httpClient)
        {
            _currentTeamBuilder = new CurrentTeamBuilder();
            _setTeamBuilder = new SetTeamBuilder();
            _wishlistBuilder = new TransferWishlistBuilder();
            _dictionaryBuilder = new FplPlayerDictionaryBuilder();
            _transferSelectorService = new TransferSelectorService();
            _transferApprovalService = new TransferApprovalService();
            _httpClient = httpClient;
            _fplPlayerClient = new FplPlayerClient(_httpClient);
        }

        public async Task ManageTeam(int fplTeamId, double transferPercentile = 0.1, int numberOfTransfers = 0, bool requireTransferApproval = false, bool freeTransfersOnly = true, int sleepBetweenTransfersMs = 2000)
        {
            var allPlayers = await GetPlayersAsync();

            for (int i = 0; i < numberOfTransfers; i++)
            {
                var currentTeam = await GetTeamAsync(fplTeamId);

                if (freeTransfersOnly && TeamHasNoFreeTransfers(currentTeam))
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
                    await MakeTransferFromSelection(transferSelection, fplTeamId, gameweek);
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

        //TBD: Confirm this logic for GW3
        private bool TeamHasNoFreeTransfers(MyTeamModel currentTeam)
        {
            return currentTeam.Transfers.Limit == currentTeam.Transfers.Made;
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

        private async Task MakeTransferFromSelection(TransferModel transferSelection, int fplTeamId, int gameweek)
        {
            var transferPayload = new TransferPayload()
            {
                TeamId = fplTeamId,
                GameWeek = gameweek,
                Transfers = new TransferModel[] { transferSelection }
            };

            string json = JsonConvert.SerializeObject(transferPayload);
            StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var getTeamResponse = await _httpClient.PostAsync(
                $"https://fantasy.premierleague.com/api/transfers/",
                httpContent
            );

            if (getTeamResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Select Current Team Failed. StatusCode: {getTeamResponse.StatusCode}");
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
