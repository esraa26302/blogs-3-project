using Microsoft.Extensions.Configuration;
using OneSignal.CSharp.SDK;
using OneSignal.CSharp.SDK.Resources.Notifications;
using RestSharp.Authenticators;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using blogsproject_1.Models;
using Microsoft.EntityFrameworkCore;


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
        private readonly ApplicationDbContext _context;

        public OneSignalNotificationService(IConfiguration configuration, ILogger<OneSignalNotificationService> logger, ApplicationDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _restClient = new RestClient("https://onesignal.com/api/v1");
        }

        public async Task SendNotificationAsync(string oneId, string title, string message)
        {
            var appId = _configuration["OneSignal:AppId"];
            var request = new RestRequest("notifications", Method.Post);
            request.AddHeader("Authorization", $"Basic {_configuration["OneSignal:ApiKey"]}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            var notification = new
            {
                app_id = appId,
                include_player_ids = new List<string> { oneId },
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
                    _logger.LogError($"Failed to send notification: {response.StatusCode} - {response.Content}");
                    throw new ApplicationException($"Failed to send notification: {response.StatusCode} - {response.Content}");
                }

                _logger.LogInformation($"Notification sent successfully: {response.Content}");

               
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.OneId == oneId);
                if (adminUser != null)
                {
                    var dbNotification = new Nofication
                    {
                        Title = title,
                        Message = message,
                        SentDate = DateTime.UtcNow,
                        AdminUserId = adminUser.Id
                    };

                    _context.Nofications.Add(dbNotification);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
                throw new ApplicationException("Failed to send notification", ex);
            }
        }
    }
}