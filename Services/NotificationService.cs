

using EADWebApplication.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EADWebApplication.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _notifications = database.GetCollection<Notification>("Notifications");
        }

        // Create a notification for a request-to-cancel
        public async Task CreateCancelRequestNotificationAsync(string orderId, string userId, string email, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Email = email,
                OrderId = orderId,
                Message = message
            };

            await _notifications.InsertOneAsync(notification);
        }

        // Get all cancellation requests (for CSR/Admin)
        public async Task<List<Notification>> GetAllCancelRequestsAsync()
        {
            return await _notifications.Find(n => n.Message.Contains("Cancel Request")).ToListAsync();
        }
    }
}

