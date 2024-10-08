namespace EADWebApplication.Models
{
    public class ProductFilterModel
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
