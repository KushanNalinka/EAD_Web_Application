using MongoDB.Driver;
using EADWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace EADWebApplication.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;
        private readonly VendorService _vendorService;
        private readonly ProductNotificationService _notificationService;
        private readonly IMongoCollection<Order> _orders; 

        public ProductService(IOptions<MongoDBSettings> mongoSettings, VendorService vendorService, ProductNotificationService notificationService)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>("Products");
            _vendorService = vendorService;
            _notificationService = notificationService;
            _orders = database.GetCollection<Order>("Orders"); // Initialize orders collection
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _products.Find(product => true).ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(string productId)
        {
            return await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
        }

        public async Task CreateProductAsync(Product product)
        {
            await _products.InsertOneAsync(product);

            // Check if the available quantity is below threshold after creation
            await CheckAndSendLowStockNotification(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _products.ReplaceOneAsync(p => p.Id == product.Id, product);

            // Check if the available quantity is below threshold after creation
            await CheckAndSendLowStockNotification(product);
        }

      
        public async Task DeleteProductAsync(string productId)
        {
            // Check if the product exists in any active orders
            var filter = Builders<Order>.Filter.ElemMatch(o => o.OrderItems, oi => oi.ProductId == productId && oi.OrderItemStatus != 2);
            var activeOrderWithProduct = await _orders.Find(filter).FirstOrDefaultAsync();

            if (activeOrderWithProduct != null)
            {
                throw new Exception("Cannot delete this product. It is part of an order that is not completed.");
            }

            // If no active orders, proceed to delete the product
            await _products.DeleteOneAsync(p => p.Id == productId);
        }



        public async Task UpdateStockStatusAsync(string productId, int stockStatus)
        {
            var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
            if (product != null)
            {
                product.StockStatus = stockStatus;
                product.LowStockStatusNotificationDateAndTime = DateTime.Now;

                if (stockStatus == 1)
                {
                    var notification = new ProductNotification
                    {
                        ProductName = product.ProductName,
                        AvailableQuantity = product.AvailableQuantity,
                        VendorEmail = product.VendorEmail,
                        Message = $"Stock Status is very Low for {product.ProductName}. Please update it soon."
                    };
                    await _notificationService.CreateProductNotificationAsync(notification);
                }

                await _products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }
        }

        // Change category status by admin
        public async Task UpdateCategoryStatusAsync(string category, int categoryStatus)
        {
            var filter = Builders<Product>.Filter.Eq("Category", category);
            var update = Builders<Product>.Update.Set("CategoryStatus", categoryStatus);
            await _products.UpdateManyAsync(filter, update);
        }

        
        // Fetch products including Vendor details
        public async Task<List<Product>> GetProductsWithVendorEmailAsync()
        {
            var products = await GetProductsAsync();
            foreach (var product in products)
            {
                // Fetch vendor details using the VendorEmail
                var vendor = await _vendorService.GetVendorByEmailAsync(product.VendorEmail);
                if (vendor != null)
                {
                    
                    product.VendorEmail = vendor.Email;
                   
                }
            }
            return products;
        }
        public async Task<List<ProductWithVendorDetails>> GetProductsWithVendorDetailsAsync()
        {
            var products = await GetProductsAsync();
            var productWithVendorDetailsList = new List<ProductWithVendorDetails>();

            foreach (var product in products)
            {
                // Fetch vendor details using the VendorEmail
                var vendor = await _vendorService.GetVendorByEmailAsync(product.VendorEmail);
                if (vendor != null)
                {
                    // Create a new product with both vendorEmail and vendorId
                    var productWithVendorDetails = new ProductWithVendorDetails
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        AvailableQuantity = product.AvailableQuantity,
                        Category = product.Category,
                        Description = product.Description,
                        Image = product.Image,
                        StockStatus = product.StockStatus,
                        CategoryStatus = product.CategoryStatus,
                        VendorEmail = product.VendorEmail,  
                        VendorId = vendor.Id  
                    };

                    productWithVendorDetailsList.Add(productWithVendorDetails);
                }
            }

            return productWithVendorDetailsList;
        }

        public async Task<List<Product>> FilterProductsAsync(ProductFilterModel filter)
        {
            var builder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>();

            // Apply filters based on the provided fields
            if (!string.IsNullOrEmpty(filter.ProductName))
            {
                filters.Add(builder.Regex("ProductName", new BsonRegularExpression(filter.ProductName, "i")));  // Case-insensitive match
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                filters.Add(builder.Eq(p => p.Category, filter.Category));
            }

            if (filter.MinPrice.HasValue)
            {
                filters.Add(builder.Gte(p => p.Price, filter.MinPrice.Value));
            }

            if (filter.MaxPrice.HasValue)
            {
                filters.Add(builder.Lte(p => p.Price, filter.MaxPrice.Value));
            }

            // Combine all filters, or use an empty filter if none provided
            var combinedFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _products.Find(combinedFilter).ToListAsync();
        }

        // Helper method to check and send low stock notification
        private async Task CheckAndSendLowStockNotification(Product product)
        {
            if (product.AvailableQuantity < 10)
            {
                var notification = new ProductNotification
                {
                    ProductName = product.ProductName,
                    AvailableQuantity = product.AvailableQuantity,
                    VendorEmail = product.VendorEmail,
                    Message = $"Stock for {product.ProductName} is low (less than 10 items). Please restock soon."
                };
                await _notificationService.CreateProductNotificationAsync(notification);
            }
        }
    }
}

