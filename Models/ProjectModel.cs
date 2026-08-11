using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models
{
    public class ProjectModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public string? UserId { get; set; }
        public bool Status {  get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
