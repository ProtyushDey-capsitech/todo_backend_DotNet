using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models
{
    public static class TodoStatus
    {
        public const string High = "High";
        public const string Medium = "Medium";
        public const string Low = "Low";
    }
    public class Todo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Desc { get; set; }
        public bool IsDone { get; set; } = false;
        public string Status { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
