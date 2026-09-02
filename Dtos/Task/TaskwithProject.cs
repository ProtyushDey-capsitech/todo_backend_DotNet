using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class TaskwithProject
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? projectId { get; set; }

    public string? Name { get; set; }

    public string? Desc { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? projectName { get; set; }
}