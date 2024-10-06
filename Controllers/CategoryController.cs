using Microsoft.AspNetCore.Mvc;
using EADWebApplication.Models;
using EADWebApplication.Services;
using System.Threading.Tasks;

namespace EADWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Create a new category
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryModel model)
        {
            var category = new Category
            {
                CategoryName = model.CategoryName
            };

            await _categoryService.CreateCategoryAsync(category);
            return Ok("Category created successfully.");
        }

        // Delete a category by ID
        [HttpDelete("delete/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(string categoryId)
        {
            await _categoryService.DeleteCategoryAsync(categoryId);
            return Ok("Category deleted successfully.");
        }

        // Get a category by ID
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category not found.");
            }
            return Ok(category);
        }

        // Get all categories
        [HttpGet("list")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
    }

    public class CategoryModel
    {
        public string CategoryName { get; set; }
    }
}
