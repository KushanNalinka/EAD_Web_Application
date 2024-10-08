using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EADWebApplication.Models
{
    public class ShortNotification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string OrderId { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Automatically store the time

        public bool Removed { get; set; } = false;  // New attribute, default value is false
    }
}
