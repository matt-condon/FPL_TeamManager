using FplManager.Infrastructure.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FplManager.Infrastructure.FplClients
{
    public class CustomFplEntryClient
    {
        private readonly HttpClient _client;

        public CustomFplEntryClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<FplEntryExtension> Get(int teamId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var url = $"https://fantasy.premierleague.com/api/entry/{teamId}/";

            var json = await _client.GetStringAsync(url);

            return JsonConvert.DeserializeObject<FplEntryExtension>(json);
        }
    }
}
