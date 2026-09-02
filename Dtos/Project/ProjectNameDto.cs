using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Dtos.Project
{
    public class ProjectNameDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? name { get; set; }
    }
}
