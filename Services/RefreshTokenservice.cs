using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Models;

namespace Projects.Services
{
    public class RefreshTokenservice
    {
        private readonly IMongoCollection<RefreshToken> _refreshCollection;

        public RefreshTokenservice(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

            _refreshCollection = mongoDatabase.GetCollection<RefreshToken>(DbCollections.Refresh);
        }

        public async Task<string> CheakToken(string token, string ip)
        {
            var existToken = await _refreshCollection
                .Find(x =>
                    x.Token == token &&
                    x.Ip == ip &&
                    x.ValidTime > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (existToken == null) throw new Exception("user not found");

            return existToken.UserId;
        }
        public async Task createToken(string token, string userId, string ip)
        {
            var nweRefreshToken = new RefreshToken
            {
                Token = token,
                Ip = ip,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ValidTime = DateTime.UtcNow.AddDays(5)
            };
            await _refreshCollection.InsertOneAsync(nweRefreshToken);
        }

    }
}
