using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Dtos.Project
{
    public class ResponseProjectData:UpsertProjectDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int TaskCount { get; set; }
    }
}
