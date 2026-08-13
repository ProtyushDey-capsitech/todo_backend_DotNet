using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Dtos.Todo
{
    public class ResponseTaskData: UpsertTaskDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
