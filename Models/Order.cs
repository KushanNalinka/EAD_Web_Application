using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EADWebApplication.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }  // The user who placed the order

        public string Email { get; set; }  // Storing email from token

        public int OrderStatus { get; set; } = 0;  // 0 = Pending, 1 = Dispatched, 2 = Completed

        public bool Delivered { get; set; } = false;  // Default false
        public string Note { get; set; }  // Optional note for the order

        public decimal OrderTotal { get; set; }  // Total amount for the order

        public string DeliveryAddress { get; set; }  // Where the order will be delivered

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;  // Automatically filled

        public DateTime OrderTime { get; set; } = DateTime.UtcNow;  // Automatically filled

        public int PaymentMethod { get; set; }  // 1 = Cash on Delivery, 2 = Card on Delivery

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();  // List of ordered items
    }

    public class OrderItem
    {
        public string ProductId { get; set; }  // ID of the product

        public string ProductName { get; set; }  // Name of the product

        public int Quantity { get; set; }  // Quantity of this product ordered

        public decimal UnitPrice { get; set; }  // Unit price of the product

        public decimal TotalPrice { get; set; }  // Total price = Quantity * Unit Price

        public string VendorId { get; set; }  // The vendor selling this product

        public string VendorEmail { get; set; }  // The vendor's email

        public int OrderItemStatus { get; set; } = 0;  // 0 = Pending, 1 = Dispatched, 2 = Completed
    }
}

