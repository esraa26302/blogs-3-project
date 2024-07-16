using Newtonsoft.Json;
using System.Text;

namespace blogsproject_1.Services
{
    public class OneSignalService
    {
        private readonly HttpClient _httpClient;
        private readonly string _appId;
        private readonly string _apiKey;

        public OneSignalService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _appId = configuration["OneSignal:AppId"];
            _apiKey = configuration["OneSignal:ApiKey"];
        }

        public async Task SetUserTagAsync(string playerId, string userId)
        {
            var url = "https://onesignal.com/api/v1/players/" + playerId;
            var requestContent = new
            {
                tags = new
                {
                    userId = userId
                }
            };
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Headers =
            {
                { "Authorization", "Basic " + _apiKey }
            },
                Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
        }
    }
}

