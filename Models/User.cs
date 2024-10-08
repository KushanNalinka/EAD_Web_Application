using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EADWebApplication.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "User";  // Default role
        public int Status { get; set; } = 1;  // 1 = Active, 0 = Deactivated
    }
}
