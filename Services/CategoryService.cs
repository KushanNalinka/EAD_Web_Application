using MongoDB.Driver;
using EADWebApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class CategoryService
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryService(IOptions<MongoDBSettings> mongoSettings)
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
        _categories = database.GetCollection<Category>("Categories");
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _categories.Find(_ => true).ToListAsync();
    }

    public async Task<Category> GetCategoryByIdAsync(string categoryId)
    {
        return await _categories.Find(c => c.Id == categoryId).FirstOrDefaultAsync();
    }

    public async Task CreateCategoryAsync(Category category)
    {
        await _categories.InsertOneAsync(category);
    }

    public async Task DeleteCategoryAsync(string categoryId)
    {
        await _categories.DeleteOneAsync(c => c.Id == categoryId);
    }
}
