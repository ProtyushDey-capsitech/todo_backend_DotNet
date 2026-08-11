using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Todo;
using Projects.Models;
using System.Net.Mime;

namespace Projects.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<TaskModel> _todo;

        public TaskService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var dataBase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _todo = dataBase.GetCollection<TaskModel>(DbCollections.Tasks);
        }

        //public async Task<List<ResponseTodoData>> getAsync(string userId, PaginatedQueryDto dto)
        //{
        //    int skip = (dto.Page - 1) * dto.PageSize;
        //    return await _todo.Find(x => x.UserId == userId)
        //    .Skip(skip)
        //    .Limit(dto.PageSize)
        //    .Project(x => new ResponseTodoData
        //    {
        //        IsDone = x.IsDone,
        //        Id = x.Id,
        //        Status = x.Status,
        //        Desc = x.Desc
        //    }).ToListAsync();
        //}

        public async Task<string> CreateAsync(UpdateTask dto, string userId , string pid)
        {
            var newTask = new TaskModel
            {
                Name = dto.Name,
                Desc = dto.Desc,
                ProjectId = pid,
                UserId = userId,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
            };
            await _todo.InsertOneAsync(newTask);
            return newTask.Id;
        }
        //public async Task UpdateWorkAsync(string id, UpdateTodo dto, string userid)
        //{
        //    var update = Builders<TodoModel>.Update.Set(x => x.Status, dto.Status).Set(x => x.Desc, dto.Desc);
        //    var res = await _todo.UpdateOneAsync(x => x.Id == id && x.UserId == userid, update);
        //    if (res.MatchedCount == 0) throw new Exception("task not found or Unauthorizd");
        //}

        //public async Task UpdateAsync (string id, string userId)
        //{
        //    var a = await _todo.Find(x => x.Id == id).FirstOrDefaultAsync();
        //    if(a==null) throw new Exception("task not found");
        //    if (a.UserId != userId) throw new Exception("unauthorize to edit");
        //    var update = Builders<TodoModel>.Update.Set(x=>x.IsDone, !a.IsDone);
        //    await _todo.UpdateOneAsync(x => x.Id == id, update);
        //}
        //public async Task DeleteAsync(string id, string userId)
        //{
        //    TodoModel todo = await _todo.Find(x => x.Id == id).FirstOrDefaultAsync();
        //    if (todo.UserId != userId) throw new Exception("unauthorize to delete");
        //    await _todo.DeleteOneAsync(x => x.Id == id);
        //}
    }
}
