using Capsitech.OTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Project;
using Projects.Dtos.Task;
using Projects.Dtos.Todo;
using Projects.Models;
using System;
using System.Net.Mime;
using System.Security.Cryptography;

namespace Projects.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<TaskModel> _task;
        private readonly IMongoCollection<ProjectModel> _project;

        public TaskService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var dataBase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _task = dataBase.GetCollection<TaskModel>(DbCollections.Tasks);
            _project = dataBase.GetCollection<ProjectModel>(DbCollections.Project);
        }

        public async Task<List<ResponseAllTask>> getAsync(string userId, projectListReq projectIds, string? name)
        {
            var filter = Builders<TaskModel>.Filter.Eq(x => x.UserId, userId);

            if (!string.IsNullOrEmpty(name))
            {
                filter &= Builders<TaskModel>.Filter.Regex(
                        x => x.Name,
                       new BsonRegularExpression(name)
                );
            }

            filter &= Builders<TaskModel>.Filter.In(x => x.ProjectId, projectIds.ProjectIds);

            var tasks = await _task.Aggregate()
                .Match(filter)
                .Lookup<TaskModel, ProjectModel, TaskLookupProject>(
                _project,
                x => x.ProjectId,
                x => x.Id,
                x => x.project
                )
                //.Unwind("project")
                .Project(x => new TaskwithProject
                {
                    Id = x.Id,
                    projectId = x.ProjectId,
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Priority = x.Priority,
                    projectName = x.project!.First().Name
                })
                .Group(
                    x => x.Status,
                    g => new ResponseAllTask
                    {
                        Status = g.Key,
                        Tasks = g.ToList(),
                        count = g.ToList().Count()
                    }
                )
                .ToListAsync();

            return tasks;
        }

        public async Task<string> CreateAsync(UpdateTask dto, string userId, string pid)
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

        public async Task UpdateTaskAsync(string id, UpdateTask dto, string pid, string uId)
        {
            var filter = Builders<TaskModel>.Filter.And(Builders<TaskModel>.Filter.Eq(x => x.UserId, uId),
                Builders<TaskModel>.Filter.Eq(x => x.Id, id),
                Builders<TaskModel>.Filter.Eq(x => x.ProjectId, pid));

            var update = Builders<TaskModel>.Update.Set(x => x.Name, dto.Name).Set(x => x.Desc, dto.Desc).Set(x => x.Priority, dto.Priority);
            await _task.UpdateOneAsync(filter, update);
        }

        public async Task UpdateAsync(string id, string userId, string status)
        {
            var a = await _task.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (a == null) throw new Exception("task not found");
            if (a.UserId != userId) throw new Exception("unauthorize to edit");
            var update = Builders<TaskModel>.Update.Set(x => x.Status, status);
            await _task.UpdateOneAsync(x => x.Id == id, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _task.DeleteOneAsync(ta => ta.Id == id);
        }

        public async Task<CountTaskDto> CountTask(string userId)
        {
            var filter = Builders<TaskModel>.Filter.Eq(x => x.UserId, userId);

            var TotalTaskPipeline = new EmptyPipelineDefinition<TaskModel>()
                .Count();
            var TotalTaskFacet = AggregateFacet.Create(
                name: "TotalTask",
                pipeline: TotalTaskPipeline);

            var TodoTaskPipeline = new EmptyPipelineDefinition<TaskModel>()
                .Match(x => x.Status == "Todo").Count();
            var TodoTaskFacet = AggregateFacet.Create(
                name: "TodoTask",
                pipeline: TodoTaskPipeline);

            var InprogressTaskPipeline = new EmptyPipelineDefinition<TaskModel>()
                .Match(x => x.Status == "Inprogress").Count();
            var InprogressTaskFacet = AggregateFacet.Create(
                name: "InprogressTask",
                pipeline: InprogressTaskPipeline);

            //var TotalProjectPipeline = new EmptyPipelineDefinition<TaskModel>()
            //    .Lookup<string, ProjectModel, TaskLookupProject>(
            //    _project,
            //    x => userId,
            //    x => x.UserId,
            //    x => x.project
            //    )
            //    .Unwind(x=>x.project).Count();
            //var TotalProjectFacet = AggregateFacet.Create(
            //    name: "TotalProject",
            //    pipeline: TotalProjectPipeline);


            var result = await _task.Aggregate()
                .Match(filter)
                .Facet(TotalTaskFacet, TodoTaskFacet, InprogressTaskFacet
                //TotalProjectFacet
                )
                .FirstOrDefaultAsync();

            return new CountTaskDto
            {
                //TotalProject = 8,
                TotalTask = result.Facets
                .First(x => x.Name == "TotalTask")
                .Output<AggregateCountResult>()
                .FirstOrDefault()?.Count ?? 0,

                TodoTask = result.Facets
                .First(x => x.Name == "TodoTask")
                .Output<AggregateCountResult>()
                .FirstOrDefault()?.Count ?? 0,

                InprogressTask = result.Facets
                .First(x => x.Name == "InprogressTask")
                .Output<AggregateCountResult>()
                .FirstOrDefault()?.Count ?? 0,
                TotalProject=4
                //TotalProject = result.Facets
                //.First(x => x.Name == "TotalProject")
                //.Output<AggregateCountResult>()
                //.FirstOrDefault()?.Count ?? 0
            };
        }

        public async Task<Dictionary<string, int>> GetTaskCountByStatusAsync(string projectId, string userId)
        {
            var result = await _task.Aggregate()
                .Match(x => x.ProjectId == projectId && x.UserId == userId)
                .Group(
                    x => x.Status,
                    g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                .ToListAsync();

            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<List<ResponseStatusCount>> GetCountStatusAsync(string userId)
        {
            var filter = Builders<TaskModel>.Filter.Eq(x => x.UserId, userId);
            var tasks = await _task.Aggregate()
                .Match(filter)
                .Project(x => new TaskwithProject
                {
                    Id = x.Id,
                    projectId = x.ProjectId,
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Priority = x.Priority,
                    //projectName = x.project.First().Name
                })
                .Group(
                    x => x.Status,
                    g => new ResponseStatusCount
                    {
                        Status = g.Key,
                        count = g.ToList().Count()
                    }
                )
                .ToListAsync();

            return tasks;
        }

        public async Task<List<ResponsePriorityCount>> GetCountPriorityAsync(string userId)
        {
            var filter = Builders<TaskModel>.Filter.Eq(x => x.UserId, userId);
            var tasks = await _task.Aggregate()
                .Match(filter)
                .Project(x => new TaskwithProject
                {
                    Id = x.Id,
                    projectId = x.ProjectId,
                    Name = x.Name,
                    Desc = x.Desc,
                    Status = x.Status,
                    Priority = x.Priority,
                    //projectName = x.project.First().Name
                })
                .Group(
                    x => x.Priority,
                    g => new ResponsePriorityCount
                    {
                        priority = g.Key,
                        count = g.ToList().Count()
                    }
                )
                .ToListAsync();

            return tasks;
        }

        public async Task<List<TaskwithProject>> GetRecentTask(string userId)
        {
            var filter = Builders<TaskModel>.Filter.Eq(x => x.UserId, userId);
            var tasks = await _task.Aggregate()
                .Match(filter)
                .SortByDescending(x => x.CreatedAt)
                .Limit(5)
                .Lookup<TaskModel, ProjectModel, TaskLookupProject>(
                _project,
                x => x.ProjectId,
                x => x.Id,
                x => x.project)
                .Unwind(x => x.project)
                .As<TaskUnwindProject>()
                .Project(x => new TaskwithProject
                {
                     Id = x.Id,
                     Name = x.Name,
                     Desc = x.Desc,
                     Status = x.Status,
                     Priority = x.Priority,
                        projectId = x.ProjectId,
                     projectName = x.project!.Name
                })
                .ToListAsync();

            return tasks;
        }
    }

}
