using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Todo;
using Projects.Models;
using System.Net.Mime;

namespace Projects.Services
{
    public class TodoService
    {
        private readonly IMongoCollection<Todo> _todo;

        public TodoService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var dataBase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _todo = dataBase.GetCollection<Todo>(DbCollections.Todos);
        }

        public async Task<List<ResponseData>> getAsync(string userId, PaginatedQueryDto dto)
        {
            int skip = (dto.Page - 1) * dto.PageSize;
            return await _todo.Find(x => x.UserId == userId)
            .Skip(skip)
            .Limit(dto.PageSize)
            .Project(x => new ResponseData
            {
                IsDone = x.IsDone,
                Id = x.Id,
                Status = x.Status,
                Desc = x.Desc
            }).ToListAsync();
        }

        public async Task<string> CreateAsync(UpsertTodoDto dto, string userId)
        {
            var newTodo = new Todo
            {
                Desc = dto.Desc,
                IsDone = dto.IsDone,
                Status = dto.Status,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
            await _todo.InsertOneAsync(newTodo);
            return newTodo.Id;
        }
        public async Task UpdateWorkAsync(string id, UpdateTodo dto , string userid){
            var update = Builders<Todo>.Update.Set(x => x.Status, dto.Status).Set(x => x.Desc, dto.Desc);
            var res =await _todo.UpdateOneAsync(x => x.Id == id && x.UserId == userid, update);
            if (res.MatchedCount == 0) throw new Exception("task not found or Unauthorizd");
        }

        public async Task UpdateAsync (string id, string userId)
        {
            var a = await _todo.Find(x => x.Id == id).FirstOrDefaultAsync();
            if(a==null) throw new Exception("task not found");
            if (a.UserId != userId) throw new Exception("unauthorize to edit");
            var update = Builders<Todo>.Update.Set(x=>x.IsDone, !a.IsDone);
            await _todo.UpdateOneAsync(x => x.Id == id, update);
        }
        public async Task DeleteAsync(string id, string userId)
        {
            Todo todo = await _todo.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (todo.UserId != userId) throw new Exception("unauthorize to delete");
            await _todo.DeleteOneAsync(x => x.Id == id);
        }
    }
}
