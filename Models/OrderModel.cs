namespace EADWebApplication.Models
{
    public class OrderModel
    {
        public int PaymentMethod { get; set; }  // 1 = Cash on Delivery, 2 = Card on Delivery

        public string Note { get; set; }  // Optional note for the order

        public string DeliveryAddress { get; set; }  // Where the order will be delivered

        public List<OrderItemModel> OrderItems { get; set; } = new List<OrderItemModel>();  // List of ordered items
    }

    public class OrderItemModel
    {
        public string ProductId { get; set; }  // ID of the product

        public string ProductName { get; set; }  // Name of the product

        public int Quantity { get; set; }  // Quantity of this product ordered

        public decimal UnitPrice { get; set; }  // Unit price of the product

        public string VendorId { get; set; }  // The vendor selling this product

        public string VendorEmail { get; set; }  // The vendor's email
    }
}

