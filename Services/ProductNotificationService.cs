using MongoDB.Driver;
using EADWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EADWebApplication.Services
{
    public class ProductNotificationService
    {
        private readonly IMongoCollection<ProductNotification> _notifications;

        public ProductNotificationService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _notifications = database.GetCollection<ProductNotification>("ProductNotifications");
        }

        public async Task CreateProductNotificationAsync(ProductNotification notification)
        {
            await _notifications.InsertOneAsync(notification);
        }

        public async Task<List<ProductNotification>> GetAllNotificationsAsync()
        {
            return await _notifications.Find(notification => true).ToListAsync();
        }

        public async Task<ProductNotification> GetNotificationByIdAsync(string notificationId)
        {
            return await _notifications.Find(n => n.Id == notificationId).FirstOrDefaultAsync();
        }

        public async Task<List<ProductNotification>> GetNotificationsByVendorEmailAsync(string vendorEmail)
        {
            return await _notifications.Find(n => n.VendorEmail == vendorEmail && !n.Removed).ToListAsync();
        }

        public async Task UpdateNotificationAsync(ProductNotification notification)
        {
            await _notifications.ReplaceOneAsync(n => n.Id == notification.Id, notification);
        }

        public async Task DeleteNotificationAsync(string notificationId)
        {
            await _notifications.DeleteOneAsync(n => n.Id == notificationId);
        }

        public async Task MarkNotificationAsRemovedAsync(string notificationId, bool removed)
        {
            var notification = await GetNotificationByIdAsync(notificationId);
            if (notification != null)
            {
                notification.Removed = removed;
                await UpdateNotificationAsync(notification);
            }
        }
    }
}
