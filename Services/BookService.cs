using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projects.Config.Db;
using Projects.Dtos.Book;
using Projects.Dtos.Common;
using Projects.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Projects.Services
{
    public class BookService
    {
        private readonly IMongoCollection<Book> _booksCollection;
        private readonly AuthorService _authorService;
        private readonly GenreService _genreService;

        public BookService(
            IOptions<DbSettings> dbSettings,
            AuthorService authorService,
            GenreService genreService
        )
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<Book>(DbCollections.Books);

            _authorService = authorService;
            _genreService = genreService;
        }

        public async Task<List<Book>> GetAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<Book?> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Book> CreateAsync(UpsertBookDto dto)
        {
            var authors = await _authorService.GetIdNamesAsync(dto.AuthorIds);
            var genres = await _genreService.GetIdNamesAsync(dto.GenreIds);

            var book = new Book
            {
                Title = dto.Title,
                Description = dto.Description,
                ISBN = dto.ISBN,
                PublishedOn = dto.PublishedOn,
                Pages = dto.Pages,
                Price = dto.Price,
                InStock = dto.InStock,
                Authors = authors,
                Genres = genres,
                CreatedAt = DateTime.UtcNow,
            };

            await _booksCollection.InsertOneAsync(book);
            return book;
        }

        public async Task UpdateAsync(string id, Book updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<PaginatedResultDto<Book>> GetPaginatedAsync(BookQueryDto query)
        {
            var filter = Builders<Book>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var regex = new BsonRegularExpression(query.Search, "i");
                filter = Builders<Book>.Filter.Or(
                    Builders<Book>.Filter.Regex("title", regex),
                    Builders<Book>.Filter.Regex("description", regex),
                    Builders<Book>.Filter.In(
                        "authors._id",
                        query.AuthorIds.Select(id => new ObjectId(id))
                    ),
                    Builders<Book>.Filter.In(
                        "genres._id",
                        query.GenreIds.Select(id => new ObjectId(id))
                    )
                );
            }

            var find = _booksCollection.Find(filter);
            var total = await find.CountDocumentsAsync();

            var books = query.FetchAll
                ? await find.ToListAsync()
                : await find.Skip((query.Page - 1) * query.PageSize)
                    .Limit(query.PageSize)
                    .ToListAsync();

            var result = new PaginatedResultDto<Book>
            {
                Results = books,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            return result;
        }
    }
}
