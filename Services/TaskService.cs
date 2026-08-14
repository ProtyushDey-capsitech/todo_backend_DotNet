using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Todo;
using Projects.Models;
using System.Net.Mime;
using System.Security.Cryptography;

namespace Projects.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<TaskModel> _task;

        public TaskService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var dataBase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _task = dataBase.GetCollection<TaskModel>(DbCollections.Tasks);
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
            await _task.InsertOneAsync(newTask);
            return newTask.Id!;
        }
        public async Task UpdateTaskAsync (string id, UpdateTask dto, string pid, string uId)
        {
            var filter = Builders<TaskModel>.Filter.And(Builders<TaskModel>.Filter.Eq(x => x.UserId, uId),
                Builders<TaskModel>.Filter.Eq(x => x.Id, id),
                Builders<TaskModel>.Filter.Eq(x => x.ProjectId, pid));

            var update = Builders<TaskModel>.Update.Set(x => x.Name, dto.Name).Set(x => x.Desc, dto.Desc).Set(x => x.Priority, dto.Priority);
            await _task.UpdateOneAsync(filter, update);
        }

        public async Task UpdateAsync(string id, string userId ,string status)
        {
            var a = await _task.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (a == null) throw new Exception("task not found");
            if (a.UserId != userId) throw new Exception("unauthorize to edit");
            var update = Builders<TaskModel>.Update.Set(x => x.Status, status);
            await _task.UpdateOneAsync(x => x.Id == id, update);
        }
        public async Task DeleteAsync(string id)
        {
            await _task.DeleteOneAsync(ta => ta.Id==id);
        }
    }
}
