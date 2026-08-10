using Projects.Models.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public List<IdName> Authors { get; set; } = [];

    public List<IdName> Genres { get; set; } = [];

    public string? ISBN { get; set; }

    public DateTime? PublishedOn { get; set; }

    public int? Pages { get; set; }

    public decimal? Price { get; set; }

    public bool InStock { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}
