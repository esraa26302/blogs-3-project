using Microsoft.Extensions.Configuration;
using OneSignal.CSharp.SDK;
using OneSignal.CSharp.SDK.Resources.Notifications;
using RestSharp.Authenticators;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;


namespace blogsproject_1.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message);
    }

    public class OneSignalNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly IRestClient _restClient;
        private readonly ILogger<OneSignalNotificationService> _logger;

        public OneSignalNotificationService(IConfiguration configuration, ILogger<OneSignalNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _restClient = new RestClient("https://onesignal.com/apps/adf99ce1-b541-4afb-a667-1246aa0df969/notifications");

            var appId = _configuration["OneSignal:AppId"];
            var apiKey = _configuration["OneSignal:ApiKey"];

            _logger.LogInformation($"OneSignal AppId: {appId}");
            _logger.LogInformation($"OneSignal ApiKey: {apiKey}");
        }

        public async Task SendNotificationAsync(string OneId, string title, string message)
        {
            var appId = _configuration["OneSignal:AppId"];
            var request = new RestRequest("notifications", Method.Post);
            request.AddHeader("Authorization", $"Basic {_configuration["OneSignal:ApiKey"]}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json"); 

            var notification = new
            {
                app_id = appId,
                include_player_ids = new List<string> { OneId },
                headings = new Dictionary<string, string> { { "en", title } },
                contents = new Dictionary<string, string> { { "en", message } }
            };

            request.AddJsonBody(notification);

            _logger.LogInformation($"Sending notification with AppId: {appId}");
            _logger.LogInformation($"Notification details: {System.Text.Json.JsonSerializer.Serialize(notification)}");

            RestResponse response = null;
            try
            {
                response = await _restClient.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    throw new ApplicationException($"Failed to send notification: {response.StatusCode} - {response.Content}");
                }

                _logger.LogInformation($"Notification sent successfully: {response.Content}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
                throw new ApplicationException("Failed to send notification", ex);
            }
        }
    }
}