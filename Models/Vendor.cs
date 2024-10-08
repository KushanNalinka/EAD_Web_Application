
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
        public string Email { get; set; }  
        public string Password { get; set; }
        public string Role { get; set; } = "Vendor";  // Default role is Vendor
        public string Category { get; set; }

        
        public List<CommentEntry> Comments { get; set; } = new List<CommentEntry>();

        public int Status { get; set; } = 1;  // 1 = Active, 0 = Deactivated
    }

    // Comment and Rank Model
    public class CommentEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  
        public string UserId { get; set; }  
        public string Comment { get; set; }  // Comment text
        public int Rank { get; set; }  // Rank value
    }
}

