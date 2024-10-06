using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EADWebApplication.Models
{
    public class ProductNotification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ProductName { get; set; }
        public int AvailableQuantity { get; set; }
        public string VendorEmail { get; set; }
        public string VendorId { get; set; }
        public bool Removed { get; set; } = false;
        public string Message { get; set; }
        public DateTime NotificationDate { get; set; } = DateTime.Now;
    }
}

