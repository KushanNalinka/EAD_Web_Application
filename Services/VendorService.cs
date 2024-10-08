using MongoDB.Driver;
using EADWebApplication.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace EADWebApplication.Services
{
    public class VendorService
    {
        private readonly IMongoCollection<Vendor> _vendors;
        private readonly UserService _userService; 

        public VendorService(IOptions<MongoDBSettings> mongoSettings, UserService userService)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _vendors = database.GetCollection<Vendor>("Vendors");
            _userService = userService;
        }

        public async Task<Vendor> GetVendorByEmailAsync(string email)
        {
            return await _vendors.Find(v => v.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateVendorAsync(Vendor vendor)
        {
            await _vendors.InsertOneAsync(vendor);
            // Create an entry in the User collection for login purposes
            var newUser = new User
            {
                Email = vendor.Email,
                Password = vendor.Password,  
                Role = "Vendor",
                Status = 1
            };
            await _userService.CreateUserAsync(newUser);
        }

        public async Task UpdateVendorAsync(Vendor vendor)
        {
            await _vendors.ReplaceOneAsync(v => v.Id == vendor.Id, vendor);
            // Sync the changes to the User collection (for email/password updates)
            var user = await _userService.GetUserByEmailAsync(vendor.Email);
            if (user != null)
            {
                user.Email = vendor.Email;
                user.Password = vendor.Password;
                await _userService.UpdateUserAsync(user);
            }
        }

        public async Task DeleteVendorAsync(string email)
        {
            await _vendors.DeleteOneAsync(v => v.Email == email);
            // Delete from User collection as well
            var user = await _userService.GetUserByEmailAsync(email);
            if (user != null)
            {
                await _userService.DeleteUserAsync(user.Id);
            }
        }

        

        public async Task<List<Vendor>> GetVendorsAsync()
        {
            return await _vendors.Find(_ => true).ToListAsync();  // List all vendors
        }

        public async Task AddCommentAsync(string vendorId, string comment, int rank, string userId)
        {
            var vendor = await _vendors.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
            if (vendor != null)
            {
                var newComment = new CommentEntry
                {
                    Id = ObjectId.GenerateNewId().ToString(),  
                    UserId = userId,
                    Comment = comment,
                    Rank = rank
                };

                vendor.Comments.Add(newComment);
                await _vendors.ReplaceOneAsync(v => v.Id == vendor.Id, vendor);
            }
        }

        public async Task UpdateCommentAsync(string vendorId, string commentId, string newComment, int newRank, string userId)
        {
            var vendor = await _vendors.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
            if (vendor != null)
            {
                var comment = vendor.Comments.FirstOrDefault(c => c.Id == commentId && c.UserId == userId);
                if (comment != null)
                {
                    comment.Comment = newComment;
                    comment.Rank = newRank;
                    await _vendors.ReplaceOneAsync(v => v.Id == vendor.Id, vendor);
                }
            }
        }

        public async Task DeleteCommentAsync(string vendorId, string commentId, string userId)
        {
            var vendor = await _vendors.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
            if (vendor != null)
            {
                vendor.Comments.RemoveAll(c => c.Id == commentId && c.UserId == userId);
                await _vendors.ReplaceOneAsync(v => v.Id == vendor.Id, vendor);
            }
        }

        public async Task<List<Vendor>> GetVendorsWithUserCommentsAsync(string userId)
        {
            // Find vendors where the user has left a comment
            return await _vendors.Find(v => v.Comments.Any(c => c.UserId == userId)).ToListAsync();
        }


        // Get Vender New
        public async Task<Vendor> GetVendorByIdAsync(string vendorId)
        {
            return await _vendors.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
        }

        public async Task ActivateVendorAsync(string email)
        {
            // Find the vendor by email
            var vendor = await _vendors.Find(v => v.Email == email).FirstOrDefaultAsync();
            if (vendor == null)
            {
                throw new Exception("Vendor not found.");
            }

            // Activate the vendor
            vendor.Status = 1;
            await _vendors.ReplaceOneAsync(v => v.Id == vendor.Id, vendor);

            // Also activate the corresponding user in the User collection
            var user = await _userService.GetUserByEmailAsync(email);
            if (user != null)
            {
                user.Status = 1;  // Activate user
                await _userService.UpdateUserAsync(user);
            }
        }

        public async Task DeactivateVendorAsync(string email)
        {
            // Find the vendor by email
            var vendor = await _vendors.Find(v => v.Email == email).FirstOrDefaultAsync();
            if (vendor == null)
            {
                throw new Exception("Vendor not found.");
            }

            // Deactivate the vendor
            vendor.Status = 0;
            await _vendors.ReplaceOneAsync(v => v.Id == vendor.Id, vendor);

            // Also deactivate the corresponding user in the User collection
            var user = await _userService.GetUserByEmailAsync(email);
            if (user != null)
            {
                user.Status = 0;  // Deactivate user
                await _userService.UpdateUserAsync(user);
            }
        }



    }
}
