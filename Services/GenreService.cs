using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Genre;
using Projects.Models;
using Projects.Models.Common;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Projects.Services
{
    public class GenreService
    {
        private readonly IMongoCollection<Genre> _genresCollection;

        public GenreService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _genresCollection = mongoDatabase.GetCollection<Genre>(DbCollections.Genres);
        }

        public async Task<List<Genre>> GetAsync() =>
            await _genresCollection.Find(_ => true).ToListAsync();

        public async Task<Genre?> GetAsync(string id) =>
            await _genresCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Genre?> GetByNameAsync(string name) =>
            await _genresCollection
                .Find(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();

        public async Task<Genre> CreateAsync(UpsertGenreDto newGenre)
        {
            var existing = await GetByNameAsync(newGenre.Name);

            if (existing != null)
            {
                throw new InvalidOperationException("Genre already exists.");
            }

            var genre = newGenre.MapDtoToModel();
            await _genresCollection.InsertOneAsync(genre);
            return genre;
        }

        public async Task UpdateAsync(string id, UpsertGenreDto updatedGenre)
        {
            var genre = updatedGenre.MapDtoToModel(id);
            await _genresCollection.ReplaceOneAsync(x => x.Id == id, genre);
        }

        public async Task RemoveAsync(string id) =>
            await _genresCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<PaginatedResultDto<Genre>> GetPaginatedAsync(GenreQueryDto query)
        {
            var filter = Builders<Genre>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var regex = new BsonRegularExpression(query.Search, "i");

                filter = Builders<Genre>.Filter.Or(Builders<Genre>.Filter.Regex("name", regex));
            }

            var find = _genresCollection.Find(filter);
            var total = await find.CountDocumentsAsync();

            var results = query.FetchAll
                ? await find.ToListAsync()
                : await find.Skip((query.Page - 1) * query.PageSize)
                    .Limit(query.PageSize)
                    .ToListAsync();

            var result = new PaginatedResultDto<Genre>
            {
                Results = results,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            return result;
        }

        public async Task<List<IdName>> GetIdNamesAsync(List<string> ids)
        {
            return await _genresCollection
                .Find(Builders<Genre>.Filter.In(g => g.Id, ids))
                .Project(g => new IdName { Id = g.Id!, Name = g.Name })
                .ToListAsync();
        }
    }
}
