using Projects.Config.Db;
using Projects.Dtos.Author;
using Projects.Dtos.Common;
using Projects.Models;
using Projects.Models.Common;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Projects.Services
{
    public class AuthorService
    {
        private readonly IMongoCollection<Author> _authorsCollection;

        private readonly IMongoCollection<BsonDocument> _authorsCollectionBson;

        public AuthorService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _authorsCollection = database.GetCollection<Author>(
                dbSettings.Value.AuthorsCollectionName
            );
            _authorsCollectionBson = database.GetCollection<BsonDocument>(DbCollections.Authors);
        }

        public async Task<List<Author>> GetAsync() =>
            await _authorsCollection.Find(_ => true).ToListAsync();

        public async Task<Author?> GetAsync(string id) =>
            await _authorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Author> CreateAsync(UpsertAuthorDto author)
        {
            var newAuthor = author.MapDtoToModel();

            await _authorsCollection.InsertOneAsync(newAuthor);
            return newAuthor;
        }

        public async Task UpdateAsync(string id, UpsertAuthorDto updatedAuthor)
        {
            var payload = updatedAuthor.MapDtoToModel(id);
            await _authorsCollection.ReplaceOneAsync(x => x.Id == id, payload);
        }

        public async Task RemoveAsync(string id) =>
            await _authorsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<PaginatedResultDto<AuthorWithBooks>> GetPaginatedAsync(
            AuthorQueryDto query
        )
        {
            // dynamic result2 = new
            // {
            //     Results = new List<Author>(),
            //     Total = 10,
            //     Page = query.Page,
            //     PageSize = query.PageSize,
            // };

            // return result2;

            var filter = Builders<BsonDocument>.Filter.Empty;

            // if (!string.IsNullOrWhiteSpace(query.Search))
            // {
            //     var regex = new BsonRegularExpression(query.Search, "i");

            //     filter = Builders<Author>.Filter.Or(
            //         Builders<Author>.Filter.Regex("name.first", regex),
            //         Builders<Author>.Filter.Regex("name.middle", regex),
            //         Builders<Author>.Filter.Regex("name.last", regex)
            //     );
            // }

            if (!string.IsNullOrWhiteSpace(query.BookTitle))
            {
                var bookTitleRegex = new BsonRegularExpression(query.BookTitle, "i");
                filter = Builders<BsonDocument>.Filter.And(
                    filter,
                    Builders<BsonDocument>.Filter.Or(
                        Builders<BsonDocument>.Filter.Eq("books.title", bookTitleRegex),
                        Builders<BsonDocument>.Filter.Eq("books.description", query.BookTitle),
                        Builders<BsonDocument>.Filter.Regex("books.Isbn", bookTitleRegex)
                    )
                );
            }

            var booksSearchRegEx = new BsonRegularExpression(query.BookTitle ?? "", "i");

            var pipeline = _authorsCollectionBson
                .Aggregate()
                .Lookup("Books", "_id", "authors._id", "books")
                // .Match(
                //     BsonDocument.Parse(
                //         @"
                //         {
                //             'books.title': {{booksSearchRegEx}},

                //         }
                //         "
                //     )
                // );
                .Match(
                    BsonDocument.Parse(
                        $$"""
                        {
                            "books.title": {{booksSearchRegEx}}
                        }
                        """
                    )
                );

            // pipeline = pipeline.AppendStage<BsonDocument>(
            //     BsonDocument.Parse(
            //         $$$"""
            //             {
            //                 $lookup: {
            //                     from: "Books",
            //                     localField: "_id",
            //                     foreignField: "authors._id",
            //                     as: "books"
            //                 }
            //             }
            //         """
            //     )
            // );

            // pipeline = pipeline.Project<AuthorWithBooks>(
            //     BsonDocument.Parse(
            //         $$"""
            //             {
            //                 "_id": 1,
            //                 "name": 1,
            //                 "books": 1
            //             }
            //         """
            //     )
            // );

            // pipeline = pipeline.Match(
            //     BsonDocument.Parse(
            //         $$"""
            //         {
            //             "books.title": RegExp("{{query.BookTitle ?? ""}}", "i")
            //         }
            //         """
            //     )
            // );

            // .Lookup("Books", "_id", "authors._id", "books");
            // .Match(
            //     BsonDocument.Parse(
            //         $$"""
            //         {
            //             "books.title": {{booksSearchRegEx}}
            //         }
            //         """
            //     )
            // );

            var bsonResults = await pipeline.As<AuthorWithBooks>().ToListAsync();

            var results = bsonResults;
            // .Select(bson => bson.ToBsonDocument())
            // .Select(bson => BsonSerializer.Deserialize<AuthorWithBooks>(bson))
            // .ToList();
            // var find = _authorsCollection.Find(filter);
            // var total = await find.CountDocumentsAsync();

            // var authors = query.FetchAll
            //     ? await find.ToListAsync()
            //     : await find.Skip((query.Page - 1) * query.PageSize)
            //         .Limit(query.PageSize)
            //         .ToListAsync();

            // dynamic dresult = new
            // {
            //     Results = results,
            //     Total = 10,
            //     Page = query.Page,
            //     PageSize = query.PageSize,
            // };

            // return dresult;

            var result = new PaginatedResultDto<AuthorWithBooks>
            {
                Results = results,
                Total = 10,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            // dynamic result = new
            // {
            //     Results = results,
            //     Total = 10,
            //     Page = query.Page,
            //     PageSize = query.PageSize,
            // };

            return result;
        }

        public async Task<List<IdName>> GetIdNamesAsync(List<string> ids)
        {
            var objectIds = ids.Select(id => new ObjectId(id)).ToList();

            // Filter using ObjectId list against MongoDB _id field
            var filter = Builders<Author>.Filter.In("_id", objectIds);

            // var pipeline = _authorsCollection
            //     .Aggregate()
            //     .Match(filter)
            //     .Project<IdName>(
            //         new BsonDocument
            //         {
            //             { "_id", "$_id" },
            //             {
            //                 "name",
            //                 new BsonDocument(
            //                     "$concat",
            //                     new BsonArray
            //                     {
            //                         "$name.first",
            //                         " ",
            //                         new BsonDocument(
            //                             "$ifNull",
            //                             new BsonArray { "$name.middle", "" }
            //                         ),
            //                         " ",
            //                         "$name.last",
            //                     }
            //                 )
            //             },
            //         }
            //     );

            var pipeline = _authorsCollection
                .Aggregate()
                .Match(filter)
                .Project<IdName>(
                    BsonDocument.Parse(
                        $$"""
                        {
                            "_id": "$_id",
                            "name": {
                                "$concat": [
                                    "$name.first",
                                    " ",
                                    { "$ifNull": [ "$name.middle", "" ] },
                                    " ",
                                    "$name.last"
                                ]
                            },
                        }
                        """
                    )
                );

            return await pipeline.ToListAsync();
        }
    }
}
