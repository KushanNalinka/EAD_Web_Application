using EADWebApplication.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EADWebApplication.Services
{
    public class CancelNotificationService
    {
        private readonly IMongoCollection<CancelNotification> _cancelNotifications;

        public CancelNotificationService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _cancelNotifications = database.GetCollection<CancelNotification>("CancelNotifications");
        }

        // Notify the customer about order updates (e.g., order delivered, canceled)
        public async Task NotifyCustomerAsync(string userId, string email, string orderId, string message)
        {
            var notification = new CancelNotification
            {
                UserId = userId,
                Email = email,
                OrderId = orderId,
                Message = message
            };

            await _cancelNotifications.InsertOneAsync(notification);
        }

        // Get all notifications for a specific user
        public async Task<List<CancelNotification>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _cancelNotifications.Find(n => n.UserId == userId).ToListAsync();
        }
    }
}
