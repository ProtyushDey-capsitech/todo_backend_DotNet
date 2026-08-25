using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models
{
    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? Ip { get; set; }
        public DateTime ValidTime { get; set; } = DateTime.UtcNow;
        public bool IsExpired { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
