﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EADWebApplication.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  

        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }  

        public int StockStatus { get; set; } = 2;  // 0 = Out of Stock, 1 = Low Stock, 2 = In Stock
        public int CategoryStatus { get; set; } = 1;  // 0 = Inactive, 1 = Active

      
        public string VendorEmail { get; set; }  // Store the Vendor's Email instead of VendorId

        public DateTime? LowStockStatusNotificationDateAndTime { get; set; } = DateTime.Now;
    }
}
