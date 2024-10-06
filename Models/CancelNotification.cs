using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EADWebApplication.Models
{
    public class CancelNotification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }  // User related to the notification
        public string Email { get; set; }  // Email of the user
        public string OrderId { get; set; }  // Order related to the notification
        public string Message { get; set; }  // Notification message
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
