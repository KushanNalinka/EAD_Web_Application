namespace EADWebApplication.Models
{
    public class ProductWithVendorDetails
    {
        public string Id { get; set; }  // Product ID
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int StockStatus { get; set; }
        public int CategoryStatus { get; set; }
        public string VendorEmail { get; set; }  // Stored in the product document
        public string VendorId { get; set; }  // Retrieved from Vendor service
    }
}
