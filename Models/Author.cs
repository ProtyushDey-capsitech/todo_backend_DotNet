using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models
{
    public class Author
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public PersonName Name { get; set; } = new PersonName();
    }

    public class AuthorWithBooks : Author
    {
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
