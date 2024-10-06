using EADWebApplication.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EADWebApplication.Services
{
    public class ShortNotificationService
    {
        private readonly IMongoCollection<ShortNotification> _notifications;

        public ShortNotificationService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _notifications = database.GetCollection<ShortNotification>("ShortNotifications");
        }

        // Create a new notification
        /*public async Task CreateNotificationAsync(ShortNotification notification)
        {
            await _notifications.InsertOneAsync(notification);
        }*/

        public async Task CreateNotificationAsync(string userId, string email, string message, string orderId)
        {
            var newNotification = new ShortNotification
            {
                UserId = userId,
                Email = email,
                Message = message,
                OrderId = orderId,  // Ensure orderId is passed and stored
                CreatedAt = DateTime.UtcNow,
                Removed = false
            };

            await _notifications.InsertOneAsync(newNotification);
        }


        // Get all notifications (CSR/Admin only)
        public async Task<List<ShortNotification>> GetAllNotificationsAsync()
        {
            return await _notifications.Find(_ => true).ToListAsync();
        }

        // Get notifications by UserId
        // Get notifications by UserId where removed == false
        public async Task<List<ShortNotification>> GetNonRemovedNotificationsByUserIdAsync(string userId)
        {
            return await _notifications.Find(n => n.UserId == userId && n.Removed == false).ToListAsync();
        }

        

        // Get notification by Id
        public async Task<ShortNotification> GetNotificationByIdAsync(string notificationId)
        {
            return await _notifications.Find(n => n.Id == notificationId).FirstOrDefaultAsync();
        }

        // Update an existing notification (to mark as removed)
        public async Task UpdateNotificationAsync(ShortNotification notification)
        {
            await _notifications.ReplaceOneAsync(n => n.Id == notification.Id, notification);
        }
    }
}
