using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Models;
using System.Security.Cryptography;

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
                    //x.IsExpired == false &&
                    x.ValidTime > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (existToken == null) throw new Exception("Token not found");

            return existToken.UserId;
        }
        public async Task CreateToken(string token, string userId, string ip)
        {
            var nweRefreshToken = new RefreshToken
            {
                Token = token,
                Ip = ip,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ValidTime = DateTime.UtcNow.AddDays(15)
            };
            await _refreshCollection.InsertOneAsync(nweRefreshToken);
        }

        public async Task Invoketoken(string token, string ip)
        {
            var filter = Builders<RefreshToken>.Filter.And(Builders<RefreshToken>.Filter.Eq(x => x.Token, token),
    Builders<RefreshToken>.Filter.Eq(x => x.Ip, ip));

            var update = Builders<RefreshToken>.Update.Set(x => x.IsExpired, true);
            await _refreshCollection.UpdateOneAsync(filter, update);
        }

    }
}
