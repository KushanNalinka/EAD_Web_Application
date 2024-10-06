

using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Models;
using EADWebApplication.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EADWebApplication.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly JwtHelper _jwtHelper;

        public ProductController(ProductService productService, JwtHelper jwtHelper)
        {
            _productService = productService;
            _jwtHelper = jwtHelper;
        }

        // Get all products with vendor details (For all users)
        [Authorize]
        [HttpGet("listwithEmail")]
        public async Task<IActionResult> ListProductswithEmail()
        {
            var products = await _productService.GetProductsWithVendorEmailAsync();
            return Ok(products);
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> ListProducts()
        {
            var products = await _productService.GetProductsWithVendorDetailsAsync();
            return Ok(products);
        }


       

        // Vendor creates a product with image upload
        [Authorize(Roles = "Vendor")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductModel productModel, IFormFile imageFile)
        {
            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            // Validate image file
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { error = "Image file is required." });
            }

            // Save image to server
            var imageFileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine("wwwroot/images", imageFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Create new product
            var product = new Product
            {
                ProductName = productModel.ProductName,
                Price = productModel.Price,
                AvailableQuantity = productModel.AvailableQuantity,
                Category = productModel.Category,
                Description = productModel.Description,
                Image = imageFileName,  // Save image path
                VendorEmail = vendorEmail,
                StockStatus = 2,
                CategoryStatus = 1
            };

            await _productService.CreateProductAsync(product);
            return Ok(new { message = "Product created successfully." });
        }




        // Vendor updates a product with optional image upload
        [Authorize(Roles = "Vendor")]
        [HttpPut("update/{productId}")]
        public async Task<IActionResult> UpdateProduct(string productId, [FromForm] ProductModel productUpdate, IFormFile imageFile)
        {
            var existingProduct = await _productService.GetProductByIdAsync(productId);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;  // Vendor Email from token
            if (existingProduct.VendorEmail != vendorEmail)
            {
                return Unauthorized("You can only update your own products.");
            }

            // Update fields
            existingProduct.ProductName = productUpdate.ProductName;
            existingProduct.Price = productUpdate.Price;
            existingProduct.AvailableQuantity = productUpdate.AvailableQuantity;
            existingProduct.Category = productUpdate.Category;
            existingProduct.Description = productUpdate.Description;

            // Handle optional image update
            if (imageFile != null && imageFile.Length > 0)
            {
                // Save new image
                var imageFileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine("wwwroot/images", imageFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Update the product's image field
                existingProduct.Image = imageFileName;
            }

            await _productService.UpdateProductAsync(existingProduct);
            return Ok("Product updated successfully.");
        }


        // Vendor deletes a product
        [Authorize(Roles = "Vendor")]
        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            var existingProduct = await _productService.GetProductByIdAsync(productId);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            var vendorEmail = User.FindFirst(ClaimTypes.Email)?.Value;  // Vendor Email from token
            if (existingProduct.VendorEmail != vendorEmail)
            {
                return Unauthorized("You can only delete your own products.");
            }

            try
            {
                await _productService.DeleteProductAsync(productId);
                return Ok("Product deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

     

        [Authorize(Roles = "Admin")]
        [HttpPut("stock-status/{productId}")]
        public async Task<IActionResult> UpdateStockStatus(string productId, [FromBody] StockStatusModel model)
        {
            await _productService.UpdateStockStatusAsync(productId, model.StockStatus);
            return Ok("Stock status updated successfully.");
        }

        public class StockStatusModel
        {
            public int StockStatus { get; set; }
        }


        // Admin updates category status
        [Authorize(Roles = "Admin")]
        [HttpPut("category-status")]
        public async Task<IActionResult> UpdateCategoryStatus([FromBody] CategoryStatusModel categoryStatusModel)
        {
            await _productService.UpdateCategoryStatusAsync(categoryStatusModel.Category, categoryStatusModel.CategoryStatus);
            return Ok("Category status updated successfully.");
        }

        // Fetch specific product details
        [Authorize]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(string productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            return Ok(product);
        }

        // POST: api/product/filter
    [HttpPost("filter")]
    public async Task<IActionResult> FilterProducts([FromBody] ProductFilterModel filter)
        {
            var products = await _productService.FilterProductsAsync(filter);
            return Ok(products);
        }
    }
    public class CategoryStatusModel
    {
        public string Category { get; set; }
        public int CategoryStatus { get; set; }
    }
}


