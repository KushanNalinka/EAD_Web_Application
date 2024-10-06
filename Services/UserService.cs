using MongoDB.Driver;
using EADWebApplication.Models;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Options;

namespace EADWebApplication.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return await _users.Find(u => u.Email == email).AnyAsync() == false;
        }
        // Deletion Of User
        public async Task DeleteUserAsync(string userId)
        {
            await _users.DeleteOneAsync(u => u.Id == userId);
        }

        public static string EncryptPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            // Fetch all users from the Users collection
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            // Fetch a single user by their MongoDB ObjectId
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }


    }
}

