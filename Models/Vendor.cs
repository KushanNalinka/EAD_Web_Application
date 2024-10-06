
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EADWebApplication.Models
{
    public class Vendor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string VendorName { get; set; }
        public string Email { get; set; }  // Unique email across both User and Vendor collections
        public string Password { get; set; }
        public string Role { get; set; } = "Vendor";  // Default role is Vendor
        public string Category { get; set; }

        // Array of comments, each with a unique ID and associated rank
        public List<CommentEntry> Comments { get; set; } = new List<CommentEntry>();

        public int Status { get; set; } = 1;  // 1 = Active, 0 = Deactivated
    }

    // Comment and Rank Model
    public class CommentEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  // Unique ID for each comment/rank entry
        public string UserId { get; set; }  // User ID of the commenter
        public string Comment { get; set; }  // Comment text
        public int Rank { get; set; }  // Rank value
    }
}

